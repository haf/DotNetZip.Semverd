// Compatibility.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa .
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-August-05 18:32:33>
//
// ------------------------------------------------------------------
//
// This module defines the tests for compatibility for DotNetZip.  The
// idea is to verify that DotNetZip can read the zip files produced by
// other tools, and that other tools can read the output produced
// by DotNetZip. The tools and libraries tested are:
//  - WinZip
//  - 7zip
//  - Infozip (unzip 6.0, zip 3.0)
//  - Perl's IO::Compress
//  - zipfldr.dll (via script)
//  - the Visual Studio DLL
//  - MS-Word
//
// ------------------------------------------------------------------


using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;
using System.IO;


namespace Ionic.Zip.Tests
{
    /// <summary>
    /// Summary description for Compatibility
    /// </summary>
    [TestClass]
    public class Compatibility : IonicTestClass
    {
        EncryptionAlgorithm[] crypto =
        {
            EncryptionAlgorithm.None,
            EncryptionAlgorithm.PkzipWeak,
            EncryptionAlgorithm.WinZipAes128,
            EncryptionAlgorithm.WinZipAes256,
        };

        Ionic.Zlib.CompressionLevel[] compLevels =
            {
                Ionic.Zlib.CompressionLevel.None,
                Ionic.Zlib.CompressionLevel.BestSpeed,
                Ionic.Zlib.CompressionLevel.Default,
                Ionic.Zlib.CompressionLevel.BestCompression,
            };


        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            // get the path to the DotNetZip DLL
            string SourceDir = System.IO.Directory.GetCurrentDirectory();
            for (int i = 0; i < 3; i++)
                SourceDir = Path.GetDirectoryName(SourceDir);

            IonicZipDll = Path.Combine(SourceDir, "Zip\\bin\\Debug\\Ionic.Zip.dll");

            Assert.IsTrue(File.Exists(IonicZipDll), "DLL ({0}) does not exist", IonicZipDll);

            // register it for COM interop
            string output;

            int rc = TestUtilities.Exec_NoContext(RegAsm, String.Format("\"{0}\" /codebase /verbose", IonicZipDll), out output);
            if (rc != 0)
            {
                string cmd = String.Format("{0} \"{1}\" /codebase /verbose", RegAsm, IonicZipDll);
                throw new Exception(String.Format("Failed to register DotNetZip with COM rc({0}) cmd({1}) out({2})", rc, cmd, output));
            }
        }


        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            string output;
            // unregister the DLL for COM interop
            int rc = TestUtilities.Exec_NoContext(RegAsm, String.Format("\"{0}\" /unregister /verbose", IonicZipDll), out output);
            if (rc != 0)
                throw new Exception(String.Format("Failed to unregister DotNetZip with COM  rc({0}) ({1})", rc, output));
        }


        private static string IonicZipDll;
        private static string RegAsm = "c:\\windows\\Microsoft.NET\\Framework\\v2.0.50727\\regasm.exe";



        private System.Reflection.Assembly _myself;
        private System.Reflection.Assembly myself
        {
            get
            {
                if (_myself == null)
                {
                    _myself = System.Reflection.Assembly.GetExecutingAssembly();
                }
                return _myself;
            }
        }


        private string _windir = null;
        private string windir
        {
            get
            {
                if (_windir == null)
                {
                    _windir = System.Environment.GetEnvironmentVariable("Windir");
                    Assert.IsTrue(Directory.Exists(_windir), "%windir% does not exist ({0})", _windir);
                }
                return _windir;
            }
        }



        private string _perl = null;
        private string perl
        {
            get
            {
                if (_perl == null)
                {
                    var sysPath = Environment.GetEnvironmentVariable("Path");
                    var pathElts = sysPath.Split(';');
                    foreach (var elt in pathElts)
                    {
                        var putative = Path.Combine(elt, "perl.exe");
                        if (File.Exists(putative))
                        {
                            _perl = putative;
                            break;
                        }
                    }
                    Assert.IsTrue(File.Exists(_perl), "Cannot find perl.exe");
                }
                return _perl;
            }
        }



        private string _cscriptExe = null;
        private string cscriptExe
        {
            get
            {
                if (_cscriptExe == null)
                {
                    _cscriptExe = Path.Combine(Path.Combine(windir, "system32"), "cscript.exe");
                    Assert.IsTrue(File.Exists(_cscriptExe), "cscript.exe does not exist ({0})", _cscriptExe);
                }
                return _cscriptExe;
            }
        }


        private string GetScript(string scriptName)
        {
            // check existence of script and script engine
            string testBin = TestUtilities.GetTestBinDir(CurrentDir);
            string resourceDir = Path.Combine(testBin, "Resources");
            string script = Path.Combine(resourceDir, scriptName);
            Assert.IsTrue(File.Exists(script), "script ({0}) does not exist", script);
            return script;
        }

        private void VerifyFileTimes(string extractDir,
                                     IEnumerable<String> filesToCheck,
                                     bool applyShellAllowance,
                                     bool checkNtfsTimes,
                                     int thresholdNanoseconds)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("Verify file times...");
            TimeSpan threshold = new TimeSpan(thresholdNanoseconds);
            TestContext.WriteLine("Using threshold: ({0})", threshold.ToString());

            foreach (var fqPath in filesToCheck)
            {
                var f = Path.GetFileName(fqPath);
                var extractedFile = Path.Combine(extractDir, f);
                Assert.IsTrue(File.Exists(extractedFile), "File does not exist ({0})", extractedFile);

                // check times
                DateTime t1 = File.GetLastWriteTimeUtc(fqPath);
                DateTime t2 = File.GetLastWriteTimeUtc(extractedFile);
                TestContext.WriteLine("{0} lastwrite orig({1})  extracted({2})",
                                      Path.GetFileName(fqPath),
                                      t1.ToString("G"),
                                      t2.ToString("G"));

                TimeSpan delta = (t1 > t2) ? t1 - t2 : t2 - t1;
                if (checkNtfsTimes)
                {
                    Assert.AreEqual<DateTime>(t1, t2, "LastWriteTime delta actual({0}) expected({1})", delta.ToString(), threshold.ToString());
                    t1 = File.GetCreationTimeUtc(fqPath);
                    t2 = File.GetCreationTimeUtc(extractedFile);
                    delta = (t1 > t2) ? t1 - t2 : t2 - t1;
                    Assert.IsTrue(delta <= threshold, "CreationTime delta actual({0}) expected({1})", delta.ToString(), threshold.ToString());
                }
                else
                {
                    if (applyShellAllowance)
                    {
                        if (delta > threshold)
                        {
                            // In some cases - specifically when the file lastmod time
                            // is on the other side of a DST event - extracting with the
                            // shell gets a time on the extracted file that is 1 hour
                            // off the expected value. This doesn't happen when WinZip
                            // or DotNetZip is used for extraction - only when using the
                            // shell extension.  In those cases we can allow for the
                            // extra hour.
                            TestContext.WriteLine("Adjusting delta for shell allowance...");
                            delta -= new TimeSpan(1, 0, 0);  // 1 hour
                        }
                    }

                    Assert.IsTrue(delta <= threshold,
                                  "LastWriteTime delta actual({0}) expected({1})",
                                  delta.ToString(),
                                  threshold.ToString());
                }
            }
        }


        private void VerifyTimesUnix(string extractDir, IEnumerable<String> filesToCheck)
        {
            VerifyFileTimes(extractDir, filesToCheck, true, false,
                            10000 * 1000); // 1 second for unix
        }

        private void VerifyTimesNtfs(string extractDir, IEnumerable<String> filesToCheck)
        {
            VerifyFileTimes(extractDir, filesToCheck, true, false,
                            100 * 1000);  // default 0.01s  for NTFS
        }

        private void VerifyTimesDos(string extractDir, IEnumerable<String> filesToCheck)
        {
            VerifyFileTimes(extractDir, filesToCheck, false, false,
                            20000 * 1000);  // 2 seconds for DOS times

        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Error_ZipFile_Initialize_Error()
        {
            string notaZipFile = GetScript("VbsUnzip-ShellApp.vbs");

            // try to read a bogus zip archive
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Initialize(notaZipFile);
            }
        }




        [TestMethod]
        public void ShellApplication_Unzip()
        {
            // get a set of files to zip up
            string subdir = Path.Combine(TopLevelDir, "files");
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            var script = GetScript("VbsUnzip-ShellApp.vbs");

            int i = 0;
            foreach (var compLevel in compLevels)
            {
                // create and fill the directories
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("ShellApplication_Unzip.{0}.zip", i));
                string extractDir = Path.Combine(TopLevelDir, String.Format("extract.{0}", i));

                // Create the zip archive
                //Directory.SetCurrentDirectory(TopLevelDir);
                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.CompressionLevel = (Ionic.Zlib.CompressionLevel)compLevel;
                    //zip.StatusMessageTextWriter = System.Console.Out;
                    for (int j = 0; j < filesToZip.Length; j++)
                        zip1.AddItem(filesToZip[j], "files");
                    zip1.Save(zipFileToCreate);
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                     "Incorrect number of entries in the zip file.");

                // run the unzip script
                this.Exec(cscriptExe,
                          String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, extractDir));

                // check the files in the extract dir
                VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

                // verify the file times
                VerifyTimesDos(Path.Combine(extractDir, "files"), filesToZip);
                i++;
            }
        }


        [TestMethod]
        public void ShellApplication_Unzip_NonSeekableOutput()
        {
            // get a set of files to zip up
            string subdir = Path.Combine(TopLevelDir, "files");
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            var script = GetScript("VbsUnzip-ShellApp.vbs");

            int i = 0;
            foreach (var compLevel in compLevels)
            {
                // create and fill the directories
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("ShellApplication_Unzip_NonSeekableOutput.{0}.zip", i));
                string extractDir = Path.Combine(TopLevelDir, String.Format("extract.{0}", i));

                // Create the zip archive
                //Directory.SetCurrentDirectory(TopLevelDir);

                // Want to test the library when saving to non-seekable output streams.  Like
                // stdout or ASPNET's Response.OutputStream.  This simulates it.
                using (var rawOut = System.IO.File.Create(zipFileToCreate))
                {
                    using (var nonSeekableOut = new Ionic.Zip.Tests.NonSeekableOutputStream(rawOut))
                    {
                        using (ZipFile zip1 = new ZipFile())
                        {
                            zip1.CompressionLevel = (Ionic.Zlib.CompressionLevel)compLevel;
                            for (int j = 0; j < filesToZip.Length; j++)
                                zip1.AddItem(filesToZip[j], "files");
                            zip1.Save(nonSeekableOut);
                        }
                    }
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                     "Incorrect number of entries in the zip file.");

                // run the unzip script
                this.Exec(cscriptExe,
                          String.Format("\"{0}\" {1} {2}", script,
                          Path.GetFileName(zipFileToCreate),
                          Path.GetFileName(extractDir)));

                // check the files in the extract dir
                VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

                VerifyFileTimes(Path.Combine(extractDir, "files"), filesToZip,
                                false, false, 20000 * 1000);  // 2s threshold for DOS times
                i++;
            }
        }


