// UpdateTests.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa
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
// Time-stamp: <2011-August-05 16:52:59>
//
// ------------------------------------------------------------------
//
// This module defines tests for updating zip files via DotNetZip.
//
// ------------------------------------------------------------------

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;

namespace Ionic.Zip.Tests.Update
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UpdateTests : IonicTestClass
    {
        public UpdateTests() : base() { }

        [TestMethod]
        public void UpdateZip_AddNewDirectory()
        {
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_AddNewDirectory.zip");

            String CommentOnArchive = "BasicTests::UpdateZip_AddNewDirectory(): This archive will be overwritten.";

            string newComment = "This comment has been OVERWRITTEN." + DateTime.Now.ToString("G");
            string dirToZip = Path.Combine(TopLevelDir, "zipup");

            int i, j;
            int entries = 0;
            string subdir = null;
            String filename = null;
            int subdirCount = _rnd.Next(4) + 4;
            for (i = 0; i < subdirCount; i++)
            {
                subdir = Path.Combine(dirToZip, "Directory." + i);
                Directory.CreateDirectory(subdir);

                int fileCount = _rnd.Next(3) + 3;
                for (j = 0; j < fileCount; j++)
                {
                    filename = Path.Combine(subdir, "file" + j + ".txt");
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(12000) + 5000);
                    entries++;
                }
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(dirToZip);
                zip.Comment = CommentOnArchive;
                zip.Save(zipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entries,
                    "The created Zip file has an unexpected number of entries.");

            BasicVerifyZip(zipFileToCreate);

            // Now create a new subdirectory and add that one
            subdir = Path.Combine(TopLevelDir, "NewSubDirectory");
            Directory.CreateDirectory(subdir);

            filename = Path.Combine(subdir, "newfile.txt");
            TestUtilities.CreateAndFillFileText(filename, _rnd.Next(12000) + 5000);
            entries++;

            using (ZipFile zip = new ZipFile(zipFileToCreate))
            {
                zip.AddDirectory(subdir);
                zip.Comment = newComment;
                // this will add entries into the existing zip file
                zip.Save();
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entries,
                    "The overwritten Zip file has the wrong number of entries.");

            using (ZipFile readzip = new ZipFile(zipFileToCreate))
            {
                Assert.AreEqual<string>(newComment,
                                        readzip.Comment,
                                        "The zip comment is incorrect.");
            }
        }



        [TestMethod]
        public void UpdateZip_ChangeMetadata_AES()
        {
            Directory.SetCurrentDirectory(TopLevelDir);
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_ChangeMetadata_AES.zip");
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int numFilesToCreate = _rnd.Next(13) + 24;
            //int numFilesToCreate = 2;
            string filename = null;
            for (int j = 0; j < numFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                //TestUtilities.CreateAndFillFileText(filename, 500);
            }

            string password = Path.GetRandomFileName() + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            using (var zip = new ZipFile())
            {
                zip.Password = password;
                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                zip.AddFiles(Directory.GetFiles(subdir), "");
                zip.Save(zipFileToCreate);
            }

            // Verify the correct number of files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), numFilesToCreate,
                                 "Fie! The updated Zip file has the wrong number of entries.");

            // test extract (and implicitly check CRCs, passwords, etc)
            VerifyZip(zipFileToCreate, password);

            byte[] buffer = new byte[_rnd.Next(10000) + 10000];
            _rnd.NextBytes(buffer);
            using (var zip = ZipFile.Read(zipFileToCreate))
            {
                // modify the metadata for an entry
                zip[0].LastModified = DateTime.Now - new TimeSpan(7 * 31, 0, 0);
                zip.Password = password;
                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                zip.AddEntry(Path.GetRandomFileName(), buffer);
                zip.Save();
            }

            // Verify the correct number of files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), numFilesToCreate + 1,
                                 "Fie! The updated Zip file has the wrong number of entries.");

            // test extract (and implicitly check CRCs, passwords, etc)
            VerifyZip(zipFileToCreate, password);
        }



        private void VerifyZip(string zipfile, string password)
        {
            Stream bitBucket = Stream.Null;
            TestContext.WriteLine("Checking file {0}", zipfile);
            using (ZipFile zip = ZipFile.Read(zipfile))
            {
                zip.Password = password;
                zip.BufferSize = 65536;
                foreach (var s in zip.EntryFileNames)
                {
                    TestContext.WriteLine("  Entry: {0}", s);
                    zip[s].Extract(bitBucket);
                }
            }
            System.Threading.Thread.Sleep(0x500);
        }



        [TestMethod]
        public void UpdateZip_RemoveEntry_ByLastModTime()
        {
            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_RemoveEntry_ByLastModTime.zip");
            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int numFilesToCreate = _rnd.Next(13) + 24;
            string filename = null;
            int entriesAdded = 0;
            for (int j = 0; j < numFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Add the files to the zip, save the zip
            Directory.SetCurrentDirectory(TopLevelDir);
            int ix = 0;
            System.DateTime origDate = new System.DateTime(2007, 1, 15, 12, 1, 0);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                {
                    ZipEntry e = zip1.AddFile(f, "");
                    e.LastModified = origDate + new TimeSpan(24 * 31 * ix, 0, 0);  // 31 days * number of entries
                    ix++;
                }
                zip1.Comment = "UpdateTests::UpdateZip_RemoveEntry_ByLastModTime(): This archive will soon be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");


            // selectively remove a few files in the zip archive
            var threshold = new TimeSpan(24 * 31 * (2 + _rnd.Next(ix - 12)), 0, 0);
            int numRemoved = 0;
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                // We cannot remove the entry from the list, within the context of
                // an enumeration of said list.
                // So we add the doomed entry to a list to be removed
                // later.
                // pass 1: mark the entries for removal
                var entriesToRemove = new List<ZipEntry>();
                foreach (ZipEntry e in zip2)
                {
                    if (e.LastModified < origDate + threshold)
                    {
                        entriesToRemove.Add(e);
                        numRemoved++;
                    }
                }

                // pass 2: actually remove the entry.
                foreach (ZipEntry zombie in entriesToRemove)
                    zip2.RemoveEntry(zombie);

                zip2.Comment = "UpdateTests::UpdateZip_RemoveEntry_ByLastModTime(): This archive has been updated.";
                zip2.Save();
            }

            // Verify the correct number of files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded - numRemoved,
                "Fie! The updated Zip file has the wrong number of entries.");

            // verify that all entries in the archive are within the threshold
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (ZipEntry e in zip3)
                    Assert.IsTrue((e.LastModified >= origDate + threshold),
                        "Merde. The updated Zip file has entries that lie outside the threshold.");
            }

        }


        [TestMethod]
        public void UpdateZip_RemoveEntry_ByFilename_WithPassword()
        {
            string password = "*!ookahoo";
            string filename = null;
            int entriesToBeAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string zipFileToCreate = "ByFilename_WithPassword.zip";
            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create a bunch of files, fill them with content
            int numFilesToCreate = _rnd.Next(13) + 24;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = String.Format("file{0:D3}.txt", j);
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                                 filename);
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                entriesToBeAdded++;
            }

            // Add the files to the zip, save the zip
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                zip1.Password = password;
                zip1.AddFiles(filenames, "");

                zip1.Comment = "UpdateTests::UpdateZip_RemoveEntry_ByFilename_WithPassword(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesToBeAdded,
                "The Zip file has the wrong number of entries.");


            // selectively remove a few files in the zip archive
            var filesToRemove = new List<string>();
            int numToRemove = _rnd.Next(numFilesToCreate - 4) + 1;
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                for (j = 0; j < numToRemove; j++)
                {
                    // select a new, uniquely named file to create
                    do
                    {
                        filename = String.Format("file{0:D3}.txt", _rnd.Next(numFilesToCreate));
                    } while (filesToRemove.Contains(filename));
                    // add this file to the list
                    filesToRemove.Add(filename);
                    zip2.RemoveEntry(filename);
                }

                zip2.Comment = "This archive has been modified. Some files have been removed.";
                zip2.Save();
            }


            // extract all files, verify none should have been removed,
            // and verify the contents of those that remain
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip3.EntryFileNames)
                {
                    Assert.IsFalse(filesToRemove.Contains(s1), String.Format("File ({0}) was not expected.", s1));

                    zip3[s1].ExtractWithPassword("extract", password);
                    repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                                     s1);

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s1));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));

                }
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesToBeAdded - filesToRemove.Count,
                "The updated Zip file has the wrong number of entries.");
        }



        [TestMethod]
        public void UpdateZip_RenameEntry()
        {
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var files = TestUtilities.GenerateFilesFlat(dirToZip,
                                                        _rnd.Next(13) + 24,
                                                        42 * 1024 + _rnd.Next(20000));

            // Two passes:  in pass 1, simply rename the file;
            // in pass 2, rename it so that it has a directory.
            // This shouldn't matter, but we test it anyway.
            for (int k = 0; k < 2; k++)
            {
                string zipFileToCreate = String.Format("UpdateZip_RenameEntry-{0}.zip", k);
                TestContext.WriteLine("-----------------------------");
                TestContext.WriteLine("{0}: Trial {1}, adding {2} files into '{3}'...",
                                      DateTime.Now.ToString("HH:mm:ss"),
                                      k,
                                      files.Length,
                                      zipFileToCreate);

                // Add the files to the zip, save the zip
                Directory.SetCurrentDirectory(TopLevelDir);
                using (ZipFile zip1 = new ZipFile())
                {
                    foreach (String f in files)
                        zip1.AddFile(f, "");
                    zip1.Comment = "This archive will be updated.";
                    zip1.Save(zipFileToCreate);
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                     files.Length,
                                     "the Zip file has the wrong number of entries.");

                // selectively rename a few files in the zip archive
                int renameCount = 0;
                using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                {
                    var toRename = new List<ZipEntry>();
                    while (toRename.Count < 2)
                    {
                        foreach (ZipEntry e in zip2)
                        {
                            if (_rnd.Next(2) == 1)
                                toRename.Add(e);
                        }
                    }

                    foreach (ZipEntry e in toRename)
                    {
                        var newname = (k == 0)
                            ? e.FileName + "-renamed"
                            : "renamed_files\\" + e.FileName;

                        TestContext.WriteLine("  renaming {0} to {1}", e.FileName, newname);
                        e.FileName = newname;
                        e.Comment = "renamed";
                        renameCount++;
                    }

                    zip2.Comment = String.Format("This archive has been modified. {0} files have been renamed.", renameCount);
                    zip2.Save();
                }


                // Extract all the files, verify that none have been removed,
                // and verify the names of the entries.
                int renameCount2 = 0;
                using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
                {
                    foreach (string s1 in zip3.EntryFileNames)
                    {
                        string dir = String.Format("extract{0}", k);
                        zip3[s1].Extract(dir);
                        string origFilename = Path.GetFileName((s1.Contains("renamed"))
                            ? s1.Replace("-renamed", "")
                            : s1);

                        if (zip3[s1].Comment == "renamed") renameCount2++;
                    }
                }

                Assert.AreEqual<int>(renameCount, renameCount2,
                    "The updated Zip file has the wrong number of renamed entries.");

                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                     files.Length,
                                     "Wrong number of entries.");
            }
        }


        [TestMethod]
        public void UpdateZip_UpdateEntryComment()
        {
            for (int k = 0; k < 2; k++)
            {
                int j;
                int entriesToBeAdded = 0;
                string filename = null;
                string repeatedLine = null;

                // select the name of the zip file
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("UpdateZip_UpdateEntryComment-{0}.zip", k));

                // create the subdirectory
                string subdir = Path.Combine(TopLevelDir, String.Format("A{0}", k));
                Directory.CreateDirectory(subdir);

                // create a bunch of files
                //int numFilesToCreate = _rnd.Next(15) + 18;
                int numFilesToCreate = _rnd.Next(5) + 3;

                TestContext.WriteLine("\n-----------------------------\r\n{0}: Trial {1}, adding {2} files into '{3}'...",
                    DateTime.Now.ToString("HH:mm:ss"),
                    k,
                    numFilesToCreate,
                    zipFileToCreate);

                for (j = 0; j < numFilesToCreate; j++)
                {
                    filename = String.Format("file{0:D3}.txt", j);
                    repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                                     filename);

                    int filesize = _rnd.Next(34000) + 800;

                    TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename),
                                                        repeatedLine,
                                                        filesize);
                    entriesToBeAdded++;
                }

                // Add the files to the zip, save the zip
                Directory.SetCurrentDirectory(TopLevelDir);
                using (ZipFile zip1 = new ZipFile())
                {
                    String[] filenames = Directory.GetFiles(String.Format("A{0}", k));
                    foreach (String f in filenames)
                        zip1.AddFile(f, "");

                    zip1.Comment = "UpdateTests::UpdateZip_UpdateEntryComment(): This archive will be updated.";
                    zip1.Save(zipFileToCreate);
                }

                // Verify the files are in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesToBeAdded,
                    "the Zip file has the wrong number of entries.");

                // update the comments for a few files in the zip archive
                int updateCount = 0;
                using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                {
                    do
                    {
                        foreach (ZipEntry e in zip2)
                        {
                            if (_rnd.Next(2) == 1)
                            {
                                if (String.IsNullOrEmpty(e.Comment))
                                {
                                    e.Comment = "This is a new comment on entry " + e.FileName;
                                    updateCount++;
                                }
                            }
                        }
                    } while (updateCount < 2);
                    zip2.Comment = String.Format("This archive has been modified.  Comments on {0} entries have been inserted.", updateCount);
                    zip2.Save();
                }


                // Extract all files, verify that none have been removed,
                // and verify the contents of those that remain.
                int commentCount = 0;
                using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
                {
                    foreach (string s1 in zip3.EntryFileNames)
                    {
                        string dir = String.Format("extract{0}", k);
                        zip3[s1].Extract(dir);
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                                         s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine(dir, s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                    String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));

                        if (!String.IsNullOrEmpty(zip3[s1].Comment))
                        {
                            commentCount++;
                        }
                    }
                }

                Assert.AreEqual<int>(updateCount, commentCount,
                    "The updated Zip file has the wrong number of entries with comments.");

                // Verify the files are in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesToBeAdded,
                    "The updated Zip file has the wrong number of entries.");
            }
        }




        [TestMethod]
        public void UpdateZip_RemoveEntry_ByFilename()
        {
            for (int k = 0; k < 2; k++)
            {
                int j;
                int entriesToBeAdded = 0;
                string filename = null;
                string repeatedLine = null;

                // select the name of the zip file
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("UpdateZip_RemoveEntry_ByFilename-{0}.zip", k));

                // create the subdirectory
                string subdir = Path.Combine(TopLevelDir, String.Format("A{0}", k));
                Directory.CreateDirectory(subdir);

                // create a bunch of files
                int numFilesToCreate = _rnd.Next(13) + 24;

                for (j = 0; j < numFilesToCreate; j++)
                {
                    filename = String.Format("file{0:D3}.txt", j);
                    repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                                     filename);
                    TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                    entriesToBeAdded++;
                }

                // Add the files to the zip, save the zip.
                // in pass 2, remove one file, then save again.
                Directory.SetCurrentDirectory(TopLevelDir);
                using (ZipFile zip1 = new ZipFile())
                {
                    String[] filenames = Directory.GetFiles(String.Format("A{0}", k));

                    foreach (String f in filenames)
                        zip1.AddFile(f, "");

                    zip1.Comment = "UpdateTests::UpdateZip_RemoveEntry_ByFilename(): This archive will be updated.";
                    zip1.Save(zipFileToCreate);

                    // conditionally remove a single entry, on the 2nd trial
                    if (k == 1)
                    {
                        int chosen = _rnd.Next(filenames.Length);
                        zip1.RemoveEntry(zip1[chosen]);
                        zip1.Save();
                    }
                }

                // Verify the files are in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesToBeAdded - k,
                    "Trial {0}: the Zip file has the wrong number of entries.", k);

                if (k == 0)
                {
                    // selectively remove a few files in the zip archive
                    var filesToRemove = new List<string>();
                    int numToRemove = _rnd.Next(numFilesToCreate - 4) + 1;
                    using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                    {
                        for (j = 0; j < numToRemove; j++)
                        {
                            // select a new, uniquely named file to create
                            do
                            {
                                filename = String.Format("file{0:D3}.txt", _rnd.Next(numFilesToCreate));
                            } while (filesToRemove.Contains(filename));
                            // add this file to the list
                            filesToRemove.Add(filename);
                            zip2.RemoveEntry(filename);

                        }

                        zip2.Comment = "This archive has been modified. Some files have been removed.";
                        zip2.Save();
                    }


                    // extract all files, verify none should have been removed,
                    // and verify the contents of those that remain
                    using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
                    {
                        foreach (string s1 in zip3.EntryFileNames)
                        {
                            Assert.IsFalse(filesToRemove.Contains(s1),
                                           String.Format("File ({0}) was not expected.", s1));

                            zip3[s1].Extract("extract");
                            repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                                             s1);

                            // verify the content of the updated file.
                            var sr = new StreamReader(Path.Combine("extract", s1));
                            string sLine = sr.ReadLine();
                            sr.Close();

                            Assert.AreEqual<string>(repeatedLine, sLine,
                                        String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));

                        }
                    }

                    // Verify the files are in the zip
                    Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                         entriesToBeAdded - filesToRemove.Count,
                                         "The updated Zip file has the wrong number of entries.");
                }
            }
        }




        [TestMethod]
        public void UpdateZip_RemoveEntry_ViaIndexer_WithPassword()
        {
            string password = TestUtilities.GenerateRandomPassword();
            string filename = null;
            int entriesToBeAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_RemoveEntry_ViaIndexer_WithPassword.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int numFilesToCreate = _rnd.Next(13) + 14;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = String.Format("file{0:D3}.txt", j);
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    filename);
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                entriesToBeAdded++;
            }

            // Add the files to the zip, save the zip
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                zip.Password = password;
                foreach (String f in filenames)
                    zip.AddFile(f, "");

                zip.Comment = "UpdateTests::UpdateZip_OpenForUpdate_Password_RemoveViaIndexer(): This archive will be updated.";
                zip.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(entriesToBeAdded, TestUtilities.CountEntries(zipFileToCreate),
                "The Zip file has the wrong number of entries.");

            // selectively remove a few files in the zip archive
            var filesToRemove = new List<string>();
            int numToRemove = _rnd.Next(numFilesToCreate - 4);
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                for (j = 0; j < numToRemove; j++)
                {
                    // select a new, uniquely named file to create
                    do
                    {
                        filename = String.Format("file{0:D3}.txt", _rnd.Next(numFilesToCreate));
                    } while (filesToRemove.Contains(filename));
                    // add this file to the list
                    filesToRemove.Add(filename);

                    // remove the file from the zip archive
                    zip2.RemoveEntry(filename);
                }

                zip2.Comment = "This archive has been modified. Some files have been removed.";
                zip2.Save();
            }

            // extract all files, verify none should have been removed,
            // and verify the contents of those that remain
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip3.EntryFileNames)
                {
                    Assert.IsFalse(filesToRemove.Contains(s1), String.Format("File ({0}) was not expected.", s1));

                    zip3[s1].ExtractWithPassword("extract", password);
                    repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                                     s1);

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s1));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine,
                                            sLine,
                                            String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));

                }
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 entriesToBeAdded - filesToRemove.Count,
                                 "The updated Zip file has the wrong number of entries.");
        }



        [TestMethod]
        public void UpdateZip_RemoveAllEntries()
        {
            string password = "Wheeee!!" + TestUtilities.GenerateRandomLowerString(7);
            string filename = null;
            int entriesToBeAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_RemoveAllEntries.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int numFilesToCreate = _rnd.Next(13) + 14;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = String.Format("file{0:D3}.txt", j);
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    filename);
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                entriesToBeAdded++;
            }

            // Add the files to the zip, save the zip
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                zip.Password = password;
                foreach (String f in filenames)
                    zip.AddFile(f, "");

                zip.Comment = "UpdateTests::UpdateZip_RemoveAllEntries(): This archive will be updated.";
                zip.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(entriesToBeAdded,
                                 TestUtilities.CountEntries(zipFileToCreate),
                                 "The Zip file has the wrong number of entries.");

            // remove all the entries from the zip archive
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                zip2.RemoveSelectedEntries("*.*");
                zip2.Comment = "This archive has been modified. All the entries have been removed.";
                zip2.Save();
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(0, TestUtilities.CountEntries(zipFileToCreate),
                "The Zip file has the wrong number of entries.");


        }


        [TestMethod]
        public void UpdateZip_AddFile_OldEntriesWithPassword()
        {
            string password = "Secret!";
            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_AddFile_OldEntriesWithPassword.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int numFilesToCreate = _rnd.Next(10) + 8;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Create the zip file
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Password = password;
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_AddFile_OldEntriesWithPassword(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");


            // Create a bunch of new files...
            var addedFiles = new List<string>();
            int numToUpdate = _rnd.Next(numFilesToCreate - 4);
            for (j = 0; j < numToUpdate; j++)
            {
                // select a new, uniquely named file to create
                filename = String.Format("newfile{0:D3}.txt", j);
                // create a new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                addedFiles.Add(filename);
            }

            // add each one of those new files in the zip archive
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in addedFiles)
                    zip2.AddFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_AddFile_OldEntriesWithPassword(): This archive has been updated.";
                zip2.Save();
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                 entriesAdded + addedFiles.Count,
                                 "The Zip file has the wrong number of entries.");


            // now extract the newly-added files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in addedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].Extract("extract");

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }


            // extract all the other files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip4.EntryFileNames)
                {
                    bool addedLater = false;
                    foreach (string s2 in addedFiles)
                    {
                        if (s2 == s1) addedLater = true;
                    }
                    if (!addedLater)
                    {
                        zip4[s1].ExtractWithPassword("extract", password);
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }



        [TestMethod]
        public void UpdateZip_UpdateItem()
        {
            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_UpdateItem.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create a bunch of files
            int numFilesToCreate = _rnd.Next(10) + 8;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("Content for Original file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Create the zip file
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_UpdateItem(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // create another subdirectory
            subdir = Path.Combine(TopLevelDir, "B");
            Directory.CreateDirectory(subdir);

            // create a bunch more files
            int newFileCount = numFilesToCreate + _rnd.Next(3) + 3;
            for (j = 0; j < newFileCount; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("Content for the updated file {0} {1}",
                    Path.GetFileName(filename),
                    System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(1000) + 2000);
                entriesAdded++;
            }

            // Update those files in the zip file
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("B");
                foreach (String f in filenames)
                    zip1.UpdateItem(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_UpdateItem(): This archive has been updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), newFileCount,
                "The Zip file has the wrong number of entries.");

            // now extract the files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in zip3.EntryFileNames)
                {
                    repeatedLine = String.Format("Content for the updated file {0} {1}",
                        s,
                        System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].Extract("extract");

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }
        }


        [TestMethod]
        public void UpdateZip_AddFile_NewEntriesWithPassword()
        {
            string password = "V.Secret!";
            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_AddFile_NewEntriesWithPassword.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create a bunch of files
            int numFilesToCreate = _rnd.Next(10) + 8;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                zip.AddFiles(filenames, "");
                zip.Comment = "UpdateTests::UpdateZip_AddFile_NewEntriesWithPassword(): This archive will be updated.";
                zip.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // Create a bunch of new files...
            var addedFiles = new List<string>();
            int numToUpdate = _rnd.Next(numFilesToCreate - 4);
            for (j = 0; j < numToUpdate; j++)
            {
                // select a new, uniquely named file to create
                filename = String.Format("newfile{0:D3}.txt", j);
                // create a new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                addedFiles.Add(filename);
            }

            // add each one of those new files in the zip archive using a password
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                zip2.Password = password;
                foreach (string s in addedFiles)
                    zip2.AddFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_AddFile_OldEntriesWithPassword(): This archive has been updated.";
                zip2.Save();
            }


            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded + addedFiles.Count,
                "The Zip file has the wrong number of entries.");


            // now extract the newly-added files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in addedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].ExtractWithPassword("extract", password);

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }


            // extract all the other files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip4.EntryFileNames)
                {
                    bool addedLater = false;
                    foreach (string s2 in addedFiles)
                    {
                        if (s2 == s1) addedLater = true;
                    }
                    if (!addedLater)
                    {
                        zip4[s1].Extract("extract");
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }


        [TestMethod]
        public void UpdateZip_AddFile_DifferentPasswords()
        {
            string password1 = Path.GetRandomFileName();
            string password2 = "Secret2" + Path.GetRandomFileName();
            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_AddFile_DifferentPasswords.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create a bunch of files
            int numFilesToCreate = _rnd.Next(11) + 8;
            for (j = 0; j < numFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Add the files to the zip, save the zip
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Password = password1;
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_AddFile_DifferentPasswords(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // Create a bunch of new files...
            var addedFiles = new List<string>();
            //int numToUpdate = _rnd.Next(numFilesToCreate - 4);
            int numToUpdate = 1;
            for (j = 0; j < numToUpdate; j++)
            {
                // select a new, uniquely named file to create
                filename = String.Format("newfile{0:D3}.txt", j);
                // create a new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                addedFiles.Add(filename);
            }

            // add each one of those new files in the zip archive using a password
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                zip2.Password = password2;
                foreach (string s in addedFiles)
                    zip2.AddFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_AddFile_OldEntriesWithPassword(): This archive has been updated.";
                zip2.Save();
            }


            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded + addedFiles.Count,
                "The Zip file has the wrong number of entries.");


            // now extract the newly-added files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in addedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].ExtractWithPassword("extract", password2);

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }


            // extract all the other files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip4.EntryFileNames)
                {
                    bool addedLater = false;
                    foreach (string s2 in addedFiles)
                    {
                        if (s2 == s1) addedLater = true;
                    }
                    if (!addedLater)
                    {
                        zip4[s1].ExtractWithPassword("extract", password1);
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }




        [TestMethod]
        public void UpdateZip_UpdateFile_NoPasswords()
        {
            string filename = null;
            int entriesAdded = 0;
            int j = 0;
            string repeatedLine = null;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_UpdateFile_NoPasswords.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int NumFilesToCreate = _rnd.Next(23) + 14;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_UpdateFile_NoPasswords(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "Zoiks! The Zip file has the wrong number of entries.");



            // create another subdirectory
            subdir = Path.Combine(TopLevelDir, "updates");
            Directory.CreateDirectory(subdir);

            // Create a bunch of new files, in that new subdirectory
            var UpdatedFiles = new List<string>();
            int NumToUpdate = _rnd.Next(NumFilesToCreate - 4);
            for (j = 0; j < NumToUpdate; j++)
            {
                // select a new, uniquely named file to create
                do
                {
                    filename = String.Format("file{0:D3}.txt", _rnd.Next(NumFilesToCreate));
                } while (UpdatedFiles.Contains(filename));
                // create a new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                UpdatedFiles.Add(filename);
            }

            // update those files in the zip archive
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                    zip2.UpdateFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_UpdateFile_OldEntriesWithPassword(): This archive has been updated.";
                zip2.Save();
            }

            // extract those files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].Extract("extract");

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }

            // extract all the other files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip4.EntryFileNames)
                {
                    bool NotUpdated = true;
                    foreach (string s2 in UpdatedFiles)
                    {
                        if (s2 == s1) NotUpdated = false;
                    }
                    if (NotUpdated)
                    {
                        zip4[s1].Extract("extract");
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }


        [TestMethod]
        public void UpdateZip_UpdateFile_2_NoPasswords()
        {
            string filename = null;
            int entriesAdded = 0;
            int j = 0;
            string repeatedLine = null;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_UpdateFile_NoPasswords.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int NumFilesToCreate = _rnd.Next(23) + 14;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.UpdateFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_UpdateFile_NoPasswords(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "Zoiks! The Zip file has the wrong number of entries.");


            // create another subdirectory
            subdir = Path.Combine(TopLevelDir, "updates");
            Directory.CreateDirectory(subdir);

            // Create a bunch of new files, in that new subdirectory
            var UpdatedFiles = new List<string>();
            int NumToUpdate = _rnd.Next(NumFilesToCreate - 4);
            for (j = 0; j < NumToUpdate; j++)
            {
                // select a new, uniquely named file to create
                do
                {
                    filename = String.Format("file{0:D3}.txt", _rnd.Next(NumFilesToCreate));
                } while (UpdatedFiles.Contains(filename));
                // create a new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                UpdatedFiles.Add(filename);
            }

            // update those files in the zip archive
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                    zip2.UpdateFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_UpdateFile_NoPasswords(): This archive has been updated.";
                zip2.Save();
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "Zoiks! The Zip file has the wrong number of entries.");

            // update those files AGAIN in the zip archive
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                    zip3.UpdateFile(Path.Combine(subdir, s), "");
                zip3.Comment = "UpdateTests::UpdateZip_UpdateFile_NoPasswords(): This archive has been re-updated.";
                zip3.Save();
            }

            // extract the updated files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip4[s].Extract("extract");

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }

            // extract all the other files and verify their contents
            using (ZipFile zip5 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip5.EntryFileNames)
                {
                    bool NotUpdated = true;
                    foreach (string s2 in UpdatedFiles)
                    {
                        if (s2 == s1) NotUpdated = false;
                    }
                    if (NotUpdated)
                    {
                        zip5[s1].Extract("extract");
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }



        [TestMethod]
        public void UpdateZip_UpdateFile_OldEntriesWithPassword()
        {
            string Password = "1234567";
            string filename = null;
            int entriesAdded = 0;
            int j = 0;
            string repeatedLine = null;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_UpdateFile_OldEntriesWithPassword.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create a bunch of files
            int NumFilesToCreate = _rnd.Next(23) + 14;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                    Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Password = Password;
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_UpdateFile_OldEntriesWithPassword(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the number of files in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // create another subdirectory
            subdir = Path.Combine(TopLevelDir, "updates");
            Directory.CreateDirectory(subdir);

            // Create a bunch of new files, in that new subdirectory
            var UpdatedFiles = new List<string>();
            int NumToUpdate = _rnd.Next(NumFilesToCreate - 4);
            for (j = 0; j < NumToUpdate; j++)
            {
                // select a new, uniquely named file to create
                do
                {
                    filename = String.Format("file{0:D3}.txt", _rnd.Next(NumFilesToCreate));
                } while (UpdatedFiles.Contains(filename));
                // create a new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                UpdatedFiles.Add(filename);
            }

            // update those files in the zip archive
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                    zip2.UpdateFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_UpdateFile_OldEntriesWithPassword(): This archive has been updated.";
                zip2.Save();
            }

            // extract those files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].Extract("extract");

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }

            // extract all the other files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip4.EntryFileNames)
                {
                    bool NotUpdated = true;
                    foreach (string s2 in UpdatedFiles)
                    {
                        if (s2 == s1) NotUpdated = false;
                    }
                    if (NotUpdated)
                    {
                        zip4[s1].ExtractWithPassword("extract", Password);
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }


        [TestMethod]
        public void UpdateZip_UpdateFile_NewEntriesWithPassword()
        {
            string Password = " P@ssw$rd";
            string filename = null;
            int entriesAdded = 0;
            string repeatedLine = null;
            int j = 0;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_UpdateFile_NewEntriesWithPassword.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create a bunch of files
            int NumFilesToCreate = _rnd.Next(23) + 9;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                         Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // create the zip archive, add those files to it
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                // no password used here.
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_UpdateFile_NewEntriesWithPassword(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // create another subdirectory
            subdir = Path.Combine(TopLevelDir, "updates");
            Directory.CreateDirectory(subdir);

            // Create a bunch of new files, in that new subdirectory
            var UpdatedFiles = new List<string>();
            int NumToUpdate = _rnd.Next(NumFilesToCreate - 5);
            for (j = 0; j < NumToUpdate; j++)
            {
                // select a new, uniquely named file to create
                do
                {
                    filename = String.Format("file{0:D3}.txt", _rnd.Next(NumFilesToCreate));
                } while (UpdatedFiles.Contains(filename));
                // create the new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                UpdatedFiles.Add(filename);
            }

            // update those files in the zip archive
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                zip2.Password = Password;
                foreach (string s in UpdatedFiles)
                    zip2.UpdateFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_UpdateFile_NewEntriesWithPassword(): This archive has been updated.";
                zip2.Save();
            }

            // extract those files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].ExtractWithPassword("extract", Password);

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(repeatedLine, sLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }

            // extract all the other files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip4.EntryFileNames)
                {
                    bool NotUpdated = true;
                    foreach (string s2 in UpdatedFiles)
                    {
                        if (s2 == s1) NotUpdated = false;
                    }
                    if (NotUpdated)
                    {
                        zip4[s1].Extract("extract");
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }


        [TestMethod]
        public void UpdateZip_UpdateFile_DifferentPasswords()
        {
            string Password1 = "Whoofy1";
            string Password2 = "Furbakl1";
            string filename = null;
            int entriesAdded = 0;
            int j = 0;
            string repeatedLine;

            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_UpdateFile_DifferentPasswords.zip");

            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create a bunch of files
            int NumFilesToCreate = _rnd.Next(13) + 14;
            for (j = 0; j < NumFilesToCreate; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                      Path.GetFileName(filename));
                TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // create the zip archive
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.Password = Password1;
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip1.AddFile(f, "");
                zip1.Comment = "UpdateTests::UpdateZip_UpdateFile_DifferentPasswords(): This archive will be updated.";
                zip1.Save(zipFileToCreate);
            }

            // Verify the files are in the zip
            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entriesAdded,
                "The Zip file has the wrong number of entries.");

            // create another subdirectory
            subdir = Path.Combine(TopLevelDir, "updates");
            Directory.CreateDirectory(subdir);

            // Create a bunch of new files, in that new subdirectory
            var UpdatedFiles = new List<string>();
            int NumToUpdate = _rnd.Next(NumFilesToCreate - 4);
            for (j = 0; j < NumToUpdate; j++)
            {
                // select a new, uniquely named file to create
                do
                {
                    filename = String.Format("file{0:D3}.txt", _rnd.Next(NumFilesToCreate));
                } while (UpdatedFiles.Contains(filename));
                // create a new file, and fill that new file with text data
                repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                    filename, System.DateTime.Now.ToString("yyyy-MM-dd"));
                TestUtilities.CreateAndFillFileText(Path.Combine(subdir, filename), repeatedLine, _rnd.Next(34000) + 5000);
                UpdatedFiles.Add(filename);
            }

            // update those files in the zip archive
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                zip2.Password = Password2;
                foreach (string s in UpdatedFiles)
                    zip2.UpdateFile(Path.Combine(subdir, s), "");
                zip2.Comment = "UpdateTests::UpdateZip_UpdateFile_DifferentPasswords(): This archive has been updated.";
                zip2.Save();
            }

            // extract those files and verify their contents
            using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s in UpdatedFiles)
                {
                    repeatedLine = String.Format("**UPDATED** This file ({0}) has been updated on {1}.",
                        s, System.DateTime.Now.ToString("yyyy-MM-dd"));
                    zip3[s].ExtractWithPassword("extract", Password2);

                    // verify the content of the updated file.
                    var sr = new StreamReader(Path.Combine("extract", s));
                    string sLine = sr.ReadLine();
                    sr.Close();

                    Assert.AreEqual<string>(sLine, repeatedLine,
                            String.Format("The content of the Updated file ({0}) in the zip archive is incorrect.", s));
                }
            }

            // extract all the other files and verify their contents
            using (ZipFile zip4 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string s1 in zip4.EntryFileNames)
                {
                    bool wasUpdated = false;
                    foreach (string s2 in UpdatedFiles)
                    {
                        if (s2 == s1) wasUpdated = true;
                    }
                    if (!wasUpdated)
                    {
                        // use original password
                        zip4[s1].ExtractWithPassword("extract", Password1);
                        repeatedLine = String.Format("This line is repeated over and over and over in file {0}",
                            s1);

                        // verify the content of the updated file.
                        var sr = new StreamReader(Path.Combine("extract", s1));
                        string sLine = sr.ReadLine();
                        sr.Close();

                        Assert.AreEqual<string>(repeatedLine, sLine,
                                String.Format("The content of the originally added file ({0}) in the zip archive is incorrect.", s1));
                    }
                }
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void UpdateZip_AddFile_ExistingFile_Error()
        {
            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "UpdateZip_AddFile_ExistingFile_Error.zip");
            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subdir);

            // create the files
            int fileCount = _rnd.Next(3) + 4;
            string filename = null;
            int entriesAdded = 0;
            for (int j = 0; j < fileCount; j++)
            {
                filename = Path.Combine(subdir, String.Format("file{0:D3}.txt", j));
                TestUtilities.CreateAndFillFileText(filename, _rnd.Next(34000) + 5000);
                entriesAdded++;
            }

            // Add the files to the zip, save the zip
            Directory.SetCurrentDirectory(TopLevelDir);
            using (ZipFile zip = new ZipFile())
            {
                String[] filenames = Directory.GetFiles("A");
                foreach (String f in filenames)
                    zip.AddFile(f, "");
                zip.Comment = "UpdateTests::UpdateZip_AddFile_ExistingFile_Error(): This archive will be updated.";
                zip.Save(zipFileToCreate);
            }

            // create and file a new file with text data
            int FileToUpdate = _rnd.Next(fileCount);
            filename = String.Format("file{0:D3}.txt", FileToUpdate);
            string repeatedLine = String.Format("**UPDATED** This file ({0}) was updated at {1}.",
                        filename,
                        System.DateTime.Now.ToString("G"));
            TestUtilities.CreateAndFillFileText(filename, repeatedLine, _rnd.Next(21567) + 23872);

            // Try to again add that file in the zip archive. This
            // should fail.
            using (ZipFile z = ZipFile.Read(zipFileToCreate))
            {
                // Try Adding a file again.  THIS SHOULD THROW.
                ZipEntry e = z.AddFile(filename, "");
                z.Comment = "UpdateTests::UpdateZip_AddFile_ExistingFile_Error(): This archive has been updated.";
                z.Save();
            }
        }


        [TestMethod]
        public void Update_MultipleSaves_wi10319()
        {
            string zipFileToCreate = "MultipleSaves_wi10319.zip";

            using (ZipFile _zipFile = new ZipFile(zipFileToCreate))
            {
                using (MemoryStream data = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(data))
                    {
                        writer.Write("Dit is een test string.");
                        writer.Flush();

                        data.Seek(0, SeekOrigin.Begin);
                        _zipFile.AddEntry("test.txt", data);
                        _zipFile.Save();
                        _zipFile.AddEntry("test2.txt", "Esta es un string de test");
                        _zipFile.Save();
                        _zipFile.AddEntry("test3.txt", "this is some content for the entry.");
                        _zipFile.Save();
                    }
                }
            }

            using (ZipFile _zipFile = new ZipFile(zipFileToCreate))
            {
                using (MemoryStream data = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(data))
                    {
                        writer.Write("Dit is een andere test string.");
                        writer.Flush();

                        data.Seek(0, SeekOrigin.Begin);

                        _zipFile.UpdateEntry("test.txt", data);
                        _zipFile.Save();
                        _zipFile.UpdateEntry("test2.txt", "Esta es un otro string de test");
                        _zipFile.Save();
                        _zipFile.UpdateEntry("test3.txt", "This is another string for content.");
                        _zipFile.Save();

                    }
                }
            }
        }




        [TestMethod]
        public void Update_MultipleSaves_wi10694()
        {
            string zipFileToCreate = "Update_MultipleSaves_wi10694.zip";
            var shortDir = "fodder";
            string subdir = Path.Combine(TopLevelDir, shortDir);
            string[] filesToZip = TestUtilities.GenerateFilesFlat(subdir);

            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddFiles(filesToZip, "Download");
                zip1.AddFiles(filesToZip, "other");
                zip1.Save(zipFileToCreate);
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), 2 * filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");

            using (var zip2 = ZipFile.Read(zipFileToCreate))
            {
                var entries = zip2.Entries.Where(e => e.FileName.Contains("Download")).ToArray();
                //PART1 - Add directory and save
                zip2.AddDirectoryByName("XX");
                zip2.Save();

                //PART2 - Rename paths (not related to XX directory from above) and save
                foreach (var zipEntry in entries)
                {
                    zipEntry.FileName = zipEntry.FileName.Replace("Download", "Download2");
                }
                zip2.Save();
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), 2 * filesToZip.Length,
                                 "Incorrect number of entries in the zip file.");
        }



        [TestMethod]
        public void Update_MultipleSavesWithRename_wi10544()
        {
            // select the name of the zip file
            string zipFileToCreate = Path.Combine(TopLevelDir, "Update_MultipleSaves_wi10319.zip");
            string entryName = "Entry1.txt";

            TestContext.WriteLine("Creating zip file... ");
            using (var zip = new ZipFile())
            {
                string firstline = "This is the first line in the Entry.\n";
                byte[] a = System.Text.Encoding.ASCII.GetBytes(firstline.ToCharArray());

                zip.AddEntry(entryName, a);
                zip.Save(zipFileToCreate);
            }

            int N = _rnd.Next(34) + 59;
            for (int i = 0; i < N; i++)
            {
                string tempZipFile = "AppendToEntry.zip.tmp" + i;

                TestContext.WriteLine("Update cycle {0}... ", i);
                using (var zip1 = ZipFile.Read(zipFileToCreate))
                {
                    using (var zip = new ZipFile())
                    {
                        zip.AddEntry(entryName, (name, stream) =>
                            {
                                var src = zip1[name].OpenReader();
                                int n;
                                byte[] b = new byte[2048];
                                while ((n = src.Read(b, 0, b.Length)) > 0)
                                    stream.Write(b, 0, n);

                                string update = String.Format("Updating zip file {0} at {1}\n", i, DateTime.Now.ToString("G"));
                                byte[] a = System.Text.Encoding.ASCII.GetBytes(update.ToCharArray());
                                stream.Write(a, 0, a.Length);
                            });

                        zip.Save(tempZipFile);
                    }
                }

                File.Delete(zipFileToCreate);
                System.Threading.Thread.Sleep(1400);
                File.Move(tempZipFile, zipFileToCreate);
            }

        }


        [TestMethod]
        public void Update_FromRoot_wi11988()
        {
            string zipFileToCreate = "FromRoot.zip";
            string dirToZip = "Fodder";
            var files = TestUtilities.GenerateFilesFlat(dirToZip);
            string windir = System.Environment.GetEnvironmentVariable("Windir");
            string substExe = Path.Combine(Path.Combine(windir, "system32"), "subst.exe");
            Assert.IsTrue(File.Exists(substExe), "subst.exe does not exist ({0})",
                          substExe);

            try
            {
                // create a subst drive
                this.Exec(substExe, "G: " + dirToZip);

                using (var zip = new ZipFile())
                {
                    zip.UpdateSelectedFiles("*.*", "G:\\", "", true);
                    zip.Save(zipFileToCreate);
                }

                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate),
                                     files.Length);
                Assert.IsTrue(files.Length > 3);
                BasicVerifyZip(zipFileToCreate);
            }
            finally
            {
                // remove the virt drive
                this.Exec(substExe, "/D G:");
            }
        }

    }
}

