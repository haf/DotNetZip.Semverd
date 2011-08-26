// PasswordTests.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008-2011 Dino Chiesa .
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
// Last Saved: <2011-August-06 11:23:57>
//
// ------------------------------------------------------------------
//
// This module provides tests for password features.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;

namespace Ionic.Zip.Tests.Password
{
    [TestClass]
    public class PasswordTests : IonicTestClass
    {
        public PasswordTests() : base() { }

        [TestMethod]
        public void Password_BasicAddAndExtract()
        {
            int i;
            string[] Passwords = { null, "Password!", TestUtilities.GenerateRandomPassword(), "A" };

            Ionic.Zlib.CompressionLevel[] compressionLevelOptions = {
                Ionic.Zlib.CompressionLevel.None,
                Ionic.Zlib.CompressionLevel.BestSpeed,
                Ionic.Zlib.CompressionLevel.Default,
                Ionic.Zlib.CompressionLevel.BestCompression,
            };

            for (int k = 0; k < compressionLevelOptions.Length; k++)
            {
                for (int j = 0; j < Passwords.Length; j++)
                {
                    TestContext.WriteLine("\n\n===================\nTrial ({0}) pw({1})", j, Passwords[j]);
                    string ZipFileToCreate = Path.Combine(TopLevelDir, String.Format("Password_BasicAddAndExtract-{0}-{1}.zip", k, j));
                    Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

                    Directory.SetCurrentDirectory(TopLevelDir);
                    string DirToZip = String.Format("zipthis-{0}-{1}", k, j);
                    Directory.CreateDirectory(DirToZip);

                    TestContext.WriteLine("\n---------------------creating files and computing checksums...");
                    int NumFilesToCreate = _rnd.Next(10) + 10;
                    string[] filenames = new string[NumFilesToCreate];
                    var checksums = new Dictionary<string, string>();
                    for (i = 0; i < NumFilesToCreate; i++)
                    {
                        filenames[i] = Path.Combine(DirToZip, String.Format("file{0:D3}.txt", i));
                        int sz = _rnd.Next(22000) + 3000;
                        //int sz = 1000;
                        var repeatedLine = String.Format("Line to Repeat... {0} {1} {2} filename: {3}", i, k, j, filenames[i]);
                        TestUtilities.CreateAndFillFileText(filenames[i], repeatedLine, sz);
                        string key = Path.GetFileName(filenames[i]);
                        checksums.Add(key, TestUtilities.GetCheckSumString(filenames[i]));
                        TestContext.WriteLine("  chk[{0}]={1}", key, checksums[key]);
                    }

                    TestContext.WriteLine("\n---------------------adding files to the archive...");

                    var sw = new StringWriter();
                    using (ZipFile zip = new ZipFile(ZipFileToCreate, sw))
                    {
                        zip.CompressionLevel = compressionLevelOptions[k];
                        zip.Password = Passwords[j];
                        zip.AddDirectory(Path.GetFileName(DirToZip));
                        zip.Save();
                    }
                    TestContext.WriteLine(sw.ToString());

                    Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), NumFilesToCreate,
                            "The Zip file has an unexpected number of entries.");

                    TestContext.WriteLine("\n---------------------verifying checksums...");

                    using (ZipFile zip = ZipFile.Read(ZipFileToCreate))
                    {
                        foreach (ZipEntry e in zip)
                            TestContext.WriteLine("found entry: {0}", e.FileName);

                        var extractDir = String.Format("extract-{0}-{1}", k, j);
                        TestContext.WriteLine("  Extract with pw({0})", Passwords[j]);
                        foreach (ZipEntry e in zip)
                        {
                            e.ExtractWithPassword(extractDir, ExtractExistingFileAction.OverwriteSilently, Passwords[j]);
                            if (!e.IsDirectory)
                            {
                                byte[] c2 = TestUtilities.ComputeChecksum(Path.Combine(extractDir, e.FileName));
                                Assert.AreEqual<string>(checksums[e.FileName],
                                        TestUtilities.CheckSumToString(c2), "The checksum of the extracted file is incorrect.");
                            }
                        }
                    }
                    TestContext.WriteLine("\n");
                }
            }
        }



        [TestMethod]
        public void Password_CheckZipPassword_wi13664()
        {
            string[] passwords = { null,
                                   "Password!",
                                   TestUtilities.GenerateRandomPassword(),
                                   "_" };

            string dirToZip = Path.Combine(TopLevelDir, "zipthis");
            int subdirCount;
            int entries = TestUtilities.GenerateFilesOneLevelDeep
                (TestContext, "wi13664", dirToZip, null, out subdirCount);
            string[] filesToZip = Directory.GetFiles("zipthis", "*.*", SearchOption.AllDirectories);

            Assert.AreEqual<int>(filesToZip.Length, entries,
                                 "Incorrect number of entries in the directory.");

            for (int j = 0; j < passwords.Length; j++)
            {
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("Password_CheckZipPassword_wi13664-{0}.zip", j));

                // Create the zip archive
                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.Password = passwords[j];
                    zip1.AddFiles(filesToZip, true, "");
                    zip1.Save(zipFileToCreate);
                }

                var r = ZipFile.CheckZipPassword(zipFileToCreate, passwords[j]);
                Assert.IsTrue(r, "Bad password in round {0}", j);
            }
        }


        [TestMethod]
        public void Password_UnsetEncryptionAfterSetPassword_wi13909_ZOS()
        {
            // Verify that unsetting the Encryption property after
            // setting a Password results in no encryption being used.
            // This method tests ZipOutputStream.
            string unusedPassword = TestUtilities.GenerateRandomPassword();
            int numTotalEntries = _rnd.Next(46)+653;
            string zipFileToCreate = "UnsetEncryption.zip";

            using (FileStream fs = File.Create(zipFileToCreate))
            {
                using (var zos = new ZipOutputStream(fs))
                {
                    zos.Password = unusedPassword;
                    zos.Encryption = EncryptionAlgorithm.None;

                    for (int i=0; i < numTotalEntries; i++)
                    {
                        if (_rnd.Next(7)==0)
                        {
                            string entryName = String.Format("{0:D5}/", i);
                            zos.PutNextEntry(entryName);
                        }
                        else
                        {
                            string entryName = String.Format("{0:D5}.txt", i);
                            zos.PutNextEntry(entryName);
                            if (_rnd.Next(12)==0)
                            {
                                var block = TestUtilities.GenerateRandomAsciiString() + " ";
                                string contentBuffer = String.Format("This is the content for entry {0}", i);
                                int n = _rnd.Next(6) + 2;
                                for (int j=0; j < n; j++)
                                    contentBuffer += block;
                                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(contentBuffer);
                                zos.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }
            }

            BasicVerifyZip(zipFileToCreate);
        }



        [TestMethod]
        public void Password_UnsetEncryptionAfterSetPassword_wi13909_ZF()
        {
            // Verify that unsetting the Encryption property after
            // setting a Password results in no encryption being used.
            // This method tests ZipFile.
            string unusedPassword = TestUtilities.GenerateRandomPassword();
            int numTotalEntries = _rnd.Next(46)+653;
            string zipFileToCreate = "UnsetEncryption.zip";

            using (var zip = new ZipFile())
            {
                zip.Password = unusedPassword;
                zip.Encryption = EncryptionAlgorithm.None;

                for (int i=0; i < numTotalEntries; i++)
                {
                    if (_rnd.Next(7)==0)
                    {
                        string entryName = String.Format("{0:D5}", i);
                        zip.AddDirectoryByName(entryName);
                    }
                    else
                    {
                        string entryName = String.Format("{0:D5}.txt", i);
                        if (_rnd.Next(12)==0)
                        {
                            var block = TestUtilities.GenerateRandomAsciiString() + " ";
                            string contentBuffer = String.Format("This is the content for entry {0}", i);
                                int n = _rnd.Next(6) + 2;
                                for (int j=0; j < n; j++)
                                    contentBuffer += block;
                            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(contentBuffer);
                            zip.AddEntry(entryName, contentBuffer);
                        }
                        else
                            zip.AddEntry(entryName, Stream.Null);
                    }
                }
                zip.Save(zipFileToCreate);
            }

            BasicVerifyZip(zipFileToCreate);
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.BadPasswordException))]
        public void Password_CheckBadPassword_wi13668()
        {
            TestContext.WriteLine("Password_CheckBadPassword_wi13668()");
            // In this case, the password is "correct" but the decrypted
            // header does not match the CRC. Therefore the library
            // should fail this password.  I don't know how the zip was
            // constructed but I suspect a broken library.
            string fileName = _GetNameForZipContentFile("wi13668-bad-pwd-472713.zip");
            string password = "472713";
            TestContext.WriteLine("Reading zip file: '{0}'", fileName);
            using (ZipFile zip = ZipFile.Read(fileName))
            {
                foreach (ZipEntry e in zip)
                {
                    // will throw if wrong password
                    e.ExtractWithPassword(Stream.Null, password);
                }
            }

        }

        private string _GetNameForZipContentFile(string shortFileName)
        {
            string SourceDir = CurrentDir;
            for (int i = 0; i < 3; i++)
                SourceDir = Path.GetDirectoryName(SourceDir);

            TestContext.WriteLine("Current Dir: {0}", CurrentDir);

            return  Path.Combine(SourceDir,
                                 "Zip Tests\\bin\\Debug\\zips\\" + shortFileName);
        }


        [TestMethod]
        public void Password_MultipleEntriesDifferentPasswords()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "Password_MultipleEntriesDifferentPasswords.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string SourceDir = CurrentDir;
            for (int i = 0; i < 3; i++)
                SourceDir = Path.GetDirectoryName(SourceDir);

            Directory.SetCurrentDirectory(TopLevelDir);

            string[] filenames =
            {
                Path.Combine(SourceDir, "Tools\\Zipit\\bin\\Debug\\Zipit.exe"),
                Path.Combine(SourceDir, "Zip\\bin\\Debug\\Ionic.Zip.xml"),
            };

            string[] checksums =
            {
                TestUtilities.GetCheckSumString(filenames[0]),
                TestUtilities.GetCheckSumString(filenames[1]),
            };

            string[] passwords =
            {
                    "12345678",
                    "0987654321",
            };

            int j = 0;
            using (ZipFile zip = new ZipFile(ZipFileToCreate))
            {
                for (j = 0; j < filenames.Length; j++)
                {
                    zip.Password = passwords[j];
                    zip.AddFile(filenames[j], "");
                }
                zip.Save();
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(ZipFileToCreate), filenames.Length,
                    "The zip file created has the wrong number of entries.");

            using (ZipFile zip = new ZipFile(ZipFileToCreate))
            {
                for (j = 0; j < filenames.Length; j++)
                {
                    zip[Path.GetFileName(filenames[j])].ExtractWithPassword("unpack", ExtractExistingFileAction.OverwriteSilently, passwords[j]);
                    string newpath = Path.Combine("unpack", filenames[j]);
                    string chk = TestUtilities.GetCheckSumString(newpath);
                    Assert.AreEqual<string>(checksums[j], chk, "File checksums do not match.");
                }
            }
        }


        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.BadPasswordException))]
        public void Password_Extract_WrongPassword()
        {
            string ZipFileToCreate = Path.Combine(TopLevelDir, "MultipleEntriesDifferentPasswords.zip");
            Assert.IsFalse(File.Exists(ZipFileToCreate), "The temporary zip file '{0}' already exists.", ZipFileToCreate);

            string SourceDir = CurrentDir;
            for (int i = 0; i < 3; i++)
                SourceDir = Path.GetDirectoryName(SourceDir);

            Directory.SetCurrentDirectory(TopLevelDir);

            string[] filenames =
            {
                Path.Combine(SourceDir, "Tools\\Zipit\\bin\\Debug\\Zipit.exe"),
                Path.Combine(SourceDir, "Zip\\bin\\Debug\\Ionic.Zip.xml"),
            };

            string[] passwords =
            {
                    "12345678",
                    "0987654321",
            };

            int j = 0;
            using (ZipFile zip = new ZipFile(ZipFileToCreate))
            {
                for (j = 0; j < filenames.Length; j++)
                {
                    zip.Password = passwords[j];
                    zip.AddFile(filenames[j], "");
                }
                zip.Save();
            }

            // now try to extract
            using (ZipFile zip = new ZipFile(ZipFileToCreate))
            {
                for (j = 0; j < filenames.Length; j++)
                    zip[Path.GetFileName(filenames[j])].ExtractWithPassword("unpack", ExtractExistingFileAction.OverwriteSilently, "WrongPassword");
            }
        }


        [TestMethod]
        public void Password_AddEntryWithPasswordToExistingZip()
        {
            string zipFileToCreate = "AddEntryWithPasswordToExistingZip.zip";
            string dnzDir = CurrentDir;
            for (int i = 0; i < 3; i++)
                dnzDir = Path.GetDirectoryName(dnzDir);

            string[] filenames =
            {
                Path.Combine(dnzDir, "Tools\\Zipit\\bin\\Debug\\Zipit.exe"),
                Path.Combine(dnzDir, "Zip\\bin\\Debug\\Ionic.Zip.xml"),
            };

            string[] checksums =
            {
                TestUtilities.GetCheckSumString(filenames[0]),
                TestUtilities.GetCheckSumString(filenames[1]),
            };

            int j = 0;
            using (ZipFile zip = new ZipFile(zipFileToCreate))
            {
                for (j = 0; j < filenames.Length; j++)
                    zip.AddFile(filenames[j], "");
                zip.Save();
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), 2,
                    "wrong number of entries.");

            string fileX = Path.Combine(dnzDir, "Tools\\Unzip\\bin\\debug\\unzip.exe");
            string checksumX = TestUtilities.GetCheckSumString(fileX);
            string password = TestUtilities.GenerateRandomPassword() + "!";
            using (ZipFile zip = new ZipFile(zipFileToCreate))
            {
                zip.Password = password;
                zip.AddFile(fileX, "");
                zip.Save();
            }

            Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), 3,
                    "wrong number of entries.");

            string unpackDir = "unpack";
            string newpath, chk, baseName;
            using (ZipFile zip = new ZipFile(zipFileToCreate))
            {
                for (j = 0; j < filenames.Length; j++)
                {
                    baseName = Path.GetFileName(filenames[j]);
                    zip[baseName].Extract(unpackDir, ExtractExistingFileAction.OverwriteSilently);
                    newpath = Path.Combine(unpackDir, filenames[j]);
                    chk = TestUtilities.GetCheckSumString(newpath);
                    Assert.AreEqual<string>(checksums[j], chk, "Checksums do not match.");
                }

                baseName = Path.GetFileName(fileX);

                zip[baseName].ExtractWithPassword(unpackDir,
                                                  ExtractExistingFileAction.OverwriteSilently,
                                                  password);

                newpath = Path.Combine(unpackDir, fileX);
                chk = TestUtilities.GetCheckSumString(newpath);
                Assert.AreEqual<string>(checksumX, chk, "Checksums do not match.");
            }
        }




        [TestMethod]
        public void SilentDeletion_wi10639()
        {
            string zipFileToCreate = "SilentDeletion.zip";
            string dirToZip = "dirToZip";
            string extractDir = "extracted";
            string password = TestUtilities.GenerateRandomPassword();
            string wrongPassword = "passworD";
            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            TestContext.WriteLine("Creating the zip.");
            using (var zip = new ZipFile())
            {
                zip.Password = password;
                zip.AddFiles(files, dirToZip);
                zip.Save(zipFileToCreate);
            }

            TestContext.WriteLine("Extract one file with wrong password.");

             // pick a random entry to extract
            int ix = -1;
            string extractedFile = null;
            // perform two passes: first with correct password to extract the
            // file.  2nd with incorrect password to see if the file is
            // deleted.

            Directory.CreateDirectory(extractDir);
            for (int i=0; i < 2; i++)
            {
                try
                {
                    using (var zip = ZipFile.Read(zipFileToCreate))
                    {
                        if (i==0)
                        {
                            do
                            {
                                ix = this._rnd.Next(zip.Entries.Count);
                            }
                            while (zip[ix].IsDirectory);
                            TestContext.WriteLine("Selected entry: {0}", zip[ix].FileName);
                            extractedFile = Path.Combine(extractDir, zip[ix].FileName.Replace("/","\\"));
                            TestContext.WriteLine("name for extracted file: {0}", extractedFile);
                            Assert.IsFalse(File.Exists(extractedFile), "The file exists.");
                        }
                        TestContext.WriteLine("Cycle {0}: ExtractWithPassword()", i);
                        zip[ix].ExtractWithPassword(extractDir,
                                                    ExtractExistingFileAction.OverwriteSilently,
                                                    (i==0)? password : wrongPassword);
                    }
                }
                catch (Ionic.Zip.BadPasswordException bpe1)
                {
                    // only swallow exceptions on the first go-round
                    if (i==0) throw;
                }
                Assert.IsTrue(File.Exists(extractedFile), "Cycle {0}: The extracted file does not exist.", i);
            }
        }



    }

}