#if SHELLAPP_UNZIP_SFX

        [TestMethod]
        public void ShellApplication_Unzip_SFX()
        {
            // get a set of files to zip up
            string subdir = Path.Combine(TopLevelDir, "files");
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            var script = GetScript("VbsUnzip-ShellApp.vbs");

            int i=0;
            foreach (var compLevel in compLevels)
            {
                // create and fill the directories
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("ShellApp_Unzip_SFX.{0}.exe", i));
                string extractDir = Path.Combine(TopLevelDir, String.Format("extract.{0}",i));

                // Create the zip archive
                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.CompressionLevel = (Ionic.Zlib.CompressionLevel) compLevel;
                    //zip.StatusMessageTextWriter = System.Console.Out;
                    for (int j = 0; j < filesToZip.Length; j++)
                        zip1.AddItem(filesToZip[j], "files");
                    zip1.SaveSelfExtractor(zipFileToCreate, SelfExtractorFlavor.ConsoleApplication);
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                     "Incorrect number of entries in the zip file.");

                // run the unzip script
                this.Exec(cscriptExe,
                          String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, extractDir));

                // check the files in the extract dir
                VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

                // verify the file times
                VerifyTimesDos(Path.Combine(extractDir, "files"), filesToZip);
                i++;
            }
        }
