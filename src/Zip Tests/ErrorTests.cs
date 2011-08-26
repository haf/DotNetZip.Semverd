// ErrorTests.cs
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
// Time-stamp: <2011-July-28 12:58:36>
//
// ------------------------------------------------------------------
//
// This module defines some "error tests" - tests that the expected
// errors or exceptions occur in DotNetZip under exceptional conditions.
// These conditions include corrupted zip files, bad input, and so on.
//
// ------------------------------------------------------------------

using System;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;


namespace Ionic.Zip.Tests.Error
{
    /// <summary>
    /// Summary description for ErrorTests
    /// </summary>
    [TestClass]
    public class ErrorTests : IonicTestClass
    {
        public ErrorTests() : base() { }


        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Error_AddFile_NonExistentFile()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "Error_AddFile_NonExistentFile.zip");
            using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipFileToCreate))
            {
                zip.AddFile("ThisFileDoesNotExist.txt");
                zip.Comment = "This is a Comment On the Archive";
                zip.Save();
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void Error_Read_NullStream()
        {
            System.IO.Stream s = null;
            using (var zip = ZipFile.Read(s))
            {
                foreach (var e in zip)
                {
                    Console.WriteLine("entry: {0}", e.FileName);
                }
            }
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void CreateZip_AddDirectory_BlankName()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddDirectory_BlankName.zip");
            using (ZipFile zip = new ZipFile(zipFileToCreate))
            {
                zip.AddDirectoryByName("");
                zip.Save();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void CreateZip_AddEntry_String_BlankName()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "CreateZip_AddEntry_String_BlankName.zip");
            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry("", "This is the content.");
                zip.Save(zipFileToCreate);
            }
        }


        private void OverwriteDecider(object sender, ExtractProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Extracting_ExtractEntryWouldOverwrite:
                    // randomly choose whether to overwrite or not
                    e.CurrentEntry.ExtractExistingFile = (_rnd.Next(2) == 0)
                        ? ExtractExistingFileAction.DoNotOverwrite
                        : ExtractExistingFileAction.OverwriteSilently;
                    break;
            }
        }



        private void _Internal_ExtractExisting(int flavor)
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("Error-Extract-ExistingFileWithoutOverwrite-{0}.zip", flavor));

            string testBin = TestUtilities.GetTestBinDir(CurrentDir);
            string resourceDir = Path.Combine(testBin, "Resources");

            Assert.IsTrue(Directory.Exists(resourceDir));

            Directory.SetCurrentDirectory(TopLevelDir);
            var filenames = Directory.GetFiles(resourceDir);

            using (ZipFile zip = new ZipFile(zipFileToCreate))
            {
                zip.AddFiles(filenames, "");
                zip.Comment = "This is a Comment On the Archive";
                zip.Save();
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filenames.Length,
                                 "The zip file created has the wrong number of entries.");

            // Extract twice: the first time should succeed.
            // The second, should fail, because of a failed file overwrite.
            // Unless flavor==3, in which case we overwrite silently.
            for (int k = 0; k < 2; k++)
            {
                using (ZipFile zip = ZipFile.Read(zipFileToCreate))
                {
                    if (flavor > 10)
                        zip.ExtractProgress += OverwriteDecider;
                    for (int j = 0; j < filenames.Length; j++)
                    {
                        ZipEntry e = zip[Path.GetFileName(filenames[j])];
                        if (flavor == 4)
                            e.Extract("unpack");
                        else
                            e.Extract("unpack", (ExtractExistingFileAction) flavor);
                    }
                }
            }
        }



        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Error_Extract_ExistingFileWithoutOverwrite_Throw()
        {
            _Internal_ExtractExisting((int)ExtractExistingFileAction.Throw);
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Error_Extract_ExistingFileWithoutOverwrite_NoArg()
        {
            _Internal_ExtractExisting(4);
        }


        // not an error test
        [TestMethod]
        public void Extract_ExistingFileWithOverwrite_OverwriteSilently()
        {
            _Internal_ExtractExisting((int)ExtractExistingFileAction.OverwriteSilently);
        }

        // not an error test
        [TestMethod]
        public void Extract_ExistingFileWithOverwrite_DoNotOverwrite()
        {
            _Internal_ExtractExisting((int)ExtractExistingFileAction.DoNotOverwrite);
        }

        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Error_Extract_ExistingFileWithoutOverwrite_InvokeProgress()
        {
            _Internal_ExtractExisting((int)ExtractExistingFileAction.InvokeExtractProgressEvent);
        }

        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Error_Extract_ExistingFileWithoutOverwrite_InvokeProgress_2()
        {
            _Internal_ExtractExisting(10+(int)ExtractExistingFileAction.InvokeExtractProgressEvent);
        }



        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Error_Extract_ExistingFileWithoutOverwrite_7()
        {
            // this is a test of the test!
            _Internal_ExtractExisting(7);
        }


        [TestMethod]
        public void Error_EmptySplitZip()
        {
            string zipFileToCreate = "zftc.zip";
            using (var zip = new ZipFile())
            {
                zip.MaxOutputSegmentSize = 1024*1024;
                zip.Save(zipFileToCreate);
            }

            string extractDir = "extract";
            using (var zip = ZipFile.Read(zipFileToCreate))
            {
                zip.ExtractAll(extractDir);
                Assert.IsTrue(zip.Entries.Count == 0);
            }
        }



        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void Error_Read_InvalidZip()
        {
            string filename = zipit;
            // try reading the invalid zipfile - this must fail.
            using (ZipFile zip = ZipFile.Read(filename))
            {
                foreach (ZipEntry e in zip)
                {
                    System.Console.WriteLine("name: {0}  compressed: {1} has password?: {2}",
                        e.FileName, e.CompressedSize, e.UsesEncryption);
                }
            }
        }



        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void Error_NonZipFile_wi11743()
        {
            // try reading an empty, extant file as a zip file
            string zipFileToRead = Path.GetTempFileName();
            _FilesToRemove.Add(zipFileToRead);
            using (ZipFile zip = new ZipFile(zipFileToRead))
            {
                zip.AddEntry("EntryName1.txt", "This is the content");
                zip.Save();
            }

        }


        private void CreateSmallZip(string zipFileToCreate)
        {
            string sourceDir = CurrentDir;
            for (int i = 0; i < 3; i++)
                sourceDir = Path.GetDirectoryName(sourceDir);

            // the list of filenames to add to the zip
            string[] fileNames =
                {
                    Path.Combine(sourceDir, "Tools\\Zipit\\bin\\Debug\\Zipit.exe"),
                    Path.Combine(sourceDir, "Zip\\bin\\Debug\\Ionic.Zip.xml"),
                    Path.Combine(sourceDir, "Tools\\WinFormsApp\\Icon2.res"),
                };

            using (ZipFile zip = new ZipFile())
            {
                for (int j = 0; j < fileNames.Length; j++)
                    zip.AddFile(fileNames[j], "");
                zip.Save(zipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 fileNames.Length,
                                 "Wrong number of entries.");
        }

        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void MalformedZip()
        {
            string filePath = Path.GetTempFileName();
            _FilesToRemove.Add(filePath);
            File.WriteAllText( filePath , "asdfasdf" );
            string outputDirectory = Path.GetTempPath();
            using ( ZipFile zipFile = ZipFile.Read( filePath ) )
            {
                zipFile.ExtractAll( outputDirectory  );
            }
        }



        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Error_UseZipEntryExtractWith_ZIS_wi10355()
        {
            string zipFileToCreate = "UseOpenReaderWith_ZIS.zip";
            CreateSmallZip(zipFileToCreate);

            // mixing ZipEntry.Extract and ZipInputStream is a no-no!!

            string extractDir = "extract";

            // Use ZipEntry.Extract with ZipInputStream.
            // This must fail.
            TestContext.WriteLine("Reading with ZipInputStream");
            using (var zip = new ZipInputStream(zipFileToCreate))
            {
                ZipEntry entry;
                while ((entry = zip.GetNextEntry()) != null)
                {
                    entry.Extract(extractDir, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }




        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Error_UseOpenReaderWith_ZIS_wi10923()
        {
            string zipFileToCreate = "UseOpenReaderWith_ZIS.zip";
            CreateSmallZip(zipFileToCreate);

            // mixing OpenReader and ZipInputStream is a no-no!!
            int n;
            var buffer = new byte[2048];

            // Use OpenReader with ZipInputStream.
            // This must fail.
            TestContext.WriteLine("Reading with ZipInputStream");
            using (var zip = new ZipInputStream(zipFileToCreate))
            {
                ZipEntry entry;
                while ((entry = zip.GetNextEntry()) != null)
                {
                    TestContext.WriteLine("  Entry: {0}", entry.FileName);
                    using (Stream file = entry.OpenReader())
                    {
                        while((n= file.Read(buffer,0,buffer.Length)) > 0) ;
                    }
                    TestContext.WriteLine("  -- OpenReader() is done. ");
                }
            }
        }



        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void Error_Save_InvalidLocation()
        {
            string badLocation = "c:\\Windows\\";
            Assert.IsTrue(Directory.Exists(badLocation));

            // Add an entry to the zipfile, then try saving to a directory.
            // This must fail.
            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry("This is a file.txt", "Content for the file goes here.");
                zip.Save(badLocation);  // fail
            }
        }


        [TestMethod]
        public void Error_Save_NonExistentFile()
        {
            int j;
            string repeatedLine;
            string filename;
            string zipFileToCreate = Path.Combine(TopLevelDir, "Error_Save_NonExistentFile.zip");

            // create the subdirectory
            string Subdir = Path.Combine(TopLevelDir, "DirToZip");
            Directory.CreateDirectory(Subdir);

            int entriesAdded = 0;
            // create the files
            int numFilesToCreate = _rnd.Next(20) + 18;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = Path.Combine(Subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(1800) + 1500);
                entriesAdded++;
            }

            string tempFileFolder = Path.Combine(TopLevelDir, "Temp");
            Directory.CreateDirectory(tempFileFolder);
            TestContext.WriteLine("Using {0} as the temp file folder....", tempFileFolder);
            String[] tfiles = Directory.GetFiles(tempFileFolder);
            int nTemp = tfiles.Length;
            TestContext.WriteLine("There are {0} files in the temp file folder.", nTemp);
            String[] filenames = Directory.GetFiles(Subdir);

            var a1 = System.Reflection.Assembly.GetExecutingAssembly();
            String myName = a1.GetName().ToString();
            string toDay = System.DateTime.Now.ToString("yyyy-MMM-dd");

            try
            {
                using (ZipFile zip = new ZipFile(zipFileToCreate))
                {
                    zip.TempFileFolder = tempFileFolder;
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;

                    TestContext.WriteLine("Zipping {0} files...", filenames.Length);

                    int count = 0;
                    foreach (string fn in filenames)
                    {
                        count++;
                        TestContext.WriteLine("  {0}", fn);

                        string file = fn;

                        if (count == filenames.Length - 2)
                        {
                            file += "xx";
                            TestContext.WriteLine("(Injecting a failure...)");
                        }

                        zip.UpdateFile(file, myName + '-' + toDay + "_done");
                    }
                    TestContext.WriteLine("\n");
                    zip.Save();
                    TestContext.WriteLine("Zip Completed '{0}'", zipFileToCreate);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine("Zip Failed (EXPECTED): {0}", ex.Message);
            }

            tfiles = Directory.GetFiles(tempFileFolder);

            Assert.AreEqual<int>(nTemp, tfiles.Length,
                    "There are unexpected files remaining in the TempFileFolder.");
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.BadStateException))]
        public void Error_Save_NoFilename()
        {
            string testBin = TestUtilities.GetTestBinDir(CurrentDir);
            string resourceDir = Path.Combine(testBin, "Resources");
            Directory.SetCurrentDirectory(TopLevelDir);
            string filename = Path.Combine(resourceDir, "TestStrings.txt");
            Assert.IsTrue(File.Exists(filename), String.Format("The file '{0}' doesnot exist.", filename));

            // add an entry to the zipfile, then try saving, never having specified a filename. This should fail.
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(filename, "");
                zip.Save(); // FAIL: don't know where to save!
            }

            // should never reach this
            Assert.IsTrue(false);
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.BadStateException))]
        public void Error_Extract_WithoutSave()
        {
            string testBin = TestUtilities.GetTestBinDir(CurrentDir);
            string resourceDir = Path.Combine(testBin, "Resources");
            Directory.SetCurrentDirectory(TopLevelDir);

            // add a directory to the zipfile, then try
            // extracting, without a Save. This should fail.
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(resourceDir, "");
                Assert.IsTrue(zip.Entries.Count > 0);
                zip[0].Extract();  // FAIL: has not been saved
            }

            // should never reach this
            Assert.IsTrue(false);
        }

        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.BadStateException))]
        public void Error_Read_WithoutSave()
        {
            string testBin = TestUtilities.GetTestBinDir(CurrentDir);
            string resourceDir = Path.Combine(testBin, "Resources");
            Directory.SetCurrentDirectory(TopLevelDir);

            // add a directory to the zipfile, then try
            // extracting, without a Save. This should fail.
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(resourceDir, "");
                Assert.IsTrue(zip.Entries.Count > 0);

                using (var s = zip[0].OpenReader()) // FAIL: has not been saved
                {
                    byte[] buffer= new byte[1024];
                    int n;
                    while ((n= s.Read(buffer,0,buffer.Length)) > 0) ;
                }
            }

            // should never reach this
            Assert.IsTrue(false);
        }


        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public void Error_AddDirectory_SpecifyingFile()
        {
            string zipFileToCreate = "AddDirectory_SpecifyingFile.zip";
            string filename = "ThisIsAFile";
            File.Copy(zipit, filename);
            string baddirname = Path.Combine(TopLevelDir, filename);
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(baddirname); // FAIL
                zip.Save(zipFileToCreate);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Error_AddFile_SpecifyingDirectory()
        {
            string zipFileToCreate = "AddFile_SpecifyingDirectory.zip";
            string badfilename = "ThisIsADirectory.txt";
            Directory.CreateDirectory(badfilename);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(badfilename); // should fail
                zip.Save(zipFileToCreate);
            }
        }

        private void IntroduceCorruption(string filename)
        {
            // now corrupt the zip archive
            using (FileStream fs = File.OpenWrite(filename))
            {
                byte[] corruption = new byte[_rnd.Next(100) + 12];
                int min = 5;
                int max = (int)fs.Length - 20;
                int offsetForCorruption, lengthOfCorruption;

                int numCorruptions = _rnd.Next(2) + 2;
                for (int i = 0; i < numCorruptions; i++)
                {
                    _rnd.NextBytes(corruption);
                    offsetForCorruption = _rnd.Next(min, max);
                    lengthOfCorruption = _rnd.Next(2) + 3;
                    fs.Seek(offsetForCorruption, SeekOrigin.Begin);
                    fs.Write(corruption, 0, lengthOfCorruption);
                }
            }
        }


        [TestMethod]
        [ExpectedException(typeof(ZipException))] // not sure which exception - could be one of several.
        public void Error_ReadCorruptedZipFile_Passwords()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "Read_CorruptedZipFile_Passwords.zip");
            string sourceDir = CurrentDir;
            for (int i = 0; i < 3; i++)
                sourceDir = Path.GetDirectoryName(sourceDir);

            Directory.SetCurrentDirectory(TopLevelDir);

            // the list of filenames to add to the zip
            string[] filenames =
            {
                Path.Combine(sourceDir, "Tools\\Zipit\\bin\\Debug\\Zipit.exe"),
                Path.Combine(sourceDir, "Zip\\bin\\Debug\\Ionic.Zip.xml"),
                Path.Combine(sourceDir, "Tools\\WinFormsApp\\Icon2.res"),
            };

            // passwords to use for those entries
            string[] passwords = { "12345678", "0987654321", };

            // create the zipfile, adding the files
            int j = 0;
            using (ZipFile zip = new ZipFile())
            {
                for (j = 0; j < filenames.Length; j++)
                {
                    zip.Password = passwords[j % passwords.Length];
                    zip.AddFile(filenames[j], "");
                }
                zip.Save(zipFileToCreate);
            }

            IntroduceCorruption(zipFileToCreate);

            try
            {
                // read the corrupted zip - this should fail in some way
                using (ZipFile zip = ZipFile.Read(zipFileToCreate))
                {
                    for (j = 0; j < filenames.Length; j++)
                    {
                        ZipEntry e = zip[Path.GetFileName(filenames[j])];

                        System.Console.WriteLine("name: {0}  compressed: {1} has password?: {2}",
                            e.FileName, e.CompressedSize, e.UsesEncryption);
                        Assert.IsTrue(e.UsesEncryption, "The entry does not use encryption");
                        e.ExtractWithPassword("unpack", passwords[j % passwords.Length]);
                    }
                }
            }
            catch (Exception exc1)
            {
                throw new ZipException("expected", exc1);
            }
        }



        [TestMethod]
        [ExpectedException(typeof(ZipException))] // not sure which exception - could be one of several.
        public void Error_ReadCorruptedZipFile()
        {
            int i;
            string zipFileToCreate = Path.Combine(TopLevelDir, "Read_CorruptedZipFile.zip");

            string sourceDir = CurrentDir;
            for (i = 0; i < 3; i++)
                sourceDir = Path.GetDirectoryName(sourceDir);

            Directory.SetCurrentDirectory(TopLevelDir);

            // the list of filenames to add to the zip
            string[] filenames =
            {
                Path.Combine(sourceDir, "Tools\\Zipit\\bin\\Debug\\Zipit.exe"),
                Path.Combine(sourceDir, "Tools\\Unzip\\bin\\Debug\\Unzip.exe"),
                Path.Combine(sourceDir, "Zip\\bin\\Debug\\Ionic.Zip.xml"),
                Path.Combine(sourceDir, "Tools\\WinFormsApp\\Icon2.res"),
            };

            // create the zipfile, adding the files
            using (ZipFile zip = new ZipFile())
            {
                for (i = 0; i < filenames.Length; i++)
                    zip.AddFile(filenames[i], "");
                zip.Save(zipFileToCreate);
            }

            // now corrupt the zip archive
            IntroduceCorruption(zipFileToCreate);

            try
            {
                // read the corrupted zip - this should fail in some way
                using (ZipFile zip = new ZipFile(zipFileToCreate))
                {
                    foreach (var e in zip)
                    {
                        System.Console.WriteLine("name: {0}  compressed: {1} has password?: {2}",
                            e.FileName, e.CompressedSize, e.UsesEncryption);
                        e.Extract("extract");
                    }
                }
            }
            catch (Exception exc1)
            {
                throw new ZipException("expected", exc1);
            }
        }


        [TestMethod]
        public void Error_LockedFile_wi13903()
        {
            TestContext.WriteLine("==Error_LockedFile_wi13903()");
            string fname = Path.GetRandomFileName();
            TestContext.WriteLine("create file {0}", fname);
            TestUtilities.CreateAndFillFileText(fname, _rnd.Next(10000) + 5000);
            string zipFileToCreate = "wi13903.zip";

            var zipErrorHandler = new EventHandler<ZipErrorEventArgs>( (sender, e)  =>
                {
                    TestContext.WriteLine("Error reading entry {0}", e.CurrentEntry);
                    TestContext.WriteLine("  (this was expected)");
                    e.CurrentEntry.ZipErrorAction = ZipErrorAction.Skip;
                });

            // lock the file
            TestContext.WriteLine("lock file {0}", fname);
            using (var s = System.IO.File.Open(fname,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.None))
            {
                using (var rawOut = File.Create(zipFileToCreate))
                {
                    using (var nonSeekableOut = new Ionic.Zip.Tests.NonSeekableOutputStream(rawOut))
                    {
                        TestContext.WriteLine("create zip file {0}", zipFileToCreate);
                        using (var zip = new ZipFile())
                        {
                            zip.ZipError += zipErrorHandler;
                            zip.AddFile(fname);
                            // should trigger a read error,
                            // which should be skipped. Result will be
                            // a zero-entry zip file.
                            zip.Save(nonSeekableOut);
                        }
                    }
                }
            }
            TestContext.WriteLine("all done, A-OK");
        }


        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void Error_Read_EmptyZipFile()
        {
            string zipFileToRead = Path.Combine(TopLevelDir, "Read_BadFile.zip");
            string newFile = Path.GetTempFileName();
            _FilesToRemove.Add(newFile);
            File.Move(newFile, zipFileToRead);
            newFile = Path.GetTempFileName();
            _FilesToRemove.Add(newFile);
            string fileToAdd = Path.Combine(TopLevelDir, "EmptyFile.txt");
            File.Move(newFile, fileToAdd);

            try
            {
                using (ZipFile zip = ZipFile.Read(zipFileToRead))
                {
                    zip.AddFile(fileToAdd, "");
                    zip.Save();
                }
            }
            catch (System.Exception exc1)
            {
              throw new ZipException("expected", exc1);
            }

        }



        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Error_AddFile_Twice()
        {
            int i;
            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "Error_AddFile_Twice.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "files");
            Directory.CreateDirectory(subdir);

            // create a bunch of files
            int numFilesToCreate = _rnd.Next(23) + 14;
            for (i = 0; i < numFilesToCreate; i++)
                TestUtilities.CreateUniqueFile("bin", subdir, _rnd.Next(10000) + 5000);

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile(zipFileToCreate))
            {
                zip1.StatusMessageTextWriter = System.Console.Out;
                string[] files = Directory.GetFiles(subdir);
                zip1.AddFiles(files, "files");
                zip1.Save();
            }


            // this should fail - adding the same file twice
            using (ZipFile zip2 = new ZipFile(zipFileToCreate))
            {
                zip2.StatusMessageTextWriter = System.Console.Out;
                string[] files = Directory.GetFiles(subdir);
                for (i = 0; i < files.Length; i++)
                    zip2.AddFile(files[i], "files");
                zip2.Save();
            }
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Error_FileNotAvailableFails()
        {
            // verify the correct exception is being thrown
            string zipFileToCreate = Path.Combine(TopLevelDir, "Error_FileNotAvailableFails.zip");

            // create a zip file with no entries
            using (var zipfile = new ZipFile(zipFileToCreate)) { zipfile.Save(); }

            // open and lock
            using (new FileStream(zipFileToCreate, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                using (new ZipFile(zipFileToCreate)) { }
            }
        }



        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void IncorrectZipContentTest1_wi10459()
        {
            byte[] content = Encoding.UTF8.GetBytes("wrong zipfile content");
            using (var ms = new MemoryStream(content))
            {
                using (var zipFile = ZipFile.Read(ms)) { }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void IncorrectZipContentTest2_wi10459()
        {
            using (var ms = new MemoryStream())
            {
                using (var zipFile = ZipFile.Read(ms)) { }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ZipException))]
        public void IncorrectZipContentTest3_wi10459()
        {
            byte[] content = new byte[8192];
            using (var ms = new MemoryStream(content))
            {
                using (var zipFile = ZipFile.Read(ms)) { }
            }
        }

    }
}
