// SelfExtractor.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009, 2011 Dino Chiesa .
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
// Time-stamp: <2011-June-18 21:12:09>
//
// ------------------------------------------------------------------
//
// This module defines the tests for the self-extracting archive capability
// within DotNetZip: creating, reading, updating, and running SFX's.
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
    /// Summary description for Self extracting archives (SFX)
    /// </summary>
    [TestClass]
    public class SelfExtractor : IonicTestClass
    {
        public SelfExtractor() : base() { }


        [TestMethod]
        public void SFX_CanRead()
        {
            SelfExtractorFlavor[] flavors =
            {
                SelfExtractorFlavor.ConsoleApplication,
                SelfExtractorFlavor.WinFormsApplication
            };

            for (int k = 0; k < flavors.Length; k++)
            {
                string sfxFileToCreate = Path.Combine(TopLevelDir, String.Format("SFX_{0}.exe", flavors[k].ToString()));
                string unpackDir = Path.Combine(TopLevelDir, "unpack");
                if (Directory.Exists(unpackDir))
                    Directory.Delete(unpackDir, true);
                string readmeString = "Hey there!  This zipfile entry was created directly from a string in application code.";

                int entriesAdded = 0;
                String filename = null;

                string Subdir = Path.Combine(TopLevelDir, String.Format("A{0}", k));
                Directory.CreateDirectory(Subdir);
                var checksums = new Dictionary<string, string>();

                int fileCount = _rnd.Next(50) + 30;
                for (int j = 0; j < fileCount; j++)
                {
                    filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                    entriesAdded++;
                    var chk = TestUtilities.ComputeChecksum(filename);
                    checksums.Add(filename.Replace(TopLevelDir + "\\", "").Replace('\\', '/'), TestUtilities.CheckSumToString(chk));
                }

                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.AddDirectory(Subdir, Path.GetFileName(Subdir));
                    zip1.Comment = "This will be embedded into a self-extracting exe";
                    MemoryStream ms1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(readmeString));
                    zip1.AddEntry("Readme.txt", ms1);
                    zip1.SaveSelfExtractor(sfxFileToCreate, flavors[k]);
                }

                TestContext.WriteLine("---------------Reading {0}...", sfxFileToCreate);
                using (ZipFile zip2 = ZipFile.Read(sfxFileToCreate))
                {
                    //string extractDir = String.Format("extract{0}", j);
                    foreach (var e in zip2)
                    {
                        TestContext.WriteLine(" Entry: {0}  c({1})  u({2})", e.FileName, e.CompressedSize, e.UncompressedSize);
                        e.Extract(unpackDir);
                        if (!e.IsDirectory)
                        {
                            if (checksums.ContainsKey(e.FileName))
                            {
                                filename = Path.Combine(unpackDir, e.FileName);
                                string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(filename));
                                Assert.AreEqual<string>(checksums[e.FileName], actualCheckString, "In trial {0}, Checksums for ({1}) do not match.", k, e.FileName);
                            }
                            else
                            {
                                Assert.AreEqual<string>("Readme.txt", e.FileName, String.Format("trial {0}", k));
                            }
                        }
                    }
                }
            }
        }



        [TestMethod]
        public void SFX_Update_Console()
        {
            SFX_Update(SelfExtractorFlavor.ConsoleApplication);
        }

        [TestMethod]
        public void SFX_Update_Winforms()
        {
            SFX_Update(SelfExtractorFlavor.WinFormsApplication);
        }

        private void SFX_Update(SelfExtractorFlavor flavor)
        {
            string sfxFileToCreate = Path.Combine(TopLevelDir,
                                                  String.Format("SFX_Update{0}.exe",
                                                                flavor.ToString()));
            string unpackDir = Path.Combine(TopLevelDir, "unpack");
            if (Directory.Exists(unpackDir))
                Directory.Delete(unpackDir, true);

            string readmeString = "Hey there!  This zipfile entry was created directly from a string in application code.";

            // create a file and compute the checksum
            string Subdir = Path.Combine(TopLevelDir, "files");
            Directory.CreateDirectory(Subdir);
            var checksums = new Dictionary<string, string>();

            string filename = Path.Combine(Subdir, "file1.txt");
            TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
            var chk = TestUtilities.ComputeChecksum(filename);
            checksums.Add(filename.Replace(TopLevelDir + "\\", "").Replace('\\', '/'), TestUtilities.CheckSumToString(chk));

            // create the SFX
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFile(filename, Path.GetFileName(Subdir));
                zip1.Comment = "This will be embedded into a self-extracting exe";
                MemoryStream ms1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(readmeString));
                zip1.AddEntry("Readme.txt", ms1);
                var sfxOptions = new SelfExtractorSaveOptions
                {
                    Flavor = flavor,
                    Quiet = true,
                    DefaultExtractDirectory = unpackDir
                };
                zip1.SaveSelfExtractor(sfxFileToCreate, sfxOptions);
            }

            // verify count
            Assert.AreEqual<int>(TestUtilities.CountEntries(sfxFileToCreate), 2, "The Zip file has the wrong number of entries.");

            // create another file
            filename = Path.Combine(Subdir, "file2.txt");
            TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
            chk = TestUtilities.ComputeChecksum(filename);
            checksums.Add(filename.Replace(TopLevelDir + "\\", "").Replace('\\', '/'), TestUtilities.CheckSumToString(chk));
            string password = "ABCDEFG";
            // update the SFX
            using (ZipFile zip1 = ZipFile.Read(sfxFileToCreate))
            {
                zip1.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                zip1.Encryption = EncryptionAlgorithm.WinZipAes256;
                zip1.Comment = "The password is: " + password;
                zip1.Password = password;
                zip1.AddFile(filename, Path.GetFileName(Subdir));
                var sfxOptions = new SelfExtractorSaveOptions
                {
                    Flavor = flavor,
                    Quiet = true,
                    DefaultExtractDirectory = unpackDir
                };
                zip1.SaveSelfExtractor(sfxFileToCreate, sfxOptions);
            }

            // verify count
            Assert.AreEqual<int>(TestUtilities.CountEntries(sfxFileToCreate), 3, "The Zip file has the wrong number of entries.");


            // read the SFX
            TestContext.WriteLine("---------------Reading {0}...", sfxFileToCreate);
            using (ZipFile zip2 = ZipFile.Read(sfxFileToCreate))
            {
                zip2.Password = password;
                //string extractDir = String.Format("extract{0}", j);
                foreach (var e in zip2)
                {
                    TestContext.WriteLine(" Entry: {0}  c({1})  u({2})", e.FileName, e.CompressedSize, e.UncompressedSize);
                    e.Extract(unpackDir);
                    if (!e.IsDirectory)
                    {
                        if (checksums.ContainsKey(e.FileName))
                        {
                            filename = Path.Combine(unpackDir, e.FileName);
                            string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(filename));
                            Assert.AreEqual<string>(checksums[e.FileName], actualCheckString, "Checksums for ({1}) do not match.", e.FileName);
                            //TestContext.WriteLine("     Checksums match ({0}).\n", actualCheckString);
                        }
                        else
                        {
                            Assert.AreEqual<string>("Readme.txt", e.FileName);
                        }
                    }
                }
            }

            int N = (flavor == SelfExtractorFlavor.ConsoleApplication) ? 2 : 1;
            for (int j = 0; j < N; j++)
            {
                // run the SFX
                TestContext.WriteLine("Running the SFX... ");
                var psi = new System.Diagnostics.ProcessStartInfo(sfxFileToCreate);
                if (flavor == SelfExtractorFlavor.ConsoleApplication)
                {
                    if (j == 0)
                        psi.Arguments = "-o -p " + password; // overwrite
                    else
                        psi.Arguments = "-p " + password;
                }
                psi.WorkingDirectory = TopLevelDir;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);
                process.WaitForExit();
                int rc = process.ExitCode;
                TestContext.WriteLine("SFX exit code: ({0})", rc);

                if (j == 0)
                {
                    Assert.AreEqual<Int32>(0, rc, "The exit code from the SFX was nonzero ({0}).", rc);
                }
                else
                {
                    Assert.AreNotEqual<Int32>(0, rc, "The exit code from the SFX was zero ({0}).");
                }
            }

            // verify the unpacked files?
        }



        [TestMethod]
        public void SFX_Console()
        {
            string exeFileToCreate = Path.Combine(TopLevelDir, "SFX_Console.exe");
            string unpackDir = Path.Combine(TopLevelDir, "unpack");
            string readmeString = "Hey there!  This zipfile entry was created directly from a string in application code.";

            int entriesAdded = 0;
            String filename = null;

            string Subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(Subdir);
            var checksums = new Dictionary<string, string>();

            int fileCount = _rnd.Next(10) + 10;
            for (int j = 0; j < fileCount; j++)
            {
                filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                entriesAdded++;
                var chk = TestUtilities.ComputeChecksum(filename);
                checksums.Add(filename, TestUtilities.CheckSumToString(chk));
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(Subdir, Path.GetFileName(Subdir));
                zip.Comment = "This will be embedded into a self-extracting exe";
                MemoryStream ms1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(readmeString));
                zip.AddEntry("Readme.txt", ms1);
                var sfxOptions = new SelfExtractorSaveOptions
                {
                    Flavor = Ionic.Zip.SelfExtractorFlavor.ConsoleApplication,
                    DefaultExtractDirectory = unpackDir
                };
                zip.SaveSelfExtractor(exeFileToCreate, sfxOptions);
            }

            var psi = new System.Diagnostics.ProcessStartInfo(exeFileToCreate);
            psi.WorkingDirectory = TopLevelDir;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit();

            // now, compare the output in unpackDir with the original
            string DirToCheck = Path.Combine(unpackDir, "A");
            // verify the checksum of each file matches with its brother
            foreach (string fname in Directory.GetFiles(DirToCheck))
            {
                string originalName = fname.Replace("\\unpack", "");
                if (checksums.ContainsKey(originalName))
                {
                    string expectedCheckString = checksums[originalName];
                    string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(fname));
                    Assert.AreEqual<String>(expectedCheckString, actualCheckString, "Unexpected checksum on extracted filesystem file ({0}).", fname);
                }
                else
                    Assert.AreEqual<string>("Readme.txt", originalName);

            }
        }


        [TestMethod]
        public void SFX_WinForms()
        {
            string[] Passwords = { null, "12345" };
            for (int k = 0; k < Passwords.Length; k++)
            {
                string exeFileToCreate = Path.Combine(TopLevelDir, String.Format("SFX_WinForms-{0}.exe", k));
                string DesiredunpackDir = Path.Combine(TopLevelDir, String.Format("unpack{0}", k));

                String filename = null;

                string Subdir = Path.Combine(TopLevelDir, String.Format("A{0}", k));
                Directory.CreateDirectory(Subdir);
                var checksums = new Dictionary<string, string>();

                int fileCount = _rnd.Next(10) + 10;
                for (int j = 0; j < fileCount; j++)
                {
                    filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                    var chk = TestUtilities.ComputeChecksum(filename);
                    checksums.Add(filename, TestUtilities.CheckSumToString(chk));
                }

                using (ZipFile zip = new ZipFile())
                {
                    zip.Password = Passwords[k];
                    zip.AddDirectory(Subdir, Path.GetFileName(Subdir));
                    zip.Comment = "For testing purposes, please extract to:  " + DesiredunpackDir;
                    if (Passwords[k] != null) zip.Comment += String.Format("\r\n\r\nThe password for all entries is:  {0}\n", Passwords[k]);
                    var sfxOptions = new SelfExtractorSaveOptions
                    {
                        Flavor = Ionic.Zip.SelfExtractorFlavor.WinFormsApplication,
                        // workitem 12608
                        SfxExeWindowTitle = "Custom SFX Title " + DateTime.Now.ToString("G"),
                        DefaultExtractDirectory = DesiredunpackDir
                    };
                    zip.SaveSelfExtractor(exeFileToCreate, sfxOptions);
                }

                // run the self-extracting EXE we just created
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(exeFileToCreate);
                psi.Arguments = DesiredunpackDir;
                psi.WorkingDirectory = TopLevelDir;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);
                process.WaitForExit();

                // now, compare the output in TargetDirectory with the original
                string DirToCheck = Path.Combine(DesiredunpackDir, String.Format("A{0}", k));
                // verify the checksum of each file matches with its brother
                var fileList = Directory.GetFiles(DirToCheck);
                Assert.AreEqual<Int32>(checksums.Keys.Count, fileList.Length, "Trial {0}: Inconsistent results.", k);

                foreach (string fname in fileList)
                {
                    string expectedCheckString = checksums[fname.Replace(String.Format("\\unpack{0}", k), "")];
                    string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(fname));
                    Assert.AreEqual<String>(expectedCheckString, actualCheckString, "Trial {0}: Unexpected checksum on extracted filesystem file ({1}).", k, fname);
                }
            }
        }



        string programCode =

            "using System;\n" +
            "namespace Ionic.Tests.Zip.SelfExtractor\n" +
            "{\n" +
            "\n" +
            "    public class TestDriver\n" +
            "    {\n" +
            "        static int Main(String[] args)\n" +
            "        {\n" +
            "            int rc = @@XXX@@;\n" +
            "            Console.WriteLine(\"Hello from the post-extract command.\\nThis app will return {0}.\", rc);\n" +
            "            return rc;\n" +
            "        }\n" +
            "    }\n" +
            "}\n";


        private void CompileApp(int rc, string pathToExe)
        {
            var csharp = new Microsoft.CSharp.CSharpCodeProvider();
            var cp = new System.CodeDom.Compiler.CompilerParameters
            {
                GenerateInMemory = false,
                GenerateExecutable = true,
                IncludeDebugInformation = false,
                OutputAssembly = pathToExe
            };

            // set the return code in the app
            var cr = csharp.CompileAssemblyFromSource
                (cp, programCode.Replace("@@XXX@@", rc.ToString()));

            if (cr == null)
                throw new Exception("Errors compiling post-extract exe!");

            foreach (string s in cr.Output)
                TestContext.WriteLine(s);

            if (cr.Errors.Count != 0)
                throw new Exception("Errors compiling post-extract exe!");
        }


        // Here's a set of SFX tests with post-extract EXEs.
        // We vary these parameters:
        //  - exe exists or not - 2 trials each test.
        //  - exe name has spaces or not
        //  - winforms or not
        //  - whether to run the exe or just compile
        //  - whether to append args or not
        //  - force noninteractive or not (only for Winforms flavor, to allow automated tests)

        [TestMethod]
        public void SFX_RunOnExit_Console()
        {
            _Internal_SelfExtractor_Command("post-extract-run-on-exit-{0:D4}.exe",
                                            SelfExtractorFlavor.ConsoleApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_Console_Args()
        {
            _Internal_SelfExtractor_Command("post-extract-run-on-exit-{0:D4}.exe",
                                            SelfExtractorFlavor.ConsoleApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            true); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms()
        {
            _Internal_SelfExtractor_Command("post-extract-run-on-exit-{0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_DontRun()
        {
            // This test case just tests the generation (compilation) of
            // the SFX.  It is included because the interactive winforms
            // SFX is not performed on automated test runs.
            _Internal_SelfExtractor_Command("post-extract-run-on-exit-{0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            false,  // runPostExtract
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_Interactive()
        {
            _Internal_SelfExtractor_Command("post-extract-run-on-exit-{0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // runPostExtract
                                            false,  // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_NonInteractive()
        {
            _Internal_SelfExtractor_Command("post-extract-run-on-exit-{0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            true,   // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_NonInteractive_Args()
        {
            _Internal_SelfExtractor_Command("post-extract-run-on-exit-{0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            true,   // forceNoninteractive
                                            true); // wantArgs
        }

        // ------------------------------------------------------------------ //

        [TestMethod]
        public void SFX_RunOnExit_Console_withSpaces()
        {
            _Internal_SelfExtractor_Command("post extract run on exit {0:D4}.exe",
                                            SelfExtractorFlavor.ConsoleApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }


        [TestMethod]
        public void SFX_RunOnExit_Console_withSpaces_Args()
        {
            _Internal_SelfExtractor_Command("post extract run on exit {0:D4}.exe",
                                            SelfExtractorFlavor.ConsoleApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            true);  // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_withSpaces()
        {
            _Internal_SelfExtractor_Command("post extract run on exit {0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // runPostExtract
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_withSpaces_DontRun()
        {
            // This test case just tests the generation (compilation) of
            // the SFX.  It is included because the interactive winforms
            // SFX is not performed on automated test runs.
            _Internal_SelfExtractor_Command("post extract run on exit {0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            false,  // run the SFX?
                                            true,   // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_withSpaces_Interactive()
        {
            _Internal_SelfExtractor_Command("post extract run on exit {0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // actually run the program
                                            false,  // quiet
                                            false,  // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_withSpaces_NonInteractive()
        {
            _Internal_SelfExtractor_Command("post extract run on exit {0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // actually run the program
                                            true,   // quiet
                                            true,   // forceNoninteractive
                                            false); // wantArgs
        }

        [TestMethod]
        public void SFX_RunOnExit_WinForms_withSpaces_NonInteractive_Args()
        {
            _Internal_SelfExtractor_Command("post extract run on exit {0:D4}.exe",
                                            SelfExtractorFlavor.WinFormsApplication,
                                            true,   // actually run the program
                                            true,   // quiet
                                            true,   // forceNoninteractive
                                            true);  // wantArgs
        }


        public void _Internal_SelfExtractor_Command(string cmdFormat,
                                                    SelfExtractorFlavor flavor,
                                                    bool runSfx,
                                                    bool quiet,
                                                    bool forceNoninteractive,
                                                    bool wantArgs)
        {
            TestContext.WriteLine("==============================");
            TestContext.WriteLine("SFX_RunOnExit({0})", flavor.ToString());

            //int entriesAdded = 0;
            //String filename = null;
            string postExtractExe = String.Format(cmdFormat, _rnd.Next(3000));

            // If WinForms and want forceNoninteractive, have the post-extract-exe return 0,
            // else, select a random number.
            int expectedReturnCode = (forceNoninteractive &&
                                      flavor == SelfExtractorFlavor.WinFormsApplication)
                ? 0
                : _rnd.Next(1024) + 20;
            TestContext.WriteLine("The post-extract command ({0}) will return {1}",
                                  postExtractExe, expectedReturnCode);

            string subdir = "A";
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);

            for (int k = 0; k < 2; k++)
            {
                string readmeString =
                    String.Format("Hey! This zipfile entry was created directly from " +
                                  "a string in application code. Flavor ({0}) Trial({1})",
                                  flavor.ToString(), k);
                string exeFileToCreate = String.Format("SFX_Command.{0}.{1}.exe",
                                                       flavor.ToString(), k);

                TestContext.WriteLine("----------------------");
                TestContext.WriteLine("Trial {0}", k);
                string unpackDir = String.Format("unpack.{0}", k);

                var sw = new System.IO.StringWriter();
                using (ZipFile zip = new ZipFile())
                {
                    zip.StatusMessageTextWriter = sw;
                    zip.AddDirectory(subdir, subdir); // Path.GetFileName(subdir));
                    zip.Comment = String.Format("Trial options: fl({0})  cmd ({3})\r\n"+
                                                "actuallyRun({1})\r\nquiet({2})\r\n"+
                                                "exists? {4}\r\nexpected rc={5}",
                                                flavor,
                                                runSfx,
                                                quiet,
                                                postExtractExe,
                                                k!=0,
                                                expectedReturnCode
                                                );
                    var ms1 = new MemoryStream(Encoding.UTF8.GetBytes(readmeString));
                    zip.AddEntry("Readme.txt", ms1);
                    if (k != 0)
                    {
                        CompileApp(expectedReturnCode, postExtractExe);
                        zip.AddFile(postExtractExe);
                    }

                    var sfxOptions = new SelfExtractorSaveOptions
                    {
                        Flavor = flavor,
                        DefaultExtractDirectory = unpackDir,
                        SfxExeWindowTitle = "Custom SFX Title " + DateTime.Now.ToString("G"),
                        Quiet = quiet
                    };

                    // In the case of k==0, this exe does not exist.  It will result in
                    // a return code of 5.  In k == 1, the exe exists and will succeed.
                    if (postExtractExe.Contains(' '))
                        sfxOptions.PostExtractCommandLine= "\"" + postExtractExe + "\"";
                    else
                        sfxOptions.PostExtractCommandLine= postExtractExe;

                    if (wantArgs)
                        sfxOptions.PostExtractCommandLine += " arg1 arg2";

                    zip.SaveSelfExtractor(exeFileToCreate, sfxOptions);
                }

                TestContext.WriteLine("status output: " + sw.ToString());

                if (k != 0) File.Delete(postExtractExe);

                // Run the generated Self-extractor, conditionally.
                //
                // We always run, unless specifically asked not to, OR if it's a
                // winforms app and we want it to be noninteractive and there's no
                // EXE to run.  If we try running a non-existent app, it will pop an
                // error message, hence user interaction, which we need to avoid for
                // the automated test.
                if (runSfx &&
                    (k != 0 || !forceNoninteractive ||
                     flavor != SelfExtractorFlavor.WinFormsApplication))
                {
                    TestContext.WriteLine("Running the SFX... ");
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(exeFileToCreate);
                    psi.WorkingDirectory = TopLevelDir;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true; // false;
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);
                    process.WaitForExit();
                    int rc = process.ExitCode;
                    TestContext.WriteLine("SFX exit code: ({0})", rc);

                    // The exit code is returned only if it's a console SFX.
                    if (flavor == SelfExtractorFlavor.ConsoleApplication)
                    {
                        // The program actually runs if k != 0
                        if (k == 0)
                        {
                            // The file to execute should not have been found, hence rc==5.
                            Assert.AreEqual<Int32>
                                (5, rc, "In trial {0}, the exit code was unexpected.", k);
                        }
                        else
                        {
                            // The file to execute should have returned a specific code.
                            Assert.AreEqual<Int32>
                                (expectedReturnCode, rc,
                                 "In trial {0}, the exit code did not match.", k);
                        }
                    }
                    else
                        Assert.AreEqual<Int32>(0, rc, "In trial {0}, the exit code did not match.", k);

                    VerifyChecksums(Path.Combine(unpackDir, "A"),
                                    filesToZip, checksums);
                }
            }
        }



        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.BadStateException))]
        public void SFX_Save_Zip_As_EXE()
        {
            string sfxFileToCreate = "SFX_Save_Zip_As_EXE.exe";

            // create a file to zip
            string subdir = "files";
            Directory.CreateDirectory(subdir);
            string filename = Path.Combine(subdir, "file1.txt");
            TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);

            // add an entry to the zipfile, then try saving to a directory. this should fail
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(filename, "");
                zip.SaveSelfExtractor(sfxFileToCreate,
                                      SelfExtractorFlavor.ConsoleApplication);
            }

            // create another file
            filename = Path.Combine(subdir, "file2.txt");
            TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);

            // update the SFX, save to a zip format
            using (ZipFile zip = ZipFile.Read(sfxFileToCreate))
            {
                zip.AddFile(filename, "");
                zip.Save();  // FAIL
            }
        }




        [TestMethod]
        public void SFX_RemoveFilesAfterUnpack_wi10682()
        {
            string subdir = "files";
            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateFilesAndChecksums(subdir, out filesToZip, out checksums);
            string password = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            string postExeFormat = "post-extract-{0:D4}.exe";
            string postExtractExe = String.Format(postExeFormat, _rnd.Next(10000));
            CompileApp(0, postExtractExe);

            // pass 1 to run SFX and verify files are present;
            // pass 2 to run SFX and verify that it deletes files after extracting.

            // 2 passes: one for no cmd line overload, one with overload of -r+/-r-
            for (int j=0; j < 2; j++)
            {
                // 2 passes: with RemoveUnpackedFiles set or unset
                for (int k=0; k < 2; k++)
                {
                    string sfxFileToCreate =
                        String.Format("SFX_RemoveFilesAfterUnpack.{0}.{1}.exe",j,k);
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.Password  = password;
                        zip.Encryption = Ionic.Zip.EncryptionAlgorithm.WinZipAes256;
                        Array.ForEach(filesToZip, x => { zip.AddFile(x, "files");});
                        zip.AddFile(postExtractExe, "files");
                        var sfxOptions = new SelfExtractorSaveOptions
                        {
                            Flavor = SelfExtractorFlavor.ConsoleApplication,
                            Quiet = true,
                            PostExtractCommandLine = Path.Combine("files",postExtractExe)
                        };

                        if (k==1)
                            sfxOptions.RemoveUnpackedFilesAfterExecute = true;

                        zip.SaveSelfExtractor(sfxFileToCreate, sfxOptions);
                    }

                    string extractDir = String.Format("extract.{0}.{1}",j,k);
                    string sfxCmdLineArgs =
                        String.Format("-p {0} -d {1}", password, extractDir);

                    if (j==1)
                    {
                        // override the option set at time of zip.SaveSfx()
                        sfxCmdLineArgs += (k==0) ? " -r+" : " -r-";
                    }

                    // invoke the SFX
                    this.Exec(sfxFileToCreate, sfxCmdLineArgs, true, true);

                    if (k==j)
                    {
                        // verify that the files are extracted, and match
                        VerifyChecksums(Path.Combine(extractDir, "files"),
                                        filesToZip, checksums);
                    }
                    else
                    {
                        // verify that no files exist in the extract directory
                        var remainingFiles = Directory.GetFiles(extractDir);
                        Assert.IsTrue(remainingFiles.Length == 0);
                    }
                }
            }
        }




    }
}