#endif



        [TestMethod]
        public void ShellApplication_Unzip_2()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "ShellApplication_Unzip-2.zip");
            // create and fill the directories
            string extractDir = Path.Combine(TopLevelDir, "extract");
            var checksums = new Dictionary<string, byte[]>();
            var filesToZip = GetSelectionOfTempFiles(_rnd.Next(13) + 8, checksums);

            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFiles(filesToZip, "files");
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Count,
                                 "Incorrect number of entries in the zip file.");

            // run the unzip script
            string script = GetScript("VbsUnzip-ShellApp.vbs");

            this.Exec(cscriptExe,
                      String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, extractDir));

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

            #if IN_A_SANE_WORLD
            // !!
            // I think the file times get messed up using the Shell to unzip.
            // !!

            // verify the file times
            VerifyTimesDos(Path.Combine(extractDir, "files"), filesToZip);
            #endif
        }



        [TestMethod]
        public void ShellApplication_SelectedFiles_Unzip()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "ShellApplication_SelectedFiles_Unzip.zip");

            TestContext.WriteLine("ZipFile version:  {0}", ZipFile.LibraryVersion);

            // create and fill the directories
            string extractDir = "extract";
            string dirToZip = "files";
            TestContext.WriteLine("creating dir '{0}' with files", dirToZip);
            Directory.CreateDirectory(dirToZip);

            int numFilesToAdd = _rnd.Next(5) + 6;
            int numFilesAdded = 0;
            int baseSize = _rnd.Next(0x100ff) + 8000;
            int nFilesInSubfolders = 0;
            Dictionary<string, byte[]> checksums = new Dictionary<string, byte[]>();
            var flist = new List<string>();
            for (int i = 0; i < numFilesToAdd && nFilesInSubfolders < 2; i++)
            {
                string fileName = string.Format("Test{0}.txt", i);
                if (i != 0)
                {
                    int x = _rnd.Next(4);
                    if (x != 0)
                    {
                        string folderName = string.Format("folder{0}", x);
                        fileName = Path.Combine(folderName, fileName);
                        if (!Directory.Exists(Path.Combine(dirToZip, folderName)))
                            Directory.CreateDirectory(Path.Combine(dirToZip, folderName));
                        nFilesInSubfolders++;
                    }
                }
                fileName = Path.Combine(dirToZip, fileName);
                TestUtilities.CreateAndFillFileBinary(fileName, baseSize + _rnd.Next(28000));
                var key = Path.GetFileName(fileName);
                var chk = TestUtilities.ComputeChecksum(fileName);
                checksums.Add(key, chk);
                flist.Add(fileName);
                numFilesAdded++;
            }

            // Create the zip archive
            var sw = new System.IO.StringWriter();
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.StatusMessageTextWriter = sw;
                //zip1.StatusMessageTextWriter = Console.Out;
                zip1.AddSelectedFiles("*.*", dirToZip, "", true);
                zip1.Save(zipFileToCreate);
            }
            TestContext.WriteLine(sw.ToString());


            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), numFilesAdded,
                                 "Incorrect number of entries in the zip file.");

            // run the unzip script
            string script = GetScript("VbsUnzip-ShellApp.vbs");

            this.Exec(cscriptExe,
                      String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, Path.Combine(TopLevelDir, extractDir)));

            // check the files in the extract dir
            foreach (var fqPath in flist)
            {
                var f = Path.GetFileName(fqPath);
                var extractedFile = fqPath.Replace("files", "extract");
                Assert.IsTrue(File.Exists(extractedFile), "File does not exist ({0})", extractedFile);
                var chk = TestUtilities.ComputeChecksum(extractedFile);
                Assert.AreEqual<String>(TestUtilities.CheckSumToString(checksums[f]),
                                        TestUtilities.CheckSumToString(chk),
                                        String.Format("Checksums for file {0} do not match.", f));
                checksums.Remove(f);
            }

            Assert.AreEqual<Int32>(0, checksums.Count, "Not all of the expected files were found in the extract directory.");
        }




        [TestMethod]
        public void ShellApplication_Zip()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "ShellApplication_Zip.zip");
            //Directory.SetCurrentDirectory(TopLevelDir);

            string subdir = Path.Combine(TopLevelDir, "files");
            string extractDir = "extract";

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive via script
            string script = GetScript("VbsCreateZip-ShellApp.vbs");

            this.Exec(cscriptExe,
                      String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, subdir));

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unzip
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                zip1.ExtractAll(extractDir);
            }

            // check the files in the extract dir
            VerifyChecksums(extractDir, filesToZip, checksums);

            VerifyTimesDos(extractDir, filesToZip);
        }


        [TestMethod]
        public void ShellApplication_Zip_2()
        {
            string zipFileToCreate = "ShellApplication_Zip.zip";
            string subdir = "files";
            string extractDir = "extract";

            TestContext.WriteLine("======================================================");

            Dictionary<string, byte[]> checksums = new Dictionary<string, byte[]>();
            var filesToZip = GetSelectionOfTempFiles(_rnd.Next(33) + 11, checksums);

            Directory.CreateDirectory(subdir);
            Directory.SetCurrentDirectory(subdir);
            var w = System.Environment.GetEnvironmentVariable("Windir");
            Assert.IsTrue(Directory.Exists(w), "%windir% does not exist ({0})", w);
            var fsutil = Path.Combine(Path.Combine(w, "system32"), "fsutil.exe");
            Assert.IsTrue(File.Exists(fsutil), "fsutil.exe does not exist ({0})", fsutil);
            string ignored;

            TestContext.WriteLine("validating the list of files...");
            List<String> markedForRemoval = new List<String> ();
            // remove those with spaces in the names.  The ShellApp (zipfldr.dll) doesn't
            // deal with these files very well.  Or something.
            foreach (var f in filesToZip)
            {
                if (Path.GetFileName(f).IndexOf(' ') > 0)
                    markedForRemoval.Add(f);
            }

            foreach (var f in markedForRemoval)
            {
                TestContext.WriteLine("removing  {0}...", Path.GetFileName(f));
                filesToZip.Remove(f);
                checksums.Remove(Path.GetFileName(f));
            }

            TestContext.WriteLine("--------------------------------------------");
            TestContext.WriteLine("creating links...");
            foreach (var f in filesToZip)
            {
                string shortfile= Path.GetFileName(f);
                Assert.IsTrue(File.Exists(f));
                string cmd = String.Format("hardlink create \"{0}\" \"{1}\"", shortfile, f);
                TestUtilities.Exec_NoContext(fsutil, cmd, out ignored);
            }

            TestContext.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            // Create the zip archive via script
            Directory.SetCurrentDirectory(TopLevelDir);
            string script = GetScript("VbsCreateZip-ShellApp.vbs");

            this.Exec(cscriptExe,
                      String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, subdir));

            // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // DEBUGGING!
            if (TestUtilities.CountEntries(zipFileToCreate) != filesToZip.Count)
            {
                string[] linkedFiles = Directory.GetFiles(subdir);

                Action<IEnumerable<String>, string> ListFiles = (list, name) =>
                {
                    TestContext.WriteLine("**********************************");
                    TestContext.WriteLine("files in ({0})", name);
                    foreach (var s in list)
                    {
                        TestContext.WriteLine("  {0}", Path.GetFileName(s));
                    }
                    TestContext.WriteLine("----------------------------------");
                    TestContext.WriteLine("  {0} total files", list.Count());
                };

                ListFiles(linkedFiles, "Linked Files");
                ListFiles(filesToZip, "selected Files");

                IEnumerable<String> selection = null;
                using (var zip = ZipFile.Read(zipFileToCreate))
                {
                    selection = from e in zip.Entries select e.FileName;
                }

                ListFiles(selection, "zipped Files");

                foreach (var file in linkedFiles)
                {
                    if (!selection.Contains(Path.GetFileName(file)))
                    {
                        TestContext.WriteLine("Missing: {0}", Path.GetFileName(file));
                    }
                }
            }
            // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Count,
                                 "Incorrect number of entries in the zip file.");

            // unzip
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                zip1.ExtractAll(extractDir);
            }

            // check the files in the extract dir
            VerifyChecksums(extractDir, filesToZip, checksums);

            VerifyTimesDos(extractDir, filesToZip);
        }



        [TestMethod]
        public void VStudio_Zip()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "VStudio_Zip.zip");
            string subdir = Path.Combine(TopLevelDir, "files");
            string extractDir = "extract";

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            //Directory.SetCurrentDirectory(TopLevelDir);

            String[] a = Array.ConvertAll(filesToZip, x => Path.GetFileName(x));
            Microsoft.VisualStudio.Zip.ZipFileCompressor zfc = new Microsoft.VisualStudio.Zip.ZipFileCompressor(zipFileToCreate, "files", a, true);

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unzip
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                zip1.ExtractAll(extractDir);
            }

            // check the files in the extract dir
            VerifyChecksums(extractDir, filesToZip, checksums);

            // visual Studio's ZIP library doesn't bother with times...
            //VerifyNtfsTimes(extractDir, filesToZip);
        }



        [TestMethod]
        [Timeout(3 * 60 * 1000)]  // timeout in ms.
        public void VStudio_UnZip()
        {
            string zipFileToCreate = "VStudio_UnZip.zip";
            string shortDir = "files";
            string subdir = Path.Combine(TopLevelDir, shortDir);
            string extractDir = "extract";

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < filesToZip.Length; i++)
                    zip1.AddItem(filesToZip[i], shortDir);
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unzip
            var decompressor = new Microsoft.VisualStudio.Zip.ZipFileDecompressor(zipFileToCreate, false, true, false);
            decompressor.UncompressToFolder(extractDir, false);

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, shortDir), filesToZip, checksums);

            // visual Studio's ZIP library doesn't bother with times...
            //VerifyNtfsTimes(Path.Combine(extractDir, "files"), filesToZip);
        }



        [TestMethod]
        public void COM_Zip()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "COM_Zip.zip");
            string shortDir = "files";
            string subdir = Path.Combine(TopLevelDir, shortDir);
            string extractDir = "extract";

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // run the COM script to create the ZIP archive
            string script = GetScript("VbsCreateZip-DotNetZip.vbs");

            this.Exec(cscriptExe,
                      String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, subdir));

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unzip
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                zip1.ExtractAll(extractDir);
            }

            // check the files in the extract dir
            VerifyChecksums(extractDir, filesToZip, checksums);

            VerifyTimesNtfs(extractDir, filesToZip);
        }



        [TestMethod]
        public void COM_Unzip()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "COM_Unzip.zip");

            // construct the directories
            //string ExtractDir = Path.Combine(TopLevelDir, "extract");
            string extractDir = "extract";
            string shortDir = "files";
            string subdir = Path.Combine(TopLevelDir, shortDir);

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < filesToZip.Length; i++)
                    zip1.AddItem(filesToZip[i], shortDir);
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");


            // run the COM script to unzip the ZIP archive
            string script = GetScript("VbsUnzip-DotNetZip.vbs");
            this.Exec(cscriptExe,
                      String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, extractDir));

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, shortDir), filesToZip, checksums);

            VerifyTimesNtfs(Path.Combine(extractDir, shortDir), filesToZip);
        }


        [TestMethod]
        public void COM_Check()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "COM_Check.zip");

            // create and fill the directories
            string subdir = Path.Combine(TopLevelDir, "files");
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < filesToZip.Length; i++)
                    zip1.AddItem(filesToZip[i], "files");
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // run the COM script to check the ZIP archive
            string script = GetScript("TestCheckZip.js");

            string testOut = this.Exec(cscriptExe,
                                      String.Format("\"{0}\" {1}", script, zipFileToCreate));

            Assert.IsTrue(testOut.StartsWith("That zip is OK"));
        }


        [TestMethod]
        public void COM_CheckWithExtract()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "COM_CheckWithExtract.zip");

            // create and fill the directories
            string subdir = Path.Combine(TopLevelDir, "files");
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < filesToZip.Length; i++)
                    zip1.AddItem(filesToZip[i], "files");
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // run the COM script to check and test-extract the ZIP archive
            string script = GetScript("TestCheckZip.js");

            string testOut = this.Exec(cscriptExe,
                                      String.Format("\"{0}\" -x {1}", script, zipFileToCreate));

            Assert.IsTrue(testOut.StartsWith("That zip is OK"), "output: {0}", testOut);
        }


        [TestMethod]
        public void COM_CheckError()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            // run the COM script to check the (not) ZIP archive
            string script = GetScript("TestCheckZip.js");

            string testOut = this.Exec(cscriptExe,
                                      String.Format("\"{0}\" {1}", script, cscriptExe));

            Assert.IsTrue(testOut.StartsWith("That zip is not OK"));
        }

        [TestMethod]
        public void COM_CheckPassword()
        {
            // create and fill the directories
            string subdir = Path.Combine(TopLevelDir, "files");
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // first case - all entries have same password - should pass the check.
            // second case - last entry uses a different password - should fail the check.
            for (int k=0; k < 2; k++)
            {
                string password = GeneratePassword(11);
                string zipFileToCreate= String.Format("COM_CheckPass-{0}.zip", k);
                zipFileToCreate = Path.Combine(TopLevelDir, zipFileToCreate);
                // Create the zip archive
                using (ZipFile zip1 = new ZipFile())
                {
                    //zip1.Password = password;
                    for (int i = 0; i < filesToZip.Length; i++)
                    {
                        var e = zip1.AddFile(filesToZip[i], "files");
                        e.Password = (k == 1 && i == filesToZip.Length-1)
                            ? "7"
                            : password;
                    }
                    zip1.Save(zipFileToCreate);
                }

                TestContext.WriteLine("Checking the count...");
                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                     filesToZip.Length,
                                     "Incorrect number of entries in the zip file.");

                TestContext.WriteLine("Checking the password (case {0})...", k);
                string script = GetScript("TestCheckZipPassword.js");
                string testOut = this.Exec(cscriptExe,
                                           String.Format("\"{0}\" {1} {2}",
                                                         script, zipFileToCreate, password));

                if (k==0)
                    Assert.IsTrue(testOut.StartsWith("That zip is OK"));
                else
                    Assert.IsFalse(testOut.StartsWith("That zip is OK"));
            }
        }


        private string GeneratePassword(int n)
        {
            // not good for passwords used on the command line with cmd line tools!!
            // return TestUtilities.GenerateRandomPassword();

            return TestUtilities.GenerateRandomAsciiString(n).Replace(" ","_");
            //return "Alphabet";
        }



        [TestMethod]
        public void InfoZip_Unzip()
        {
            if (!InfoZipIsPresent)
                throw new Exception("[InfoZip_Unzip] : InfoZip is not present");

            string shortDir = "filesToZip";
            string subdir = Path.Combine(TopLevelDir, shortDir);
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            for (int j=0; j < 2; j++)  // crypto.Length
            {
                // Cannot do WinZipAES encryption - not supported by InfoZip
                int i = 0;
                foreach (var compLevel in compLevels)
                {
                    string zipFileToCreate =
                                     String.Format("InfoZip_Unzip.{0}.{1}.zip", i,j);

                    string password = GeneratePassword(9);
                    // Create the zip archive
                    using (ZipFile zip1 = new ZipFile())
                    {
                        zip1.CompressionLevel = (Ionic.Zlib.CompressionLevel)compLevel;
                        if (j!=0)
                        {
                            zip1.Encryption = crypto[j];
                            zip1.Password = password;
                        }
                        for (int n = 0; n < filesToZip.Length; n++)
                            zip1.AddItem(filesToZip[n], shortDir);
                        zip1.Save(zipFileToCreate);
                    }

                    Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                         filesToZip.Length,
                                         "Incorrect number of entries in the zip file"+
                                         " (i,j)=({0},{1}).", i,j);

                    string extractDir = String.Format("extract.{0}.{1}", i,j);

                    if (j==0)
                    {
                        this.Exec(infoZipUnzip,
                                  String.Format("{0} -d {1}",
                                                Path.GetFileName(zipFileToCreate),
                                                Path.GetFileName(extractDir)));
                    }
                    else
                    {
                        this.Exec(infoZipUnzip,
                                  String.Format("-P {0}  {1} -d {2}",
                                                password,
                                                Path.GetFileName(zipFileToCreate),
                                                Path.GetFileName(extractDir)));
                    }

                    var extractedFiles = Directory.GetFiles(Path.Combine(extractDir, shortDir));
                    Assert.AreEqual<int>
                        (filesToZip.Length, extractedFiles.Length,
                         "Incorrect number of extracted files. (i,j)={0},{1}",
                         i,j);

                    VerifyChecksums(Path.Combine(extractDir, shortDir),
                                    filesToZip, checksums);

                    i++;
                }
            }
        }


        [TestMethod]
        public void InfoZip_Zip()
        {
            if (!InfoZipIsPresent)
                throw new Exception("InfoZip is not present");

            // create and fill the directories
            string shortDir = "filesToZip";
            string subdir = Path.Combine(TopLevelDir, shortDir);
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // infozip usage:
            // zip.exe zipfile.zip -r <directory>
            // zip.exe zipfile.zip  <list of files>

            for (int k=0; k < 2; k++)
            {
                string zipFileToCreate = String.Format("InfoZip_Zip-{0}.zip", k);
                string extractDir = "extractDir-" + k;

                var relativePath = Path.GetFileName(subdir);

                if (k==0)
                {
                    // Create the zip archive via Infozip.exe
                    // zip.exe zipfile.zip -r <directory>
                    this.Exec(infoZip, String.Format("{0} -r {1}",
                                                     zipFileToCreate, relativePath));
                }
                else
                {
                    string[] relPathFiles =
                        Array.ConvertAll(filesToZip,
                                         path =>
                                         Path.Combine(relativePath,
                                                      Path.GetFileName(path)));

                    // zip.exe zipfile.zip  <list of files>
                    this.Exec(infoZip, zipFileToCreate + " " +
                              String.Join(" ", relPathFiles));
                }

                // delay a bit between file creation and check/unzip
                System.Threading.Thread.Sleep(1200);

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                     filesToZip.Length,
                                     "Incorrect number of entries in the zip file.");

                // run the COM script to unzip the ZIP archive
                string script = GetScript("VbsUnZip-DotNetZip.vbs");

                this.Exec(cscriptExe,
                          String.Format("\"{0}\" {1} {2}",
                                        script,
                                        Path.GetFileName(zipFileToCreate),
                                        extractDir));
                // check the files in the extract dir
                VerifyChecksums(Path.Combine(extractDir,shortDir), filesToZip, checksums);
                VerifyTimesUnix(Path.Combine(extractDir,shortDir), filesToZip);
            }
        }



        [TestMethod]
        public void InfoZip_Zip_Password()
        {
            if (!InfoZipIsPresent)
                throw new Exception("[InfoZip_Zip_Password] : InfoZip is not present");

            // create and fill the directories
            string extractDir = "extractDir";
            string shortDir = "filesToZip";
            string subdir = Path.Combine(TopLevelDir, shortDir);
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // infozip usage:
            // zip.exe zipfile.zip -r -P <password>  <directory>

            string password = GeneratePassword(9);
            string zipFileToCreate = "InfoZip_Zip_Password.zip";

            // Create the zip archive via Infozip.exe
            this.Exec(infoZip, String.Format("{0} -r -P {1} {2}",
                                             zipFileToCreate,
                                             password,
                                             shortDir));

            // delay a bit between file creation and check/unzip
            System.Threading.Thread.Sleep(1200);

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // extract
            using (var zip = ZipFile.Read(zipFileToCreate))
            {
                zip.Password = password;
                zip.ExtractAll(extractDir);
            }

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, shortDir), filesToZip, checksums);
            VerifyTimesNtfs(Path.Combine(extractDir, shortDir), filesToZip);
        }



        [TestMethod]
        public void InfoZip_Zip_Split()
        {
            if (!InfoZipIsPresent)
                throw new Exception("InfoZip is not present");

            string dirToZip = "dirToZip";
            int numFiles = _rnd.Next(17) + 12;
            string[] filesToZip;
            string msg;
            Dictionary<string, byte[]> checksums;
            int[] segmentSizes = { 256, 512, 1024, 4096, 8192 }; // in kb

            _txrx = TestUtilities.StartProgressMonitor("InfoZip-compat",
                                                       "InfoZip split archives",
                                                       "Creating "+numFiles+" files");
            _txrx.Send("pb 0 max 2");
            _txrx.Send("pb 1 max " + numFiles);

            var update = new Action<int,int,Int64>( (x,y,z) => {
                    switch (x)
                    {
                        case 0:
                        break;
                        case 1:
                        break;
                        case 2:
                        _txrx.Send("pb 1 step");
                        msg = String.Format("status created {0}/{1} files",
                                            y+1,
                                            ((int)z));
                        _txrx.Send(msg);
                        break;
                    }
                });

            CreateLargeFilesWithChecksums(dirToZip, numFiles, update,
                                          out filesToZip, out checksums);

            _txrx.Send("pb 0 step");
            _txrx.Send("pb 1 max " + segmentSizes.Length);
            _txrx.Send("pb 1 value 0");
            for (int i=0; i < segmentSizes.Length; i++)
            {
                _txrx.Send("status zip with " + segmentSizes[i] + "k segments");
                string trialDir = segmentSizes[i] + "k";
                Directory.CreateDirectory(trialDir);
                string zipFileToCreate = Path.Combine(trialDir, trialDir + ".zip");
                // Create the zip archive via Infozip.exe
                this.Exec(infoZip, String.Format("{0} -r -s {1}k -sv {2}",
                                                 zipFileToCreate,
                                                 segmentSizes[i],
                                                 dirToZip));

                string extractDir = segmentSizes[i] + "k.extract";
                using (var zip = ZipFile.Read(zipFileToCreate))
                {
                    zip.ExtractAll(extractDir);
                }

                VerifyChecksums(Path.Combine(extractDir, dirToZip), filesToZip, checksums);

                _txrx.Send("pb 1 step");
            }
        }




#if NOT
        // warning [256k/256k.zip]: zipfile claims to be last disk of a
        // multi-part archive; attempting to process anyway, assuming
        // all parts have been concatenated together in order.  Expect
        // "errors" and warnings...true multi-part support doesn't exist
        // yet (coming soon).

        [TestMethod]
        public void InfoZip_Unzip_Split()
        {
            if (!InfoZipIsPresent)
                throw new Exception("InfoZip is not present");

            string dirToZip = "dirToZip";
            int numFiles = _rnd.Next(17) + 12;
            string[] filesToZip;
            string msg;
            Dictionary<string, byte[]> checksums;
            int[] segmentSizes = { 256, 512, 1024, 4096, 8192 }; // in kb

            _txrx = TestUtilities.StartProgressMonitor("InfoZip-compat",
                                                       "InfoZip split archives",
                                                       "Creating "+numFiles+" files");
            _txrx.Send("pb 0 max 2");
            _txrx.Send("pb 1 max " + numFiles);

            var update = new Action<int,int,Int64>( (x,y,z) => {
                    switch (x)
                    {
                        case 0:
                        break;
                        case 1:
                        break;
                        case 2:
                        _txrx.Send("pb 1 step");
                        msg = String.Format("status created {0}/{1} files",
                                            y+1,
                                            ((int)z));
                        _txrx.Send(msg);
                        break;
                    }
                });

            CreateLargeFilesWithChecksums(dirToZip, numFiles, update,
                                          out filesToZip, out checksums);

            _txrx.Send("pb 0 step");
            _txrx.Send("pb 1 max " + segmentSizes.Length);
            _txrx.Send("pb 1 value 0");
            for (int i=0; i < segmentSizes.Length; i++)
            {
                //Directory.SetCurrentDirectory(TopLevelDir);
                _txrx.Send("status zip with " + segmentSizes[i] + "k segments");
                string trialDir = segmentSizes[i] + "k";
                Directory.CreateDirectory(trialDir);
                string zipFileToCreate = Path.Combine(trialDir, trialDir + ".zip");
                // Create the zip archive via DotNetZip
                using (var zip = new ZipFile())
                {
                    zip.AddFiles(filesToZip);
                    zip.MaxOutputSegmentSize = segmentSizes[i]*1024;
                    zip.Save(zipFileToCreate);
                }

                // extract using InfoZip
                string extractDir = segmentSizes[i] + "k.extract";
                //Directory.SetCurrentDirectory(TopLevelDir);
                this.Exec(infoZipUnzip,
                          String.Format("{0} -d {1}",
                                        zipFileToCreate,
                                        extractDir));

                VerifyChecksums(Path.Combine(extractDir, dirToZip), filesToZip, checksums);

                _txrx.Send("pb 1 step");
            }
        }
#endif


        [TestMethod]
        public void InfoZip_Unzip_z64_wi11936()
        {
            if (!InfoZipIsPresent)
                throw new Exception("InfoZip is not present");

            string dirToZip = "dirToZip";
            int numFiles = _rnd.Next(17) + 12;
            string[] filesToZip;
            string msg;
            Dictionary<string, byte[]> checksums;

            _txrx = TestUtilities.StartProgressMonitor("InfoZip-compat",
                                                       "InfoZip split archives",
                                                       "Creating "+numFiles+" files");
            _txrx.Send("pb 0 max 3");
            _txrx.Send("pb 1 max " + numFiles);

            var update = new Action<int,int,Int64>( (x,y,z) => {
                    switch (x)
                    {
                        case 0:
                        break;
                        case 1:
                        break;
                        case 2:
                        _txrx.Send("pb 1 step");
                        msg = String.Format("status created {0}/{1} files",
                                            y+1,
                                            ((int)z));
                        _txrx.Send(msg);
                        break;
                    }
                });

            CreateLargeFilesWithChecksums(dirToZip, numFiles, update,
                                          out filesToZip, out checksums);

            _txrx.Send("pb 0 step");
            _txrx.Send("pb 1 max 3");
            _txrx.Send("pb 1 value 0");

            string zipFileToCreate = "infozip-z64-unzip.zip";
            // Create the zip archive via DotNetZip
            using (var zip = new ZipFile())
            {
                zip.AddFiles(filesToZip);
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.Save(zipFileToCreate);
            }
            _txrx.Send("pb 1 step");

            // extract using InfoZip
            string extractDir = "extract";
            this.Exec(infoZipUnzip,
                      String.Format("{0} -d {1}",
                                    zipFileToCreate,
                                    extractDir));
            _txrx.Send("pb 1 step");

            VerifyChecksums(Path.Combine(extractDir, dirToZip), filesToZip, checksums);
            _txrx.Send("pb 1 step");
        }


        [TestMethod]
        public void InfoZip_Unzip_ZeroLengthFile()
        {
            if (!InfoZipIsPresent)
                throw new Exception("InfoZip is not present");

            string password = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            // pass 1 for one regular zero length file.
            // pass 2 for zero-length file with WinZip encryption (which does not actually
            // get applied)
            // pass 3 for PKZip encryption (ditto)
            for (int k=0; k < 3; k++)
            {
                string zipFileToCreate = "ZLF.zip";

                // create an empty file
                string filename = Path.GetRandomFileName();
                using (StreamWriter sw = File.CreateText(filename)) { }

                // Create the zip archive
                using (ZipFile zip1 = new ZipFile())
                {
                    if(k==1)
                    {
                        zip1.Encryption = EncryptionAlgorithm.WinZipAes256;
                        zip1.Password = password;
                    }
                    else if (k==2)
                    {
                        zip1.Password = password;
                        zip1.Encryption = EncryptionAlgorithm.PkzipWeak;
                    }
                    zip1.AddFile(filename, "");
                    zip1.Save(zipFileToCreate);
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(1, TestUtilities.CountEntries(zipFileToCreate),
                                     "Incorrect number of entries in the zip file.");

                string extractDir = "extract." + k;
                Directory.CreateDirectory(extractDir);

                // now, extract the zip. Possibly need a password.
                // eg, unzip.exe -P <passwd> test.zip  -d  <extractdir>
                string args = zipFileToCreate + " -d " + extractDir;
                if (k!=0)
                    args = "-P " + password + " " + args;
                string infozipOut = this.Exec(infoZipUnzip, args);

                TestContext.WriteLine("{0}", infozipOut);
                Assert.IsFalse(infozipOut.Contains("signature not found"));
            }
        }




        [TestMethod]
        public void Perl_Zip()
        {
            if (perl == null)
                throw new Exception("[Perl_Zip] : Perl is not present");

            string zipFileToCreate = "newzip.zip";
            string shortDir = "filesToZip";
            string dirToZip = Path.Combine(TopLevelDir, shortDir);
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(dirToZip, out filesToZip, out checksums);

            // create a zip with perl:
            TestContext.WriteLine("Creating a zip with perl...");
            string createZipPl = GetScript("CreateZip.pl");
            this.Exec(perl,
                      String.Format("\"{0}\" {1} \"{2}\"",
                                    createZipPl,
                                    zipFileToCreate,
                                    shortDir));

            TestContext.WriteLine("");
            TestContext.WriteLine("Extracting that zip with DotNetZip...");
            string extractDir = "extract";
            using (var zip = ZipFile.Read(zipFileToCreate))
            {
                zip.ExtractAll(extractDir);
            }

            TestContext.WriteLine("");
            TestContext.WriteLine("Verifying checksums...");
            VerifyChecksums(Path.Combine(extractDir, dirToZip), filesToZip, checksums);
        }




        [TestMethod]
        public void SevenZip_Zip_1()
        {
            if (!SevenZipIsPresent)
                throw new Exception("[7z_Zip_1] : SevenZip is not present");

            string zipFileToCreate = Path.Combine(TopLevelDir, "7z_Zip_1.zip");
            // create and fill the directories
            string extractDir = "extract";
            string subdir = Path.Combine(TopLevelDir, "files");

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive via 7z.exe
            this.Exec(sevenZip, String.Format("a {0} {1}", zipFileToCreate, subdir));

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // run the COM script to unzip the ZIP archive
            string script = GetScript("VbsUnZip-DotNetZip.vbs");

            this.Exec(cscriptExe,
                      String.Format("\"{0}\" {1} {2}", script, zipFileToCreate, extractDir));

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);
            VerifyTimesNtfs(Path.Combine(extractDir, "files"), filesToZip);
        }



        [TestMethod]
        public void SevenZip_Zip_2()
        {
            if (!SevenZipIsPresent)
                throw new Exception("[7z_Zip_2] : SevenZip is not present");

            string zipFileToCreate = Path.Combine(TopLevelDir, "7z_Zip_2.zip");

            // create and fill the directories
            string extractDir = "extract";
            string subdir = Path.Combine(TopLevelDir, "files");

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive via 7z.exe
            //Directory.SetCurrentDirectory(TopLevelDir);

            this.Exec(sevenZip, String.Format("a {0} {1}", zipFileToCreate, subdir));

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unzip
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                zip1.ExtractAll(extractDir);
            }

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

            VerifyTimesNtfs(Path.Combine(extractDir, "files"), filesToZip);
        }



        [TestMethod]
        public void SevenZip_Unzip()
        {
            if (!SevenZipIsPresent)
                throw new Exception("[7z_Unzip] : SevenZip is not present");

            string zipFileToCreate = Path.Combine(TopLevelDir, "7z_Unzip.zip");

            // create and fill the directories
            string subdir = Path.Combine(TopLevelDir, "files");

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive with DotNetZip
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < filesToZip.Length; i++)
                    zip1.AddItem(filesToZip[i], "files");
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unpack the zip archive via 7z.exe
            Directory.CreateDirectory("extract");
            Directory.SetCurrentDirectory("extract");
            this.Exec(sevenZip, String.Format("x {0}", zipFileToCreate));

            // check the files in the extract dir
            Directory.SetCurrentDirectory(TopLevelDir);

            VerifyChecksums(Path.Combine("extract", "files"), filesToZip, checksums);
        }


        [TestMethod]
        public void SevenZip_Unzip_Password()
        {
            if (!SevenZipIsPresent)
                throw new Exception("[7z_Unzip_Password] : SevenZip is not present");

            string zipFileToCreate = Path.Combine(TopLevelDir, "7z_Unzip_Password.zip");
            string password = Path.GetRandomFileName();
            string subdir = Path.Combine(TopLevelDir, "files");

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive with DotNetZip
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Password = password;
                zip1.AddFiles(filesToZip, "files");
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unpack the zip archive via 7z.exe
            Directory.CreateDirectory("extract");
            Directory.SetCurrentDirectory("extract");
            this.Exec(sevenZip, String.Format("x -p{0} {1}", password, zipFileToCreate));

            // check the files in the extract dir
            Directory.SetCurrentDirectory(TopLevelDir);

            VerifyChecksums(Path.Combine("extract", "files"), filesToZip, checksums);
        }




        [TestMethod]
        public void SevenZip_Unzip_Password_NonSeekableOutput()
        {
            if (!SevenZipIsPresent)
                throw new Exception("[7z_Unzip_Password_NonSeekableOutput] : SevenZip is not present");

            string subdir = Path.Combine(TopLevelDir, "files");

            string[] filesToZip;
            Dictionary<string, byte[]> checksums = null;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);
            //CreateFilesAndChecksums(subdir, 2, 32, out filesToZip, out checksums);

#if NOT
            // debugging
            Directory.CreateDirectory(subdir);
            DateTime atMidnight = new DateTime(DateTime.Now.Year,
                                               DateTime.Now.Month,
                                               DateTime.Now.Day,
                                               11,11,11);
            filesToZip = new String[2];
            for (int z=0; z < 2; z++)
            {
                string fname = Path.Combine(subdir, String.Format("file{0:D3}.txt", z));
                File.WriteAllText(fname, "12341234123412341234123412341234");
                File.SetLastWriteTime(fname, atMidnight);
                File.SetLastAccessTime(fname, atMidnight);
                File.SetCreationTime(fname, atMidnight);
                filesToZip[z]= fname;
            }
#endif
            TestContext.WriteLine("Test Unzip with 7zip");
            TestContext.WriteLine("============================================");

            // marker file
            // using (File.Create(Path.Combine(TopLevelDir, "DotNetZip-" + ZipFile.LibraryVersion.ToString()))) ;

            int i = 0;
            foreach (var compLevel in compLevels)
            {
                TestContext.WriteLine("---------------------------------");
                TestContext.WriteLine("Trial {0}", i);
                TestContext.WriteLine("CompressionLevel = {0}", compLevel);
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("7z_Unzip_Password_NonSeekableOutput.{0}.zip", i));
                string password = Path.GetRandomFileName();
                //string password = "0123456789ABCDEF";
                string extractDir = Path.Combine(TopLevelDir, String.Format("extract.{0}", i));

                TestContext.WriteLine("Password = {0}", password);

                // Create the zip archive with DotNetZip
                //Directory.SetCurrentDirectory(TopLevelDir);
                // Want to test the library when saving to non-seekable output streams.  Like
                // stdout or ASPNET's Response.OutputStream.  This simulates it.
                using (var rawOut = System.IO.File.Create(zipFileToCreate))
                {
                    using (var nonSeekableOut = new Ionic.Zip.Tests.NonSeekableOutputStream(rawOut))
                    {
                        using (ZipFile zip1 = new ZipFile())
                        {
                            zip1.CompressionLevel = (Ionic.Zlib.CompressionLevel)compLevel;
                            zip1.Password = password;
                            zip1.AddFiles(filesToZip, "files");
                            zip1.Save(nonSeekableOut);
                        }
                    }
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                                     "Incorrect number of entries in the zip file.");

                // unpack the zip archive via 7z.exe
                Directory.CreateDirectory(extractDir);
                this.Exec(sevenZip, String.Format("x -o{0} -p{1} {2}",
                                                  Path.GetFileName(extractDir),
                                                  password,
                                                  zipFileToCreate));

                // check the files in the extract dir
                //Directory.SetCurrentDirectory(TopLevelDir);

                VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);
                i++;
            }
        }



        [TestMethod]
        public void SevenZip_Unzip_SFX()
        {
            if (!SevenZipIsPresent)
                throw new Exception("[7z_Unzip_SFX] : SevenZip is not present");

            string zipFileToCreate = Path.Combine(TopLevelDir, "7z_Unzip_SFX.exe");

            // create and fill the directories
            string subdir = Path.Combine(TopLevelDir, "files");

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // Create the zip archive with DotNetZip
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                for (int i = 0; i < filesToZip.Length; i++)
                    zip1.AddItem(filesToZip[i], "files");
                zip1.SaveSelfExtractor(zipFileToCreate,
                                       SelfExtractorFlavor.ConsoleApplication);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // unpack the zip archive via 7z.exe
            Directory.CreateDirectory("extract");
            Directory.SetCurrentDirectory("extract");
            this.Exec(sevenZip, String.Format("x {0}", zipFileToCreate));

            // check the files in the extract dir
            Directory.SetCurrentDirectory(TopLevelDir);

            VerifyChecksums(Path.Combine("extract", "files"), filesToZip, checksums);
        }



        [TestMethod]
        public void Winzip_Zip()
        {
            Winzip_Zip_Variable("");
        }


        [TestMethod]
        public void Winzip_Zip_Password()
        {
            if (!WinZipIsPresent)
                throw new Exception("[Winzip_Zip_Password] : winzip is not present");


            string password = Path.GetRandomFileName().Replace(".", "@");
            TestContext.WriteLine("creating zip with password ({0})", password);
            string zipfile = Winzip_Zip_Variable("-s" + password, false);
            string extractDir = "extract";

            // unzip with DotNetZip
            //Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = ZipFile.Read(zipfile))
            {
                zip1.Password = password;
                zip1.ExtractAll(extractDir);
            }
        }


        [TestMethod]
        public void Winzip_Zip_Normal()
        {
            Winzip_Zip_Variable("-en");
        }

        [TestMethod]
        public void Winzip_Zip_Fast()
        {
            Winzip_Zip_Variable("-ef");
        }

        [TestMethod]
        public void Winzip_Zip_SuperFast()
        {
            Winzip_Zip_Variable("-es");
        }

        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Winzip_Zip_EZ()
        {
            if (!WinZipIsPresent) throw new Exception("no winzip");
            // Unsupported compression method
            Winzip_Zip_Variable("-ez");
        }

        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Winzip_Zip_PPMd()
        {
            if (!WinZipIsPresent) throw new Exception("no winzip");
            // Unsupported compression method
            Winzip_Zip_Variable("-ep");
        }

        [TestMethod]
        public void Winzip_Zip_Bzip2()
        {
            if (!WinZipIsPresent) throw new Exception("no winzip");
            Winzip_Zip_Variable("-eb");
        }

        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Winzip_Zip_Enhanced()
        {
            if (!WinZipIsPresent) throw new Exception("no winzip");
            // Unsupported compression method
            Winzip_Zip_Variable("-ee");
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Winzip_Zip_LZMA()
        {
            if (!WinZipIsPresent) throw new Exception("no winzip");
            // Unsupported compression method
            Winzip_Zip_Variable("-el");
        }


        public string Winzip_Zip_Variable(string options)
        {
            return Winzip_Zip_Variable(options, true);
        }

        public string Winzip_Zip_Variable(string options, bool wantVerify)
        {
            if (!WinZipIsPresent)
                throw new Exception(String.Format("[options({0})] : winzip is not present", options));

            // options:
            // -sPassword
            // -ep - PPMd compression.
            // -el - LZMA compression
            // -eb - bzip2 compression
            // -ee - "enhanced" compression.
            // -en - normal compression.
            // -ef - fast compression.
            // -es - superfast compression.
            // -ez - select best method at runtime. Requires WinZip12 to extract.
            // empty string = default
            string zipFileToCreate = Path.Combine(TopLevelDir, "Winzip_Zip.zip");

            string dirInZip = "files";
            string subdir = Path.Combine(TopLevelDir, dirInZip);

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            // delay between file creation and zip creation
            System.Threading.Thread.Sleep(1200);

            // exec wzzip.exe to create the zip file
            string formatString = "-a -p " + options + " -yx {0} {1}\\*.*";
            string wzzipOut = this.Exec(wzzip, String.Format(formatString, zipFileToCreate, subdir));

            if (wantVerify)
            {
                // unzip with DotNetZip
                string extractDir = "extract";
                using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
                {
                    zip1.ExtractAll(extractDir);
                }

                // check the files in the extract dir
                VerifyChecksums(extractDir, filesToZip, checksums);

                // verify the file times.
                VerifyTimesNtfs(extractDir, filesToZip);
            }

            return zipFileToCreate;
        }



        [TestMethod]
        [Timeout(9 * 60 * 1000)]  // in ms, 60 * 1000 = 1min
        public void Winzip_Unzip_2()
        {
            if (!WinZipIsPresent)
                throw new Exception("[Winzip_Unzip_2] : winzip is not present");

            string zipFileToCreate = "Winzip_Unzip_2.zip";

            // create and fill the directories
            string extractDir = "extract";
            //string subdir = "files";
            Dictionary<string, byte[]> checksums = new Dictionary<string, byte[]>();
            var filesToZip = GetSelectionOfTempFiles(_rnd.Next(13) + 8, checksums);

            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFiles(filesToZip, "files");
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Count,
                                 "Incorrect number of entries in the zip file.");

            // now, extract the zip
            // eg, wzunzip.exe -d test.zip  <extractdir>
            Directory.CreateDirectory(extractDir);
            this.Exec(wzunzip, String.Format("-d -yx {0} \"{1}\"",
                                             zipFileToCreate, extractDir));

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

            // verify the file times
            VerifyTimesDos(Path.Combine(extractDir, "files"), filesToZip);
        }




        [TestMethod]
        public void Winzip_Unzip_ZeroLengthFile()
        {
            if (!WinZipIsPresent)
                throw new Exception("[Winzip_Unzip_ZeroLengthFile] : winzip is not present");

            string password = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            // pass 1 for one regular zero length file.
            // pass 2 for zero-length file with WinZip encryption (which does not actually
            // get applied)
            // pass 3 for PKZip encryption (ditto)
            for (int k=0; k < 3; k++)
            {
                string zipFileToCreate = "ZLF.zip";

                // create an empty file
                string filename = Path.GetRandomFileName();
                using (StreamWriter sw = File.CreateText(filename)) { }

                // Create the zip archive
                using (ZipFile zip1 = new ZipFile())
                {
                    if(k==1)
                    {
                        zip1.Encryption = EncryptionAlgorithm.WinZipAes256;
                        zip1.Password = password;
                    }
                    else if (k==2)
                    {
                        zip1.Password = password;
                        zip1.Encryption = EncryptionAlgorithm.PkzipWeak;
                    }
                    zip1.AddFile(filename, "");
                    zip1.Save(zipFileToCreate);
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(1, TestUtilities.CountEntries(zipFileToCreate),
                                     "Incorrect number of entries in the zip file.");

                // now, test the zip. Possibly need a password.
                // eg, wzunzip.exe -t test.zip  <extractdir>
                string args = "-t " + zipFileToCreate;
                if (k!=0)
                    args = "-s" + password + " " + args;
                string wzunzipOut = this.Exec(wzunzip, args);

                TestContext.WriteLine("{0}", wzunzipOut);
                Assert.IsTrue(wzunzipOut.Contains("No errors"));
                Assert.IsFalse(wzunzipOut.Contains("At least one error was detected"));
            }
        }



        [TestMethod]
        public void Winzip_Unzip_Password()
        {
            if (!WinZipIsPresent)
                throw new Exception("[Winzip_Unzip_Password] : winzip is not present");

            //Directory.SetCurrentDirectory(TopLevelDir);
            string zipFileToCreate = "Winzip_Unzip_Password.zip";
            string extractDir = "extract";
            string subdir = "fodder";
            // create a bunch of files
            var filesToZip = TestUtilities.GenerateFilesFlat(subdir);
            string password = Path.GetRandomFileName();

            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Password = password;
                zip1.AddFiles(filesToZip, "");
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(filesToZip.Length, TestUtilities.CountEntries(zipFileToCreate),
                                 "Incorrect number of entries in the zip file.");

            // now, test the zip
            // eg, wzunzip.exe -t test.zip  <extractdir>
            string args = String.Format("-t -s{0} {1}", password, zipFileToCreate);
            string wzunzipOut = this.Exec(wzunzip, args);
            TestContext.WriteLine("{0}", wzunzipOut);

            Assert.IsTrue(wzunzipOut.Contains("No errors"));
            Assert.IsFalse(wzunzipOut.Contains("At least one error was detected"));

            // extract the zip
            // eg, wzunzip.exe -d -yx -sPassword  test.zip  <extractdir>
            args = String.Format("-d -yx -s{0} {1} {2}",
                                 password, zipFileToCreate, extractDir);
            Directory.CreateDirectory(extractDir);
            wzunzipOut = this.Exec(wzunzip, args);

            Assert.IsFalse(wzunzipOut.Contains("skipping"));
            Assert.IsFalse(wzunzipOut.Contains("incorrect"));
        }



        [TestMethod]
        public void Winzip_Unzip_Password_NonSeekableOutput()
        {
            if (!WinZipIsPresent)
                throw new Exception("[Winzip_Unzip_Password_NonSeekableOutput] : winzip is not present");


            // create a bunch of files
            string subdir = "fodder";
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            var compressionLevels = Enum.GetValues(typeof(Ionic.Zlib.CompressionLevel));
            int i = 0;
            foreach (var compLevel in compressionLevels)
            {
                string zipFileToCreate =
                    Path.Combine(TopLevelDir,
                                 String.Format("Winzip_Unzip_Pwd_NonSeek.{0}.zip",
                                               i));
                string extractDir = "extract" + i.ToString();
                string password = Path.GetRandomFileName();

                // Want to test the library when saving to non-seekable
                // output streams.  Like stdout or ASPNET's
                // Response.OutputStream.  This simulates it.
                using (var rawOut = System.IO.File.Create(zipFileToCreate))
                {
                    using (var nonSeekableOut = new Ionic.Zip.Tests.NonSeekableOutputStream(rawOut))
                    {
                        // Create the zip archive
                        using (ZipFile zip1 = new ZipFile())
                        {
                            zip1.CompressionLevel = (Ionic.Zlib.CompressionLevel)compLevel;
                            zip1.Password = password;
                            zip1.AddFiles(filesToZip, "files");
                            zip1.Save(nonSeekableOut);
                        }
                    }
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(filesToZip.Length,
                                     TestUtilities.CountEntries(zipFileToCreate),
                                     "Incorrect number of entries in the zip file.");

                // now, test the zip
                // eg, wzunzip.exe -t test.zip
                string wzunzipOut = this.Exec(wzunzip, String.Format("-t -s{0} {1}",
                                                                     password, zipFileToCreate));
                TestContext.WriteLine("{0}", wzunzipOut);

                Assert.IsTrue(wzunzipOut.Contains("No errors"));
                Assert.IsFalse(wzunzipOut.Contains("At least one error was detected"));

                // extract the zip
                Directory.CreateDirectory(extractDir);
                // eg, wzunzip.exe -d -yx -sPassword  test.zip  <extractdir>
                wzunzipOut = this.Exec(wzunzip, String.Format("-d -yx -s{0} {1} {2}",
                                                             password, zipFileToCreate, extractDir));
                Assert.IsFalse(wzunzipOut.Contains("skipping"));
                Assert.IsFalse(wzunzipOut.Contains("incorrect"));

                // check the files in the extract dir
                VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

                i++;
            }
        }



        [TestMethod]
        public void Winzip_Unzip_SFX()
        {
            if (!WinZipIsPresent)
                throw new Exception("[Winzip_Unzip_SFX] : winzip is not present");

            string zipFileToCreate = "Winzip_Unzip_SFX.exe";

            // create and fill the directories
            string extractDir = Path.Combine(TopLevelDir, "extract");
            string subdir = Path.Combine(TopLevelDir, "files");

            Dictionary<string, byte[]> checksums = new Dictionary<string, byte[]>();
            var filesToZip = GetSelectionOfTempFiles(_rnd.Next(13) + 8, checksums);

            // Create the zip archive
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFiles(filesToZip, "files");
                zip1.SaveSelfExtractor(zipFileToCreate,
                                       SelfExtractorFlavor.ConsoleApplication);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 filesToZip.Count,
                                 "Incorrect number of entries in the zip file.");

            // now, extract the zip
            // eg, wzunzip.exe -d test.zip  [<extractdir>]
            Directory.CreateDirectory(extractDir);
            Directory.SetCurrentDirectory(extractDir);

            // -d = restore folder structure
            // -yx = restore extended timestamps to extracted files
            this.Exec(wzunzip, "-d -yx " + Path.Combine("..", zipFileToCreate));

            // check the files in the extract dir
            VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);

            // verify the file times
            VerifyTimesDos(Path.Combine(extractDir, "files"), filesToZip);
        }


        [TestMethod]
        public void Winzip_Unzip_Bzip2()
        {
            if (!WinZipIsPresent) throw new Exception("no winzip");

            string zipFileToCreate = "Winzip_Unzip.zip";

            string dirInZip = "files";
            string extractDir = "extract";
            string subdir = Path.Combine(TopLevelDir, dirInZip);

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            var additionalFiles = GetSelectionOfTempFiles(checksums);

            // Now, Create the zip archive with DotNetZip
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.CompressionMethod = CompressionMethod.BZip2;
                zip1.AddFiles(filesToZip, dirInZip);
                zip1.AddFiles(additionalFiles, dirInZip);
                zip1.Save(zipFileToCreate);
            }

            TestContext.WriteLine("Verifying the number of files in the zip");
            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 filesToZip.Length + additionalFiles.Count,
                                 "Incorrect number of entries in the zip file.");


            // verify that the output states that the compression method
            // used for each entry was BZIP2...
            TestContext.WriteLine("Verifying that BZIP2 was the comp method used...");

            // examine and unpack the zip archive via WinZip
            // first, examine the zip entry metadata:
            string wzzipOut = this.Exec(wzzip, "-vt " + zipFileToCreate);

            var numBzipped = TestUtilities.CountOccurrences(wzzipOut, "Compression Method: BZipped");
            TestContext.WriteLine("Found {0} bzipped entries.", numBzipped);

            // Not all of the files will be bzipped. Some of the files
            // may be "stored" because they are incompressible. This
            // should be the exception, though.
            var numStored =  TestUtilities.CountOccurrences(wzzipOut, "Compression Method: Stored");
            TestContext.WriteLine("Found {0} stored entries.", numStored);

            Assert.AreEqual<int>( numBzipped + numStored,
                                 filesToZip.Length + additionalFiles.Count);
            Assert.IsTrue( numBzipped > 2*numStored,
                           "The number of bzipped files is too low.");

            TestContext.WriteLine("Extracting...");
            // now, extract the zip
            // eg, wzunzip.exe -d test.zip  <extractdir>
            Directory.CreateDirectory(extractDir);
            Directory.SetCurrentDirectory(extractDir);
            this.Exec(wzunzip, String.Format("-d -yx \"{0}\"",
                                             Path.Combine(TopLevelDir,zipFileToCreate)));

            // check the files in the extract dir
            Directory.SetCurrentDirectory(TopLevelDir);
            String[] filesToCheck = new String[filesToZip.Length + additionalFiles.Count];
            filesToZip.CopyTo(filesToCheck, 0);
            additionalFiles.ToArray().CopyTo(filesToCheck, filesToZip.Length);

            VerifyChecksums(Path.Combine("extract", dirInZip), filesToCheck, checksums);

            VerifyFileTimes1(extractDir, additionalFiles);
        }


        [TestMethod]
        public void Winzip_Unzip_Bzip2_Large()
        {
            // BZip2 uses work buffers of 900k (ish). When compressing files that
            // can be Run-length-encoded into a buffer smaller than 900k, only one
            // "block" is used in the compressed output. Multiple blocks get
            // emitted with input files that cannot be run-length encoded into
            // 900k (ish). This test verifies that everything works correctly when
            // compressing larger files that require multiple blocks in the
            // compressed output. (At one point there was a problem combining CRCs
            // from multiple blocks.)
            if (!WinZipIsPresent) throw new Exception("no winzip");

            TestContext.WriteLine("Creating the fodder files...");
            string zipFileToCreate = "BZ_Large.zip";
            int n = _rnd.Next(5) + 5;
            int baseSize = _rnd.Next(0x80000) + 0x3000ff;
            int delta = 0x80000;
            string dirInZip = "files";
            string extractDir = "extract";
            string subdir = Path.Combine(TopLevelDir, dirInZip);
            var filesToZip = TestUtilities.GenerateFilesFlat(subdir, n, baseSize, baseSize+delta);

            TestContext.WriteLine("Creating the zip...");
            // Now, Create the zip archive with DotNetZip
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.CompressionMethod = CompressionMethod.BZip2;
                zip1.AddFiles(filesToZip, dirInZip);
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            TestContext.WriteLine("Verifying the number of files in the zip...");
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            // examine and unpack the zip archive via WinZip
            // first, examine the zip entry metadata:
            string wzzipOut = this.Exec(wzzip, "-vt " + zipFileToCreate);

            // verify that the output states that the compression method
            // used for each entry was BZIP2...
            TestContext.WriteLine("Verifying that BZIP2 was the comp method used...");
            Assert.AreEqual<int>(TestUtilities.CountOccurrences(wzzipOut, "Compression Method: BZipped"),
                                 filesToZip.Length);

            // now, extract the zip
            // eg, wzunzip.exe -d test.zip  <extractdir>
            TestContext.WriteLine("Extracting...");
            Directory.CreateDirectory(extractDir);
            Directory.SetCurrentDirectory(extractDir);
            this.Exec(wzunzip, String.Format("-d -yx \"{0}\"",
                                             Path.Combine("..",zipFileToCreate)));
        }




        [TestMethod]
        public void Winzip_Unzip_Basic()
        {
            if (!WinZipIsPresent)
                throw new Exception("[Winzip_Unzip_Basic] : winzip is not present");

            string zipFileToCreate = "Winzip_Unzip_Basic.zip";

            string dirInZip = "files";
            string extractDir = "extract";
            string subdir = Path.Combine(TopLevelDir, dirInZip);

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            var additionalFiles = GetSelectionOfTempFiles(checksums);

            int i = 0;
            // set R and S attributes on the first file
            if (!File.Exists(filesToZip[i])) throw new Exception("Something is berry berry wrong.");
            File.SetAttributes(filesToZip[i], FileAttributes.ReadOnly | FileAttributes.System);

            // set H attribute on the second file
            i++;
            if (i == filesToZip.Length) throw new Exception("Not enough files??.");
            if (!File.Exists(filesToZip[i])) throw new Exception("Something is berry berry wrong.");
            File.SetAttributes(filesToZip[i], FileAttributes.Hidden);

            // Now, Create the zip archive with DotNetZip
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFiles(filesToZip, dirInZip);
                zip1.AddFiles(additionalFiles, dirInZip);
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 filesToZip.Length + additionalFiles.Count,
                                 "Incorrect number of entries in the zip file.");

            // examine and unpack the zip archive via WinZip
            // first, examine the zip entry metadata:
            string wzzipOut = this.Exec(wzzip, "-vt " + zipFileToCreate);

            string[] expectedAttrStrings = { "s-r-", "-hw-", "--w-" };

            // example: Filename: folder5\Test8.txt
            for (i = 0; i < expectedAttrStrings.Length; i++)
            {
                var f = Path.GetFileName(filesToZip[i]);
                var fileInZip = Path.Combine(dirInZip, f);
                string textToLookFor = String.Format("Filename: {0}", fileInZip.Replace("/", "\\"));
                int x = wzzipOut.IndexOf(textToLookFor);
                Assert.IsTrue(x > 0, "Could not find expected text ({0}) in WZZIP output.", textToLookFor);
                textToLookFor = "Attributes: ";
                x = wzzipOut.IndexOf(textToLookFor, x);
                string attrs = wzzipOut.Substring(x + textToLookFor.Length, 4);
                Assert.AreEqual(expectedAttrStrings[i], attrs, "Unexpected attributes on File {0}.", i);
            }

            // now, extract the zip
            // eg, wzunzip.exe -d test.zip  <extractdir>
            Directory.CreateDirectory(extractDir);
            Directory.SetCurrentDirectory(extractDir);
            this.Exec(wzunzip, String.Format("-d -yx ..\\{0}", zipFileToCreate));

            // check the files in the extract dir
            Directory.SetCurrentDirectory(TopLevelDir);
            String[] filesToCheck = new String[filesToZip.Length + additionalFiles.Count];
            filesToZip.CopyTo(filesToCheck, 0);
            additionalFiles.ToArray().CopyTo(filesToCheck, filesToZip.Length);

            VerifyChecksums(Path.Combine("extract", dirInZip), filesToCheck, checksums);

            VerifyFileTimes1(extractDir, additionalFiles);
        }


        private void VerifyFileTimes1(string extractDir, List<string> additionalFiles)
        {
            // verify the file times
            DateTime atMidnight = new DateTime(DateTime.Now.Year,
                                               DateTime.Now.Month,
                                               DateTime.Now.Day);
            DateTime fortyFiveDaysAgo = atMidnight - new TimeSpan(45, 0, 0, 0);

            string[] extractedFiles = Directory.GetFiles(extractDir);

            foreach (var fqPath in extractedFiles)
            {
                string filename = Path.GetFileName(fqPath);
                DateTime stamp = File.GetLastWriteTime(fqPath);
                if (filename.StartsWith("testfile"))
                {
                    Assert.IsTrue((stamp == atMidnight || stamp == fortyFiveDaysAgo),
                                  "The timestamp on the file {0} is incorrect ({1}).",
                                  fqPath, stamp.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    var orig = (from f in additionalFiles
                                where Path.GetFileName(f) == filename
                                select f)
                        .First();

                    DateTime t1 = File.GetLastWriteTime(filename);
                    DateTime t2 = File.GetLastWriteTime(orig);
                    Assert.AreEqual<DateTime>(t1, t2);
                    t1 = File.GetCreationTime(filename);
                    t2 = File.GetCreationTime(orig);
                    Assert.AreEqual<DateTime>(t1, t2);
                }
            }
        }

        private List<string> GetSelectionOfTempFiles(Dictionary<string, byte[]> checksums)
        {
            return GetSelectionOfTempFiles(_rnd.Next(23) + 9, checksums);
        }

        private List<string> excludedFilenames = new List<string>();


        private List<string> GetSelectionOfTempFiles(int numFilesWanted, Dictionary<string, byte[]> checksums)
        {
            string tmpPath = Environment.GetEnvironmentVariable("TEMP"); // C:\Users\dinoch\AppData\Local\Temp
            String[] candidates = Directory.GetFiles(tmpPath);
            var theChosenOnes = new List<String>();
            int trials = 0;
            int otherSide = 0;
            int minOtherSide = numFilesWanted / 3 + 1;
            do
            {
                if (theChosenOnes.Count > numFilesWanted && otherSide >= minOtherSide) break;

                // randomly select a candidate
                var f = candidates[_rnd.Next(candidates.Length)];
                if (excludedFilenames.Contains(f)) continue;

                try
                {
                    var fi = new FileInfo(f);
                    if (Path.GetFileName(f)[0] == '~'
                        || theChosenOnes.Contains(f)
                        || fi.Length > 10000000  // too large
                        || fi.Length < 100)      // too small
                    {
                        excludedFilenames.Add(f);
                    }
                    else
                    {
                        DateTime lastwrite = File.GetLastWriteTime(f);
                        bool onOtherSideOfDst =
                            (DateTime.Now.IsDaylightSavingTime() && !lastwrite.IsDaylightSavingTime()) ||
                            (!DateTime.Now.IsDaylightSavingTime() && lastwrite.IsDaylightSavingTime());

                        if (onOtherSideOfDst)
                            otherSide++;

                        // If it's on the other side of DST,
                        //   or
                        // there are zero or one on *this side*
                        //   or
                        // we can still reach the "other side" quota.
                        if (onOtherSideOfDst || (theChosenOnes.Count - otherSide < 2) ||
                            ((otherSide < minOtherSide) && (numFilesWanted - theChosenOnes.Count > minOtherSide - otherSide)))
                        {
                            var key = Path.GetFileName(f);
                            var chk = TestUtilities.ComputeChecksum(f);
                            checksums.Add(key, chk);
                            theChosenOnes.Add(f);
                        }
                    }
                }
                catch { /* gulp! */ }
                trials++;
            }
            while (trials < 1000);

            theChosenOnes.Sort();
            return theChosenOnes;
        }





        [TestMethod]
        public void Extract_WinZip_SelfExtractor()
        {
            _Extract_ZipFile("winzip-sfx.exe");
        }

        [TestMethod]
        public void Extract_Docx()
        {
            _Extract_ZipFile("Vanishing Oatmeal Cookies.docx");
        }

        [TestMethod]
        public void Extract_ZipWithDuplicateNames_wi10330()
        {
            _Extract_ZipFile("wi10330-badzip.zip");
        }

        [TestMethod]
        public void Extract_Xlsx()
        {
            _Extract_ZipFile("Book1.xlsx");
        }

        [TestMethod]
        public void Extract_DWF()
        {
            _Extract_ZipFile("plot.dwf");
        }

        [TestMethod]
        public void Extract_InfoZipAppNote()
        {
            _Extract_ZipFile("appnote-iz-latest.zip");
        }

        [TestMethod]
        public void Extract_AndroidApp()
        {
            _Extract_ZipFile("Calendar.apk");
        }


        private void _Extract_ZipFile(string fileName)
        {
            TestContext.WriteLine("Current Dir: {0}", CurrentDir);
            string sourceDir = CurrentDir;
            for (int i = 0; i < 3; i++)
                sourceDir = Path.GetDirectoryName(sourceDir);

            string fqFileName = Path.Combine(Path.Combine(sourceDir,
                                                             "Zip Tests\\bin\\Debug\\zips"),
                                                fileName);

            TestContext.WriteLine("Reading zip file: '{0}'", fqFileName);
            using (ZipFile zip = ZipFile.Read(fqFileName))
            {
                string extractDir = "extract";
                foreach (ZipEntry e in zip)
                {

                    TestContext.WriteLine("{1,-22} {2,9} {3,5:F0}%   {4,9}  {5,3} {6:X8} {0}",
                                                                         e.FileName,
                                                                         e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                         e.UncompressedSize,
                                                                         e.CompressionRatio,
                                                                         e.CompressedSize,
                                                                         (e.UsesEncryption) ? "Y" : "N",
                                                                         e.Crc);
                    e.Extract(extractDir);
                }
            }
        }



    }


}
