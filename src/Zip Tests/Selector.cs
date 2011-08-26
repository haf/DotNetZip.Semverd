// Selector.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2010 Dino Chiesa.
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
// Time-stamp: <2011-August-06 17:57:24>
//
// ------------------------------------------------------------------
//
// This module defines tests for the File and Entry Selection stuff in
// DotNetZip.
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
    /// Summary description for Selector
    /// </summary>
    [TestClass]
    public class Selector : IonicTestClass
    {
        public Selector() : base() { }

        [ClassInitialize]
        public static void ClassInit(TestContext a)
        {
            CurrentDir = Directory.GetCurrentDirectory();
            twentyDaysAgo = DateTime.Now - new TimeSpan(20,0,0,0);
            todayAtMidnight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            tomorrow = todayAtMidnight + new TimeSpan(1, 0, 0, 0);
            threeDaysAgo = todayAtMidnight - new TimeSpan(3, 0, 0, 0);
            twoDaysAgo = todayAtMidnight - new TimeSpan(2, 0, 0, 0);
            threeYearsAgo = new DateTime(DateTime.Now.Year - 3, DateTime.Now.Month, DateTime.Now.Day);

            oneDay = new TimeSpan(1,0,0,0);
            yesterdayAtMidnight = todayAtMidnight - oneDay;
        }

        // [ClassCleanup()]
        // public static void MyClassCleanup()
        // {
        //     CleanDirectory(fodderDirectory, null);
        // }


        private static void CleanDirectory(string dirToClean, Ionic.CopyData.Transceiver txrx)
        {
            if (dirToClean == null) return;

            if (!Directory.Exists(dirToClean)) return;

            var dirs = Directory.GetDirectories(dirToClean, "*.*", SearchOption.AllDirectories);

            if (txrx!=null)
                txrx.Send("pb 1 max " + dirs.Length.ToString());

            foreach (var d in dirs)
            {
                CleanDirectory(d, txrx);
                if (txrx!=null)
                    txrx.Send("pb 1 step");
            }

            // Some of the files are marked as ReadOnly/System, and
            // before deleting the dir we must strip those attrs.
            var files = Directory.GetFiles(dirToClean, "*.*", SearchOption.AllDirectories);
            if (txrx!=null)
                txrx.Send("pb 1 max " + files.Length.ToString());

            foreach (var f in files)
            {
                var a = File.GetAttributes(f);
                // must do ReadOnly bit first - to allow setting other bits.
                if ((a & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    a &= ~FileAttributes.ReadOnly;
                    File.SetAttributes(f, a);
                }
                if (((a & FileAttributes.Hidden) == FileAttributes.Hidden) ||
                    ((a & FileAttributes.System) == FileAttributes.System))
                {
                    a &= ~FileAttributes.Hidden;
                    a &= ~FileAttributes.System;
                    File.SetAttributes(f, a);
                }
                File.Delete(f);
                if (txrx!=null)
                    txrx.Send("pb 1 step");
            }

            // Delete the directory with delay and retry.
            // Sometimes I have a console window in the directory
            // and I want it to not give up so easily.
            int tries =0;
            bool success = false;
            do
            {
                try
                {
                    Directory.Delete(dirToClean, true);
                    success = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(600);
                }
                tries++;
            } while (tries < 100 && !success);
        }





        [TestMethod]
        public void Selector_EdgeCases()
        {
            string Subdir = Path.Combine(TopLevelDir, "A");

            Ionic.FileSelector ff = new Ionic.FileSelector("name = *.txt");
            var list = ff.SelectFiles(Subdir);

            ff.SelectionCriteria = "name = *.bin";
            list = ff.SelectFiles(Subdir);
        }


        private static DateTime twentyDaysAgo;
        private static DateTime todayAtMidnight;
        private static DateTime tomorrow;
        private static DateTime threeDaysAgo;
        private static DateTime threeYearsAgo;
        private static DateTime twoDaysAgo;
        private static DateTime yesterdayAtMidnight;
        private static TimeSpan oneDay;

        private string fodderDirectory;
        private Object LOCK = new Object();
        private int numFodderFiles, numFodderDirs;


        /// <summary>
        ///   Checks a fodder directory to see if suitable.
        /// </summary>
        /// <param name='dir'>the directory to check</param>
        ///
        /// <returns>
        ///   true if the directory contains a goodly number of fodder files.
        /// </returns>
        private bool TryOneFodderDir(string dir)
        {
            if (!Directory.Exists(dir))
                return false;

            var ctime = File.GetCreationTime(dir).ToUniversalTime();
            if ((todayAtMidnight - ctime) > oneDay || (ctime - todayAtMidnight) > oneDay)
                return false;


            var fodderFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

            numFodderFiles = fodderFiles.Length;
            if (numFodderFiles <= 2)
            {
                numFodderFiles = 0;
                return false;
            }

            var fodderDirs = Directory.GetDirectories(dir, "*.*",
                                                      SearchOption.AllDirectories);
            numFodderDirs = fodderDirs.Length;
            if (numFodderDirs <= 2)
            {
                numFodderDirs = numFodderFiles = 0;
                return false;
            }
            return true;
        }


        private string SetupFiles()
        {
            lock (LOCK)
            {
                if (fodderDirectory != null && numFodderFiles > 5)
                    return fodderDirectory;

                string homeDir = System.Environment.GetEnvironmentVariable("TEMP");
                var oldDirs = Directory.GetDirectories(homeDir, "*.SelectorTests");

                foreach (var dir in oldDirs)
                {
                    if (TryOneFodderDir(dir))
                    {
                        fodderDirectory = dir;
                        return dir;
                    }

                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }

                // Arriving here means no good fodder directories exist.
                // Create one.
                ActuallyCreateFodderFiles();
                Assert.IsTrue(TryOneFodderDir(fodderDirectory));
                return fodderDirectory;
            }
        }


        private static void DeleteOldFodderDirectories( Ionic.CopyData.Transceiver txrx )
        {
            // Before creating the directory for the current run, Remove old directories.
            // For some reason the test cleanup code tends to leave these directories??
            string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
            var oldDirs = Directory.GetDirectories(tempDir, "*.SelectorTests");
            if (oldDirs.Length > 0)
            {
                if (txrx != null)
                {
                    txrx.Send("status deleting old directories...");
                    txrx.Send(String.Format("pb 0 max {0}", oldDirs.Length));
                }

                foreach (var dir in oldDirs)
                {
                    CleanDirectory(dir, txrx);
                    if (txrx != null) txrx.Send("pb 0 step");
                }
            }
        }



        private void ActuallyCreateFodderFiles()
        {
            var txrx = TestUtilities.StartProgressMonitor("selector-setup",
                                                          "Selector one-time setup",
                                                          "setting up files...");
            var rnd = new System.Random();
            DeleteOldFodderDirectories(txrx);

            int fileCount = rnd.Next(95) + 95;
            if (txrx!=null)
            {
                txrx.Send("status creating files...");
                txrx.Send(String.Format("pb 0 max {0}", fileCount));
            }

            fodderDirectory = TestUtilities.GenerateUniquePathname("SelectorTests");

            // remember this directory so we can restore later
            string originalDir = Directory.GetCurrentDirectory();

            int entriesAdded = 0;

            // get the base directory for tests:
            Directory.SetCurrentDirectory(CurrentDir);
            Directory.CreateDirectory(fodderDirectory);
            Directory.SetCurrentDirectory(fodderDirectory);

            string[] nameFormats =
                {
                    "file{0:D3}",
                    "{0:D3}",
                    "PrettyLongFileName-{0:D3}",
                    "Very-Long-Filename-{0:D3}-with-a-repeated-segment-{0:D3}-{0:D3}-{0:D3}-{0:D3}",

                };

            string[] dirs =
                {
                    "dir1",
                    "dir1\\dirA",
                    "dir1\\dirB",
                    "dir2"
                };


            foreach (string s in dirs)
                Directory.CreateDirectory(s);

            for (int j = 0; j < fileCount; j++)
            {
                // select the size
                int sz = 0;
                if (j % 5 == 0) sz = rnd.Next(15000) + 150000;
                else if (j % 17 == 1) sz = rnd.Next(50 * 1024) + 1024 * 1024;
                else if (rnd.Next(13) == 0) sz = 8080; // exactly
                else sz = rnd.Next(5000) + 5000;

                // randomly select the format of the file name
                int n = rnd.Next(4);

                // binary or text
                string filename = null;
                if (rnd.Next(2) == 0)
                {
                    filename = Path.Combine(fodderDirectory, String.Format(nameFormats[n], j) + ".txt");
                    TestUtilities.CreateAndFillFileText(filename, sz);
                }
                else
                {
                    filename = Path.Combine(fodderDirectory, String.Format(nameFormats[n], j) + ".bin");
                    TestUtilities.CreateAndFillFileBinary(filename, sz);
                }


                // maybe backdate ctime
                if (rnd.Next(2) == 0)
                {
                    var span = new TimeSpan(_rnd.Next(12),
                                            _rnd.Next(24),
                                            _rnd.Next(59),
                                            _rnd.Next(59));
                    TouchFile(filename, WhichTime.ctime, twentyDaysAgo + span);
                }

                // maybe backdate mtime
                if (rnd.Next(2) == 0)
                {
                    var span = new TimeSpan(_rnd.Next(1),
                                            _rnd.Next(24),
                                            _rnd.Next(59),
                                            _rnd.Next(59));
                    TouchFile(filename, WhichTime.mtime, threeDaysAgo+span);
                }

                // maybe backdate atime
                if (rnd.Next(2) == 0)
                {
                    var span = new TimeSpan(_rnd.Next(24),_rnd.Next(59),_rnd.Next(59));
                    TouchFile(filename, WhichTime.atime, yesterdayAtMidnight+span);
                }

                // set the creation time to "a long time ago" on 1/14th of the files
                if (j % 14 == 0)
                {
                    DateTime x = new DateTime(1998, 4, 29); // julianna
                    var span = new TimeSpan(_rnd.Next(22),
                                            _rnd.Next(24),
                                            _rnd.Next(59),
                                            _rnd.Next(59));
                    File.SetCreationTime(filename, x+span);
                }

                // maybe move to a subdir
                n = rnd.Next(6);
                if (n < 4)
                {
                    string newFilename = Path.Combine(dirs[n], Path.GetFileName(filename));
                    File.Move(filename, newFilename);
                    filename = newFilename;
                }

                // mark some of the files as hidden, system, readonly, etc
                if (j % 9 == 0)
                    File.SetAttributes(filename, FileAttributes.Hidden);
                if (j % 14 == 0)
                    File.SetAttributes(filename, FileAttributes.ReadOnly);
                if (j % 13 == 0)
                    File.SetAttributes(filename, FileAttributes.System);
                if (j % 11 == 0)
                    File.SetAttributes(filename, FileAttributes.Archive);

                entriesAdded++;

                if (txrx != null)
                {
                    txrx.Send("pb 0 step");
                    if (entriesAdded % 8 == 0)
                        txrx.Send(String.Format("status creating files ({0}/{1})", entriesAdded, fileCount));
                }
            }
            // restore the cwd
            Directory.SetCurrentDirectory(originalDir);

            txrx.Send("stop");
        }



        class Trial
        {
            public string Label;
            public string C1;
            public string C2;
        }




        [TestMethod]
        public void Selector_SelectFiles()
        {
            Directory.SetCurrentDirectory(TopLevelDir);

            Trial[] trials = new Trial[]
                {
                    new Trial { Label = "name", C1 = "name = *.txt", C2 = "name = *.bin" },
                    new Trial { Label = "name (shorthand)", C1 = "*.txt", C2 = "*.bin" },
                    new Trial { Label = "size", C1 = "size < 7500", C2 = "size >= 7500" },
                    new Trial { Label = "size", C1 = "size = 8080", C2 = "size != 8080" },
                    new Trial { Label = "name & size",
                                C1 = "name = *.bin AND size > 7500",
                                C2 = "name != *.bin  OR  size <= 7500",
                    },
                    new Trial { Label = "name XOR name",
                                C1 = "name = *.bin XOR name = *4.*",
                                C2 = "(name != *.bin OR name = *4.*) AND (name = *.bin OR name != *4.*)",
                    },
                    new Trial { Label = "name XOR size",
                                C1 = "name = *.bin XOR size > 100k",
                                C2 = "(name != *.bin OR size > 100k) AND (name = *.bin OR size <= 100k)",
                    },
                    new Trial
                    {
                        Label = "mtime",
                        C1 = String.Format("mtime < {0}", twentyDaysAgo.ToString("yyyy-MM-dd")),
                        C2 = String.Format("mtime >= {0}", twentyDaysAgo.ToString("yyyy-MM-dd")),
                    },
                    new Trial
                    {
                        Label = "ctime",
                        C1 = String.Format("mtime < {0}", threeDaysAgo.ToString("yyyy-MM-dd")),
                        C2 = String.Format("mtime >= {0}", threeDaysAgo.ToString("yyyy-MM-dd")),
                    },
                    new Trial
                    {
                        Label = "atime",
                        C1 = String.Format("mtime < {0}", yesterdayAtMidnight.ToString("yyyy-MM-dd")),
                        C2 = String.Format("mtime >= {0}", yesterdayAtMidnight.ToString("yyyy-MM-dd")),
                    },
                    new Trial { Label = "size (100k)", C1="size > 100k", C2="size <= 100kb", },
                    new Trial { Label = "size (1mb)", C1="size > 1m", C2="size <= 1mb", },
                    new Trial { Label = "size (1gb)", C1="size > 1g", C2="size <= 1gb", },
                    new Trial { Label = "attributes (Hidden)", C1 = "attributes = H", C2 = "attributes != H" },
                    new Trial { Label = "attributes (ReadOnly)", C1 = "attributes = R", C2 = "attributes != R" },
                    new Trial { Label = "attributes (System)", C1 = "attributes = S", C2 = "attributes != S" },
                    new Trial { Label = "attributes (Archive)", C1 = "attributes = A", C2 = "attributes != A" },

                };


            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectFiles.zip");
            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            int count1, count2;
            //String filename = null;

            SetupFiles();
            var topLevelFiles = Directory.GetFiles(fodderDirectory, "*.*", SearchOption.TopDirectoryOnly);

            for (int m = 0; m < trials.Length; m++)
            {
                Ionic.FileSelector ff = new Ionic.FileSelector(trials[m].C1);
                var list = ff.SelectFiles(fodderDirectory);
                TestContext.WriteLine("=======================================================");
                TestContext.WriteLine("Selector: " + ff.ToString());
                TestContext.WriteLine("Criteria({0})", ff.SelectionCriteria);
                TestContext.WriteLine("Count({0})", list.Count);
                count1 = 0;
                foreach (string s in list)
                {
                    switch (m)
                    {
                        case 0:
                        case 1:
                            Assert.IsTrue(s.EndsWith(".txt"));
                            break;
                        case 2:
                            {
                                FileInfo fi = new FileInfo(s);
                                Assert.IsTrue(fi.Length < 7500);
                            }
                            break;
                        case 4:
                            {
                                FileInfo fi = new FileInfo(s);
                                bool x = s.EndsWith(".bin") && fi.Length > 7500;
                                Assert.IsTrue(x);
                            }
                            break;
                    }
                    count1++;
                }

                ff = new Ionic.FileSelector(trials[m].C2);
                list = ff.SelectFiles(fodderDirectory);
                TestContext.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                TestContext.WriteLine("Criteria({0})", ff.SelectionCriteria);
                TestContext.WriteLine("Count({0})", list.Count);
                count2 = 0;
                foreach (string s in list)
                {
                    switch (m)
                    {
                        case 0:
                        case 1:
                            Assert.IsTrue(s.EndsWith(".bin"));
                            break;
                        case 2:
                            {
                                FileInfo fi = new FileInfo(s);
                                Assert.IsTrue(fi.Length >= 7500);
                            }
                            break;
                        case 4:
                            {
                                FileInfo fi = new FileInfo(s);
                                bool x = !s.EndsWith(".bin") || fi.Length <= 7500;
                                Assert.IsTrue(x);
                            }
                            break;
                    }
                    count2++;
                }
                Assert.AreEqual<Int32>(topLevelFiles.Length, count1 + count2);
            }
        }








        [TestMethod, Timeout(7200000)]
        public void Selector_AddSelectedFiles()
        {
            Directory.SetCurrentDirectory(TopLevelDir);

            Trial[] trials = new Trial[]
                {
                    new Trial { Label = "name", C1 = "name = *.txt", C2 = "name = *.bin" },
                    new Trial { Label = "name (shorthand)", C1 = "*.txt", C2 = "*.bin" },
                    new Trial { Label = "attributes (Hidden)", C1 = "attributes = H", C2 = "attributes != H" },
                    new Trial { Label = "attributes (ReadOnly)", C1 = "attributes = R", C2 = "attributes != R" },
                    new Trial { Label = "mtime", C1 = "mtime < 2007-01-01", C2 = "mtime > 2007-01-01" },
                    new Trial { Label = "atime", C1 = "atime < 2007-01-01", C2 = "atime > 2007-01-01" },
                    new Trial { Label = "ctime", C1 = "ctime < 2007-01-01", C2 = "ctime > 2007-01-01" },
                    new Trial { Label = "size", C1 = "size < 7500", C2 = "size >= 7500" },

                    new Trial { Label = "name & size",
                                C1 = "name = *.bin AND size > 7500",
                                C2 = "name != *.bin  OR  size <= 7500",
                    },

                    new Trial { Label = "name, size & attributes",
                                C1 = "name = *.bin AND size > 8kb and attributes = H",
                                C2 = "name != *.bin  OR  size <= 8kb or attributes != H",
                    },

                    new Trial { Label = "name, size, time & attributes.",
                                C1 = "name = *.bin AND size > 7k and mtime < 2007-01-01 and attributes = H",
                                C2 = "name != *.bin  OR  size <= 7k or mtime > 2007-01-01 or attributes != H",
                    },
                };

            _txrx = TestUtilities.StartProgressMonitor("AddSelectedFiles", "AddSelectedFiles", "starting up...");

            string[] zipFileToCreate = {
                Path.Combine(TopLevelDir, "Selector_AddSelectedFiles-1.zip"),
                Path.Combine(TopLevelDir, "Selector_AddSelectedFiles-2.zip")
            };

            Assert.IsFalse(File.Exists(zipFileToCreate[0]), "The zip file '{0}' already exists.", zipFileToCreate[0]);
            Assert.IsFalse(File.Exists(zipFileToCreate[1]), "The zip file '{0}' already exists.", zipFileToCreate[1]);

            int count1, count2;

            SetupFiles();
            var topLevelFiles = Directory.GetFiles(fodderDirectory, "*.*", SearchOption.TopDirectoryOnly);

            string currentDir = Directory.GetCurrentDirectory();
            _txrx.Send(String.Format("pb 0 max {0}", 2 * (trials.Length + 1)));

            _txrx.Send("pb 0 step");

            for (int m = 0; m < trials.Length; m++)
            {
                _txrx.Send("test AddSelectedFiles");
                _txrx.Send("pb 1 max 4");
                _txrx.Send(String.Format("status test {0}/{1}: creating zip #1/2",
                                         m + 1, trials.Length));
                TestContext.WriteLine("===============================================");
                TestContext.WriteLine("AddSelectedFiles() [{0}]", trials[m].Label);
                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.AddSelectedFiles(trials[m].C1, fodderDirectory, "");
                    zip1.Save(zipFileToCreate[0]);
                }
                count1 = TestUtilities.CountEntries(zipFileToCreate[0]);
                TestContext.WriteLine("C1({0}) Count({1})", trials[m].C1, count1);
                _txrx.Send("pb 1 step");
                System.Threading.Thread.Sleep(100);
                _txrx.Send("pb 0 step");

                _txrx.Send(String.Format("status test {0}/{1}: creating zip #2/2",
                                         m + 1, trials.Length));
                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.AddSelectedFiles(trials[m].C2, fodderDirectory, "");
                    zip1.Save(zipFileToCreate[1]);
                }
                count2 = TestUtilities.CountEntries(zipFileToCreate[1]);
                TestContext.WriteLine("C2({0}) Count({1})", trials[m].C2, count2);
                Assert.AreEqual<Int32>(topLevelFiles.Length, count1 + count2);
                _txrx.Send("pb 1 step");

                /// =======================================================
                /// Now, select entries from that ZIP
                _txrx.Send(String.Format("status test {0}/{1}: selecting zip #1/2",
                                         m + 1, trials.Length));
                using (ZipFile zip1 = ZipFile.Read(zipFileToCreate[0]))
                {
                    var selected1 = zip1.SelectEntries(trials[m].C1);
                    Assert.AreEqual<Int32>(selected1.Count, count1);
                }
                _txrx.Send("pb 1 step");

                _txrx.Send(String.Format("status test {0}/{1}: selecting zip #2/2",
                                         m + 1, trials.Length));
                using (ZipFile zip1 = ZipFile.Read(zipFileToCreate[1]))
                {
                    var selected2 = zip1.SelectEntries(trials[m].C2);
                    Assert.AreEqual<Int32>(selected2.Count, count2);
                }
                _txrx.Send("pb 1 step");

                _txrx.Send("pb 0 step");
            }

        }


        [TestMethod]
        public void Selector_AddSelectedFiles_2()
        {
            string zipFileToCreate = "Selector_AddSelectedFiles_2.zip";
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var files = TestUtilities.GenerateFilesFlat(dirToZip);
            var txtFiles = Directory.GetFiles(dirToZip, "*.txt", SearchOption.AllDirectories);

            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddSelectedFiles("*.txt");
                zip1.Save(zipFileToCreate);
            }

            Assert.AreEqual<Int32>(0, TestUtilities.CountEntries(zipFileToCreate));

            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddSelectedFiles("*.txt", true);
                zip1.Save(zipFileToCreate);
            }

            Assert.AreEqual<Int32>(txtFiles.Length, TestUtilities.CountEntries(zipFileToCreate));
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddSelectedFiles("*.txt", ".", true);
                zip1.Save(zipFileToCreate);
            }

            Assert.AreEqual<Int32>(txtFiles.Length, TestUtilities.CountEntries(zipFileToCreate));

        }


        [TestMethod]
        public void Selector_AddSelectedFiles_Checkcase_file()
        {
            string zipFileToCreate = "AddSelectedFiles_Checkcase.zip";
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            Directory.SetCurrentDirectory(dirToZip);
            var f2 = Directory.GetFiles(".", "*.*");
            Array.ForEach(f2, x => { File.Move(x,Path.GetFileName(x).ToUpper()); });
            Directory.SetCurrentDirectory(TopLevelDir);

            var txtFiles = Directory.GetFiles(dirToZip, "*.txt",
                                              SearchOption.AllDirectories);

            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddSelectedFiles("*.txt", dirToZip);
                zip1.Save(zipFileToCreate);
            }

            int nEntries = 0;
            // now, verify that we have not downcased the filenames
            using (ZipFile zip2 = new ZipFile(zipFileToCreate))
            {
                foreach (var entry in zip2.Entries)
                {
                    Assert.IsFalse(entry.FileName.Equals(entry.FileName.ToLower()));
                    nEntries++;
                }
            }
            Assert.IsFalse(nEntries < 2, "not enough entries");

        }



        [TestMethod]
        public void Selector_AddSelectedFiles_Checkcase_directory()
        {
            string zipFileToCreate = "AddSelectedFiles_Checkcase.zip";
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()).ToUpper();
            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            var txtFiles = Directory.GetFiles(dirToZip, "*.txt",
                                              SearchOption.AllDirectories);

            Assert.IsFalse(txtFiles.Length < 3, "not enough entries (n={0})",
                           txtFiles.Length);

            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddSelectedFiles("*.txt", dirToZip);
                zip1.Save(zipFileToCreate);
            }

            int nEntries = 0;
            // now, verify that we have not downcased the filenames
            using (ZipFile zip2 = new ZipFile(zipFileToCreate))
            {
                foreach (var entry in zip2.Entries)
                {
                    Assert.IsFalse(entry.FileName.Equals(entry.FileName.ToLower()));
                    nEntries++;
                }
            }
            Assert.IsFalse(nEntries < 3, "not enough entries (n={0})", nEntries);
        }


        [TestMethod]
        public void Selector_AddSelectedFiles_Checkcase_directory_2()
        {
            string zipFileToCreate = "AddSelectedFiles_Checkcase.zip";
            string shortDirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()).ToUpper();
            string dirToZip = Path.Combine(TopLevelDir, shortDirToZip); // fully qualified
            var files = TestUtilities.GenerateFilesFlat(shortDirToZip);
            string keyword = "Ammon";
            int n = _rnd.Next(3)+2;
            for (int i=0; i < n; i++)
            {
                Directory.SetCurrentDirectory(dirToZip);
                string subdir = keyword + i;
                TestUtilities.GenerateFilesFlat(subdir);
                Directory.SetCurrentDirectory(subdir);
                var f2 = Directory.GetFiles(".", "*.*");
                int k = 2;
                Array.ForEach(f2, x => {
                        File.Move(x, String.Format("{0}.{1:D5}.txt", keyword.ToUpper(), k++)); });
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            TestContext.WriteLine("Create zip file");
            using (ZipFile zip1 = new ZipFile())
            {
                var criterion = "name = *\\" + keyword + "?\\*.txt";
                zip1.AddSelectedFiles(criterion, ".\\" + shortDirToZip, "files", true);
                zip1.Save(zipFileToCreate);
            }

            TestContext.WriteLine("Verify case of entry FileNames");
            int nEntries = 0;
            // now, verify that we have not downcased entry.FileName
            using (ZipFile zip2 = new ZipFile(zipFileToCreate))
            {
                foreach (var entry in zip2.Entries)
                {
                    TestContext.WriteLine("Check {0}", entry.FileName);
                    Assert.AreNotEqual<String>(entry.FileName,
                                               entry.FileName.ToLower(),
                                               entry.FileName);
                    nEntries++;
                }
            }
            Assert.IsFalse(nEntries < 3, "not enough entries");
        }



        [TestMethod]
        public void Selector_SelectEntries_FwdSlash_wi13350()
        {
            string zipFileToCreate = "SelectEntries.zip";

            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            TestContext.WriteLine("Create zip file");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(dirToZip, dirToZip);
                zip1.Save(zipFileToCreate);
            }

            // Select Entries
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                var selection1 = zip2.SelectEntries("name = " + dirToZip + "\\" + "*.txt");
                //var selection1 = zip2.SelectEntries("name = *.txt");
                Assert.IsTrue(selection1.Count > 2, "{0} is simply not enough entries!",
                              selection1.Count);
                var selection2 = zip2.SelectEntries(dirToZip + "/" + "*.txt");
                Assert.AreEqual<int>(selection1.Count,
                                     selection2.Count,
                                     "{0} != {1}",
                                     selection1.Count,
                                     selection2.Count);
            }
        }


        [TestMethod]
        public void Selector_CheckRemove_wi10499()
        {
            string zipFileToCreate = "CheckRemove.zip";
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            TestContext.WriteLine("Create zip file");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(dirToZip, dirToZip);
                zip1.Save(zipFileToCreate);
            }

            int nBefore= 0, nAfter = 0, nRemoved = 0;
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                ICollection<ZipEntry> entries = zip2.SelectEntries("*.txt");
                Assert.IsFalse(entries.Count < 3, "not enough entries");
                nBefore = entries.Count;

                foreach(ZipEntry entry in entries)
                {
                    TestContext.WriteLine("Removing {0}", entry.FileName);
                    zip2.RemoveEntry(entry);
                    nRemoved++;
                }
                var remainingEntries = zip2.SelectEntries("*.txt");
                nAfter = remainingEntries.Count;
                TestContext.WriteLine("Remaining:");
                foreach(ZipEntry entry in remainingEntries)
                {
                    TestContext.WriteLine("  {0}",
                                          entry.FileName);
                }
            }

            Assert.IsTrue(nBefore>nAfter,"Removal appeared to have no effect.");
            Assert.IsTrue(nBefore-nRemoved==nAfter,"Wrong number of entries {0}-{1}!={2}",
                          nBefore, nRemoved, nAfter);
        }


        private enum WhichTime
        {
            atime,
            mtime,
            ctime,
        }



        private static void TouchFile(string strFile, WhichTime which, DateTime stamp)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(strFile);
            if (which == WhichTime.atime)
                fi.LastAccessTime = stamp;
            else if (which == WhichTime.ctime)
                fi.CreationTime = stamp;
            else if (which == WhichTime.mtime)
                fi.LastWriteTime = stamp;
            else throw new System.ArgumentException("which");
        }



        [TestMethod]
        public void Selector_SelectEntries_ByTime()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectEntries.zip");

            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            SetupFiles();

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip...");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(fodderDirectory, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(numFodderFiles, TestUtilities.CountEntries(zipFileToCreate), "A");

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, SelectEntries() by date...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                var totalEntries = numFodderFiles+numFodderDirs;

                // all of the files should have been modified either
                // after midnight today, or before.
                string crit = String.Format("mtime >= {0}", todayAtMidnight.ToString("yyyy-MM-dd"));
                var selected1 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case A({0}) count({1})", crit, selected1.Count);
                crit = String.Format("mtime < {0}", todayAtMidnight.ToString("yyyy-MM-dd"));
                var selected2 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case B({0})  count({1})", crit, selected2.Count);
                Assert.AreEqual<Int32>(totalEntries,
                                       selected1.Count + selected2.Count, "B");


                // some nonzero (high) number of files should have been
                // created in the past twenty days.
                crit = String.Format("ctime >= {0}", twentyDaysAgo.ToString("yyyy-MM-dd"));
                var selected3 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case C({0}) count({1})", crit, selected3.Count);
                Assert.IsTrue(selected3.Count > 0, "C");


                // a nonzero number should be marked as having been
                // created more than 3 years ago.
                crit = String.Format("ctime < {0}",
                                     threeYearsAgo.ToString("yyyy-MM-dd"));
                var selected4 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case D({0})  count({1})", crit, selected4.Count);
                Assert.IsTrue(selected4.Count > 0, "D");

                // None of the files should have been created
                // more than 20 years ago
                var twentyYearsAgo = new DateTime(DateTime.Now.Year - 20,
                                                  DateTime.Now.Month,
                                                  DateTime.Now.Day);
                crit = String.Format("ctime < {0}",
                                     twentyYearsAgo.ToString("yyyy-MM-dd"));
                var selected5 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case F({0})  count({1})", crit, selected5.Count);
                Assert.IsTrue(selected5.Count==0, "F");

                // Some number of the files should have been created
                // more than three days ago
                crit = String.Format("ctime < {0}",
                                     threeDaysAgo.ToString("yyyy-MM-dd"));
                selected5 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case E({0})  count({1})", crit, selected5.Count);
                Assert.IsTrue(selected5.Count>0, "E");

                // summing all those created more than three days ago,
                // with those created in the last three days, should be all entries.
                crit = String.Format("ctime >= {0}", threeDaysAgo.ToString("yyyy-MM-dd"));
                var selected6 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case G({0})  count({1})", crit, selected6.Count);
                Assert.IsTrue(selected6.Count>0, "G");
                Assert.AreEqual<Int32>(totalEntries, selected5.Count + selected6.Count, "G");


                // some number should have been accessed in the past 2 days
                crit = String.Format("atime >= {0}  and  atime < {1}",
                                     twoDaysAgo.ToString("yyyy-MM-dd"),
                                     todayAtMidnight.ToString("yyyy-MM-dd"));
                selected5 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case H({0})  count({1})", crit, selected5.Count);
                Assert.IsTrue(selected5.Count > 0, "H");

                // those accessed *exactly* at midnight yesterday, plus
                // those NOT = all entries
                crit = String.Format("atime = {0}",
                                     yesterdayAtMidnight.ToString("yyyy-MM-dd"));
                selected5 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case I({0})  count({1})", crit, selected5.Count);

                crit = String.Format("atime != {0}",
                                     yesterdayAtMidnight.ToString("yyyy-MM-dd"));
                selected6 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case J({0})  count({1})", crit, selected6.Count);
                Assert.AreEqual<Int32>(totalEntries, selected5.Count + selected6.Count, "J");

                // those marked as last accessed more than 20 days ago == empty set
                crit = String.Format("atime <= {0}",
                                     twentyDaysAgo.ToString("yyyy-MM-dd"));
                selected5 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Case K({0})  count({1})", crit, selected5.Count);
                Assert.AreEqual<Int32>(0, selected5.Count, "K");
            }
        }



        [TestMethod]
        public void Selector_ExtractSelectedEntries()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_ExtractSelectedEntries.zip");

            SetupFiles();

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip...");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(fodderDirectory, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(numFodderFiles, TestUtilities.CountEntries(zipFileToCreate));

            string extractDir = "extract";

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, ExtractSelectedEntries() by date...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                string crit = String.Format("mtime >= {0}", todayAtMidnight.ToString("yyyy-MM-dd"));
                TestContext.WriteLine("Criteria({0})", crit);
                zip1.ExtractSelectedEntries(crit, null, extractDir);
            }

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, ExtractSelectedEntries() by date, with overwrite...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                string crit = String.Format("mtime >= {0}", todayAtMidnight.ToString("yyyy-MM-dd"));
                TestContext.WriteLine("Criteria({0})", crit);
                zip1.ExtractSelectedEntries(crit, null, extractDir, ExtractExistingFileAction.OverwriteSilently);
            }


            // workitem 9174: test ExtractSelectedEntries using a directoryPathInArchive
            List<String> dirs = new List<String>();
            // first, get the list of directories used by all entries
            TestContext.WriteLine("Reading zip, ...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                foreach (var e in zip1)
                {
                    TestContext.WriteLine("entry {0}", e.FileName);
                    string p = Path.GetDirectoryName(e.FileName.Replace("/", "\\"));
                    if (!dirs.Contains(p)) dirs.Add(p);
                }
            }

            // with or without trailing slash
            for (int i = 0; i < 2; i++)
            {
                int grandTotal = 0;
                extractDir = String.Format("extract.{0}", i);
                for (int j = 0; j < dirs.Count; j++)
                {
                    string d = dirs[j];
                    if (i == 1) d += "\\";
                    TestContext.WriteLine("====================================================");
                    TestContext.WriteLine("Reading zip, ExtractSelectedEntries() by name, with directoryInArchive({0})...", d);
                    using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
                    {
                        string crit = "name = *.bin";
                        TestContext.WriteLine("Criteria({0})", crit);
                        var s = zip1.SelectEntries(crit, d);
                        TestContext.WriteLine("  {0} entries", s.Count);
                        grandTotal += s.Count;
                        zip1.ExtractSelectedEntries(crit, d, extractDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                TestContext.WriteLine("====================================================");
                TestContext.WriteLine("Total for all dirs: {0} entries", grandTotal);

                var extracted = Directory.GetFiles(extractDir, "*.bin", SearchOption.AllDirectories);

                Assert.AreEqual<Int32>(grandTotal, extracted.Length);
            }
        }




        [TestMethod]
        public void Selector_SelectEntries_ByName()
        {
            // Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectEntries.zip");

            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            //int count1, count2;
            int entriesAdded = 0;
            String filename = null;

            string subDir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subDir);

            int fileCount = _rnd.Next(33) + 33;
            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Files being added to the zip:");
            for (int j = 0; j < fileCount; j++)
            {
                // select binary or text
                if (_rnd.Next(2) == 0)
                {
                    filename = Path.Combine(subDir, String.Format("file{0:D3}.txt", j));
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(5000) + 5000);
                }
                else
                {
                    filename = Path.Combine(subDir, String.Format("file{0:D3}.bin", j));
                    TestUtilities.CreateAndFillFileBinary(filename, _rnd.Next(5000) + 5000);
                }
                TestContext.WriteLine(Path.GetFileName(filename));
                entriesAdded++;
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip...");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(subDir, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(entriesAdded, TestUtilities.CountEntries(zipFileToCreate));



            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, SelectEntries() by name...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                var selected1 = zip1.SelectEntries("name = *.txt");
                var selected2 = zip1.SelectEntries("name = *.bin");
                var selected3 = zip1.SelectEntries("name = *.bin OR name = *.txt");
                TestContext.WriteLine("Found {0} text files, {0} bin files.", selected1.Count, selected2.Count);
                TestContext.WriteLine("Text files:");
                foreach (ZipEntry e in selected1)
                {
                    TestContext.WriteLine(e.FileName);
                }
                Assert.AreEqual<Int32>(entriesAdded, selected1.Count + selected2.Count);
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, SelectEntries() using shorthand filters...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                var selected1 = zip1.SelectEntries("*.txt");
                var selected2 = zip1.SelectEntries("*.bin");
                TestContext.WriteLine("Text files:");
                foreach (ZipEntry e in selected1)
                {
                    TestContext.WriteLine(e.FileName);
                }
                Assert.AreEqual<Int32>(entriesAdded, selected1.Count + selected2.Count);
            }

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, SelectEntries() again ...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                string crit = "name = *.txt AND name = *.bin";
                // none of the entries should match this:
                var selected1 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Criteria({0})  count({1})", crit, selected1.Count);
                Assert.AreEqual<Int32>(0, selected1.Count);

                // all of the entries should match this:
                crit = "name = *.txt XOR name = *.bin";
                var selected2 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Criteria({0})  count({1})", crit, selected2.Count);
                Assert.AreEqual<Int32>(entriesAdded, selected2.Count);

                // try an compound criterion with XOR
                crit = "name = *.bin XOR name = *2.*";
                var selected3 = zip1.SelectEntries(crit);
                Assert.IsTrue(selected3.Count > 0);
                TestContext.WriteLine("Criteria({0})  count({1})", crit, selected3.Count);

                // factor out the XOR
                crit = "(name = *.bin AND name != *2.*) OR (name != *.bin AND name = *2.*)";
                var selected4 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Criteria({0})  count({1})", crit, selected4.Count);
                Assert.AreEqual<Int32>(selected3.Count, selected4.Count);

                // take the negation of the XOR criterion
                crit = "(name != *.bin OR name = *2.*) AND (name = *.bin OR name != *2.*)";
                var selected5 = zip1.SelectEntries(crit);
                TestContext.WriteLine("Criteria({0})  count({1})", crit, selected4.Count);
                Assert.IsTrue(selected5.Count > 0);
                Assert.AreEqual<Int32>(entriesAdded, selected3.Count + selected5.Count);
            }
        }



        [TestMethod]
        public void Selector_SelectEntries_ByName_NamesWithSpaces()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectEntries_Spaces.zip");

            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            //int count1, count2;
            int entriesAdded = 0;
            String filename = null;

            string subDir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subDir);

            int fileCount = _rnd.Next(44) + 44;
            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Files being added to the zip:");
            for (int j = 0; j < fileCount; j++)
            {
                string space = (_rnd.Next(2) == 0) ? " " : "";
                if (_rnd.Next(2) == 0)
                {
                    filename = Path.Combine(subDir, String.Format("file{1}{0:D3}.txt", j, space));
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(5000) + 5000);
                }
                else
                {
                    filename = Path.Combine(subDir, String.Format("file{1}{0:D3}.bin", j, space));
                    TestUtilities.CreateAndFillFileBinary(filename, _rnd.Next(5000) + 5000);
                }
                TestContext.WriteLine(Path.GetFileName(filename));
                entriesAdded++;
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip...");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(subDir, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(entriesAdded, TestUtilities.CountEntries(zipFileToCreate));



            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                var selected1 = zip1.SelectEntries("name = *.txt");
                var selected2 = zip1.SelectEntries("name = *.bin");
                TestContext.WriteLine("Text files:");
                foreach (ZipEntry e in selected1)
                {
                    TestContext.WriteLine(e.FileName);
                }
                Assert.AreEqual<Int32>(entriesAdded, selected1.Count + selected2.Count);
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, using name patterns that contain spaces...");
            string[] selectionStrings = { "name = '* *.txt'",
                                          "name = '* *.bin'",
                                          "name = *.txt and name != '* *.txt'",
                                          "name = *.bin and name != '* *.bin'",
            };
            int count = 0;
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                foreach (string selectionCriteria in selectionStrings)
                {
                    var selected1 = zip1.SelectEntries(selectionCriteria);
                    count += selected1.Count;
                    TestContext.WriteLine("  For criteria ({0}), found {1} files.", selectionCriteria, selected1.Count);
                }
            }
            Assert.AreEqual<Int32>(entriesAdded, count);

        }


        [TestMethod]
        public void Selector_RemoveSelectedEntries_Spaces()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_RemoveSelectedEntries_Spaces.zip");

            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            //int count1, count2;
            int entriesAdded = 0;
            String filename = null;

            string subDir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subDir);

            int fileCount = _rnd.Next(44) + 44;
            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Files being added to the zip:");
            for (int j = 0; j < fileCount; j++)
            {
                string space = (_rnd.Next(2) == 0) ? " " : "";
                if (_rnd.Next(2) == 0)
                {
                    filename = Path.Combine(subDir, String.Format("file{1}{0:D3}.txt", j, space));
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(5000) + 5000);
                }
                else
                {
                    filename = Path.Combine(subDir, String.Format("file{1}{0:D3}.bin", j, space));
                    TestUtilities.CreateAndFillFileBinary(filename, _rnd.Next(5000) + 5000);
                }
                TestContext.WriteLine(Path.GetFileName(filename));
                entriesAdded++;
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip...");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(subDir, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(entriesAdded, TestUtilities.CountEntries(zipFileToCreate));


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, using name patterns that contain spaces...");
            string[] selectionStrings = { "name = '* *.txt'",
                                          "name = '* *.bin'",
                                          "name = *.txt and name != '* *.txt'",
                                          "name = *.bin and name != '* *.bin'",
            };
            foreach (string selectionCriteria in selectionStrings)
            {
                using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
                {
                    var selected1 = zip1.SelectEntries(selectionCriteria);
                    zip1.RemoveEntries(selected1);
                    TestContext.WriteLine("for pattern {0}, Removed {1} entries", selectionCriteria, selected1.Count);
                    zip1.Save();
                }

            }

            Assert.AreEqual<Int32>(0, TestUtilities.CountEntries(zipFileToCreate));
        }


        [TestMethod]
        public void Selector_RemoveSelectedEntries2()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_RemoveSelectedEntries2.zip");

            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            //int count1, count2;
            int entriesAdded = 0;
            String filename = null;

            string subDir = Path.Combine(TopLevelDir, "A");
            Directory.CreateDirectory(subDir);

            int fileCount = _rnd.Next(44) + 44;
            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Files being added to the zip:");
            for (int j = 0; j < fileCount; j++)
            {
                string space = (_rnd.Next(2) == 0) ? " " : "";
                if (_rnd.Next(2) == 0)
                {
                    filename = Path.Combine(subDir, String.Format("file{1}{0:D3}.txt", j, space));
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(5000) + 5000);
                }
                else
                {
                    filename = Path.Combine(subDir, String.Format("file{1}{0:D3}.bin", j, space));
                    TestUtilities.CreateAndFillFileBinary(filename, _rnd.Next(5000) + 5000);
                }
                TestContext.WriteLine(Path.GetFileName(filename));
                entriesAdded++;
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip...");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddDirectory(subDir, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(entriesAdded, TestUtilities.CountEntries(zipFileToCreate));


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Reading zip, using name patterns that contain spaces...");
            string[] selectionStrings = { "name = '* *.txt'",
                                          "name = '* *.bin'",
                                          "name = *.txt and name != '* *.txt'",
                                          "name = *.bin and name != '* *.bin'",
            };
            foreach (string selectionCriteria in selectionStrings)
            {
                using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
                {
                    var selected1 = zip1.SelectEntries(selectionCriteria);
                    ZipEntry[] entries = new ZipEntry[selected1.Count];
                    selected1.CopyTo(entries, 0);
                    string[] names = Array.ConvertAll(entries, x => x.FileName);
                    zip1.RemoveEntries(names);
                    TestContext.WriteLine("for pattern {0}, Removed {1} entries", selectionCriteria, selected1.Count);
                    zip1.Save();
                }

            }

            Assert.AreEqual<Int32>(0, TestUtilities.CountEntries(zipFileToCreate));
        }



        [TestMethod]
        public void Selector_SelectEntries_subDirs()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectFiles_subDirs.zip");

            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            int count1, count2;

            string fodder = Path.Combine(TopLevelDir, "fodder");
            Directory.CreateDirectory(fodder);


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating files...");
            int entries = 0;
            int i = 0;
            int subdirCount = _rnd.Next(17) + 9;
            //int subdirCount = _rnd.Next(3) + 2;
            var FileCount = new Dictionary<string, int>();

            var checksums = new Dictionary<string, string>();
            // I don't actually verify the checksums in this method...


            for (i = 0; i < subdirCount; i++)
            {
                string subDirShort = new System.String(new char[] { (char)(i + 65) });
                string subDir = Path.Combine(fodder, subDirShort);
                Directory.CreateDirectory(subDir);

                int filecount = _rnd.Next(8) + 8;
                //int filecount = _rnd.Next(2) + 2;
                FileCount[subDirShort] = filecount;
                for (int j = 0; j < filecount; j++)
                {
                    string filename = String.Format("file{0:D4}.x", j);
                    string fqFilename = Path.Combine(subDir, filename);
                    TestUtilities.CreateAndFillFile(fqFilename, _rnd.Next(1000) + 1000);

                    var chk = TestUtilities.ComputeChecksum(fqFilename);
                    var s = TestUtilities.CheckSumToString(chk);
                    var t1 = Path.GetFileName(fodder);
                    var t2 = Path.Combine(t1, subDirShort);
                    var key = Path.Combine(t2, filename);
                    key = TestUtilities.TrimVolumeAndSwapSlashes(key);
                    TestContext.WriteLine("chk[{0}]= {1}", key, s);
                    checksums.Add(key, s);
                    entries++;
                }
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip ({0} entries in {1} subdirs)...", entries, subdirCount);
            // add all the subdirectories into a new zip
            using (ZipFile zip1 = new ZipFile())
            {
                // add all of those subdirectories (A, B, C...) into the root in the zip archive
                zip1.AddDirectory(fodder, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(entries, TestUtilities.CountEntries(zipFileToCreate));


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Selecting entries by directory...");

            for (int j = 0; j < 2; j++)
            {
                count1 = 0;
                using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
                {
                    for (i = 0; i < subdirCount; i++)
                    {
                        string dirInArchive = new System.String(new char[] { (char)(i + 65) });
                        if (j == 1) dirInArchive += "\\";
                        var selected1 = zip1.SelectEntries("*.*", dirInArchive);
                        count1 += selected1.Count;
                        TestContext.WriteLine("--------------\nfiles in dir {0} ({1}):",
                                              dirInArchive, selected1.Count);
                        foreach (ZipEntry e in selected1)
                            TestContext.WriteLine(e.FileName);
                    }
                    Assert.AreEqual<Int32>(entries, count1);
                }
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Selecting entries by directory and size...");
            count1 = 0;
            count2 = 0;
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                for (i = 0; i < subdirCount; i++)
                {
                    string dirInArchive = new System.String(new char[] { (char)(i + 65) });
                    var selected1 = zip1.SelectEntries("size > 1500", dirInArchive);
                    count1 += selected1.Count;
                    TestContext.WriteLine("--------------\nfiles in dir {0} ({1}):",
                                          dirInArchive, selected1.Count);
                    foreach (ZipEntry e in selected1)
                        TestContext.WriteLine(e.FileName);
                }

                var selected2 = zip1.SelectEntries("size <= 1500");
                count2 = selected2.Count;
                Assert.AreEqual<Int32>(entries, count1 + count2 - subdirCount);
            }

        }



        [TestMethod]
        public void Selector_SelectEntries_Fullpath()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectFiles_Fullpath.zip");

            Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

            int count1, count2;

            string fodder = Path.Combine(TopLevelDir, "fodder");
            Directory.CreateDirectory(fodder);


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating files...");
            int entries = 0;
            int i = 0;
            int subdirCount = _rnd.Next(17) + 9;
            //int subdirCount = _rnd.Next(3) + 2;
            var FileCount = new Dictionary<string, int>();

            var checksums = new Dictionary<string, string>();
            // I don't actually verify the checksums in this method...


            for (i = 0; i < subdirCount; i++)
            {
                string subDirShort = new System.String(new char[] { (char)(i + 65) });
                string subDir = Path.Combine(fodder, subDirShort);
                Directory.CreateDirectory(subDir);

                int filecount = _rnd.Next(8) + 8;
                //int filecount = _rnd.Next(2) + 2;
                FileCount[subDirShort] = filecount;
                for (int j = 0; j < filecount; j++)
                {
                    string filename = String.Format("file{0:D4}.x", j);
                    string fqFilename = Path.Combine(subDir, filename);
                    TestUtilities.CreateAndFillFile(fqFilename, _rnd.Next(1000) + 1000);

                    var chk = TestUtilities.ComputeChecksum(fqFilename);
                    var s = TestUtilities.CheckSumToString(chk);
                    var t1 = Path.GetFileName(fodder);
                    var t2 = Path.Combine(t1, subDirShort);
                    var key = Path.Combine(t2, filename);
                    key = TestUtilities.TrimVolumeAndSwapSlashes(key);
                    TestContext.WriteLine("chk[{0}]= {1}", key, s);
                    checksums.Add(key, s);
                    entries++;
                }
            }

            Directory.SetCurrentDirectory(TopLevelDir);

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip ({0} entries in {1} subdirs)...", entries, subdirCount);
            // add all the subdirectories into a new zip
            using (ZipFile zip1 = new ZipFile())
            {
                // add all of those subdirectories (A, B, C...) into the root in the zip archive
                zip1.AddDirectory(fodder, "");
                zip1.Save(zipFileToCreate);
            }
            Assert.AreEqual<Int32>(entries, TestUtilities.CountEntries(zipFileToCreate));


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Selecting entries by full path...");
            count1 = 0;
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                for (i = 0; i < subdirCount; i++)
                {
                    string dirInArchive = new System.String(new char[] { (char)(i + 65) });
                    var selected1 = zip1.SelectEntries(Path.Combine(dirInArchive, "*.*"));
                    count1 += selected1.Count;
                    TestContext.WriteLine("--------------\nfiles in dir {0} ({1}):",
                                          dirInArchive, selected1.Count);
                    foreach (ZipEntry e in selected1)
                        TestContext.WriteLine(e.FileName);
                }
                Assert.AreEqual<Int32>(entries, count1);
            }


            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Selecting entries by directory and size...");
            count1 = 0;
            count2 = 0;
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                for (i = 0; i < subdirCount; i++)
                {
                    string dirInArchive = new System.String(new char[] { (char)(i + 65) });
                    string pathCriterion = String.Format("name = {0}",
                                                         Path.Combine(dirInArchive, "*.*"));
                    string combinedCriterion = String.Format("size > 1500  AND {0}", pathCriterion);

                    var selected1 = zip1.SelectEntries(combinedCriterion, dirInArchive);
                    count1 += selected1.Count;
                    TestContext.WriteLine("--------------\nfiles in ({0}) ({1} entries):",
                                          combinedCriterion,
                                          selected1.Count);
                    foreach (ZipEntry e in selected1)
                        TestContext.WriteLine(e.FileName);
                }

                var selected2 = zip1.SelectEntries("size <= 1500");
                count2 = selected2.Count;
                Assert.AreEqual<Int32>(entries, count1 + count2 - subdirCount);
            }
        }




        [TestMethod]
        public void Selector_SelectEntries_NestedDirectories_wi8559()
        {
            //Directory.SetCurrentDirectory(TopLevelDir);
            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectFiles_NestedDirectories.zip");

            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Creating zip file...");

            int dirCount = _rnd.Next(4) + 3;
            using (var zip = new ZipFile())
            {
                for (int i = 0; i < dirCount; i++)
                {
                    String dir = new String((char)(65 + i), i + 1);
                    zip.AddEntry(Path.Combine(dir, "Readme.txt"), "This is the content for the Readme.txt in directory " + dir);
                    int subDirCount = _rnd.Next(3) + 2;
                    for (int j = 0; j < subDirCount; j++)
                    {
                        String subdir = Path.Combine(dir, new String((char)(90 - j), 3));
                        zip.AddEntry(Path.Combine(subdir, "Readme.txt"), "This is the content for the Readme.txt in directory " + subdir);
                    }
                }
                zip.Save(zipFileToCreate);
            }

            // this testmethod does not extract files, or verify checksums ...

            // just want to verify that selection of entries works in nested directories as
            // well as
            TestContext.WriteLine("====================================================");
            TestContext.WriteLine("Selecting entries by path...");
            using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
            {
                for (int i = 0; i < dirCount; i++)
                {
                    String dir = new String((char)(65 + i), i + 1);
                    var selected1 = zip1.SelectEntries("*.txt", dir);
                    Assert.AreEqual<Int32>(1, selected1.Count);

                    selected1 = zip1.SelectEntries("*.txt", dir + "/ZZZ");
                    var selected2 = zip1.SelectEntries("*.txt", dir + "\\ZZZ");
                    Assert.AreEqual<Int32>(selected1.Count, selected2.Count);

                    selected1 = zip1.SelectEntries("*.txt", dir + "/YYY");
                    selected2 = zip1.SelectEntries("*.txt", dir + "\\YYY");
                    Assert.AreEqual<Int32>(selected1.Count, selected2.Count);
                }
            }
        }




        [TestMethod]
        public void Selector_SelectFiles_DirName_wi8245()
        {
            // workitem 8245
            //Directory.SetCurrentDirectory(TopLevelDir);
            SetupFiles();
            var ff = new Ionic.FileSelector("*.*");
            var result = ff.SelectFiles(fodderDirectory);
            Assert.IsTrue(result.Count > 1);
        }


        [TestMethod]
        public void Selector_SelectFiles_DirName_wi8245_2()
        {
            // workitem 8245
            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_SelectFiles_DirName_wi8245_2.zip");
            //Directory.SetCurrentDirectory(TopLevelDir);
            SetupFiles();

            var fodderFiles = Directory.GetFiles(fodderDirectory, "*.*", SearchOption.AllDirectories);

            TestContext.WriteLine("===============================================");
            TestContext.WriteLine("AddSelectedFiles()");
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.AddSelectedFiles(fodderDirectory, null, "fodder", true);
                zip1.Save(zipFileToCreate);
            }

            Assert.AreEqual<Int32>(TestUtilities.CountEntries(zipFileToCreate), fodderFiles.Length,
                                   "The Zip file has the wrong number of entries.");
        }



        [TestMethod]
        public void Selector_SelectFiles_DirName_wi9176()
        {
            // workitem 9176
            //Directory.SetCurrentDirectory(TopLevelDir);

            _txrx= TestUtilities.StartProgressMonitor("SelectFiles-DirName",
                                                      "Select Files by DirName",
                                                      "workitem 9176");

            SetupFiles();

            var binFiles = Directory.GetFiles(fodderDirectory, "*.bin", SearchOption.AllDirectories);

            int[] eCount = new int[2];
            _txrx.Send("pb 0 max 2");
            for (int i = 0; i < 2; i++)
            {
                string zipFileToCreate = Path.Combine(TopLevelDir,
                                                      String.Format("Selector_SelectFiles_DirName_wi9176-{0}.zip", i));
                _txrx.Send("pb 1 max 4");
                _txrx.Send("pb 1 value 0");
                string d = fodderDirectory;
                if (i == 1) d += "\\";
                TestContext.WriteLine("===============================================");
                TestContext.WriteLine("AddSelectedFiles(cycle={0})", i);
                using (ZipFile zip1 = new ZipFile())
                {
                    zip1.AddSelectedFiles("name = *.bin", d, "", true);
                    _txrx.Send("pb 1 step");
                    zip1.Save(zipFileToCreate);
                }
                _txrx.Send("pb 1 step");

                Assert.AreEqual<Int32>(TestUtilities.CountEntries(zipFileToCreate), binFiles.Length,
                                       "The Zip file has the wrong number of entries.");

                _txrx.Send("pb 2 step");

                using (ZipFile zip1 = ZipFile.Read(zipFileToCreate))
                {
                    foreach (var e in zip1)
                    {
                        if (e.FileName.Contains("/")) eCount[i]++;
                    }
                }
                _txrx.Send("pb 1 step");

                if (i==1)
                    Assert.AreEqual<Int32>(eCount[0], eCount[1],
                                           "Inconsistent results when the directory includes a path.", i);

                _txrx.Send("pb 0 step");
            }
        }


        [TestMethod]
        public void Selector_SelectFiles_GoodSyntax01()
        {
            string[] criteria = {
                "type = D",
                "type = F",
                "attrs = HRS",
                "attrs = L",
                "name = *.txt  OR (size > 7800)",
                "name = *.harvey  OR  (size > 7800  and attributes = H)",
                "(name = *.harvey)  OR  (size > 7800  and attributes = H)",
                "(name = *.xls)  and (name != *.xls)  OR  (size > 7800  and attributes = H)",
                "(name = '*.xls')",
                "(name = Ionic.Zip.dll) or ((size > 1mb) and (name != *.zip))",
                "(name = Ionic.Zip.dll) or ((size > 1mb) and (name != *.zip)) or (name = Joe.txt)",
                "(name=Ionic.Zip.dll) or ((size>1mb) and (name!=*.zip)) or (name=Joe.txt)",
                "(name=Ionic.Zip.dll)or((size>1mb)and(name!=*.zip))or(name=Joe.txt)",
            };

            foreach (string s in criteria)
            {
                TestContext.WriteLine("Selector: " + s);
                var ff = new Ionic.FileSelector(s);
            }
        }


        [TestMethod]
        public void Selector_Twiddle_wi10153()
        {
            // workitem 10153:
            //
            // When calling AddSelectedFiles(String,String,String,bool), and when the
            // actual filesystem path uses mixed case, but the specified directoryOnDisk
            // argument is downcased, AND when the filename contains a ~ (weird, I
            // know), verify that the path replacement works as advertised, and entries
            // are rooted in the directoryInArchive specified path.

            string zipFileToCreate = Path.Combine(TopLevelDir, "Selector_Twiddle.zip");
            string dirToZip = "dirToZip";
            var keyword = "Gamma";
            var files = TestUtilities.GenerateFilesFlat(dirToZip);
            int k = 0;
            Directory.SetCurrentDirectory(dirToZip);
            Array.ForEach(files, x => {
                    File.Move(Path.GetFileName(x),
                              String.Format("~{0}.{1:D5}.txt", keyword, k++));
                });
            Directory.SetCurrentDirectory(TopLevelDir);

            using (ZipFile zip = new ZipFile())
            {
                // must use ToLower to force case mismatch
                zip.AddSelectedFiles("name != *.zip*", dirToZip.ToLower(), "", true);
                zip.Save(zipFileToCreate);
            }

            int nEntries = 0;
            using (ZipFile zip = ZipFile.Read(zipFileToCreate))
            {
                foreach (var e in zip)
                    TestContext.WriteLine("entry {0}", e.FileName);

                TestContext.WriteLine("");

                foreach (var e in zip)
                {
                    TestContext.WriteLine("check {0}", e.FileName);
                    Assert.IsFalse(e.FileName.Contains("/"),
                                   "The filename contains a path, but shouldn't");
                    nEntries++;
                }
            }
            Assert.IsTrue(nEntries>2, "Not enough entries");
        }



        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadNoun()
        {
            new Ionic.FileSelector("fame = *.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax01()
        {
            new Ionic.FileSelector("size = ");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax02()
        {
            new Ionic.FileSelector("name = *.txt and");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax03()
        {
            new Ionic.FileSelector("name = *.txt  URF ");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax04()
        {
            new Ionic.FileSelector("name = *.txt  OR (");
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void Selector_SelectFiles_BadSyntax05()
        {
            new Ionic.FileSelector("name = *.txt  OR (size = G)");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax06()
        {
            new Ionic.FileSelector("name = *.txt  OR (size > )");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax07()
        {
            new Ionic.FileSelector("name = *.txt  OR (size > 7800");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax08()
        {
            new Ionic.FileSelector("name = *.txt  OR )size > 7800");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax09()
        {
            new Ionic.FileSelector("name = *.txt and  name =");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax10()
        {
            new Ionic.FileSelector("name == *.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax10a()
        {
            new Ionic.FileSelector("name >= *.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax11()
        {
            new Ionic.FileSelector("name ~= *.txt");
        }
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax12()
        {
            new Ionic.FileSelector("name @ = *.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax13()
        {
            new Ionic.FileSelector("name LIKE  *.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax14()
        {
            new Ionic.FileSelector("name AND  *.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax15()
        {
            new Ionic.FileSelector("name (AND  *.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax16()
        {
            new Ionic.FileSelector("mtime 2007-01-01");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax17()
        {
            new Ionic.FileSelector("size 1kb");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax18()
        {
            Ionic.FileSelector ff = new Ionic.FileSelector("");
            var list = ff.SelectFiles(".");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax19()
        {
            Ionic.FileSelector ff = new Ionic.FileSelector(null);
            var list = ff.SelectFiles(".");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax20()
        {
            new Ionic.FileSelector("attributes > HRTS");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax21()
        {
            new Ionic.FileSelector("attributes HRTS");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax22a()
        {
            new Ionic.FileSelector("attributes = HHHA");
        }
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax22b()
        {
            new Ionic.FileSelector("attributes = SHSA");
        }
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax22c()
        {
            new Ionic.FileSelector("attributes = AHA");
        }
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax22d()
        {
            new Ionic.FileSelector("attributes = RRA");
        }
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax22e()
        {
            new Ionic.FileSelector("attributes = IRIA");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax23()
        {
            new Ionic.FileSelector("attributes = INVALID");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax24a()
        {
            new Ionic.FileSelector("type = I");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Selector_SelectFiles_BadSyntax24b()
        {
            new Ionic.FileSelector("type > D");
        }

        [TestMethod]
        public void Selector_Normalize()
        {

            string[][] sPairs = {
                new string[] {
                    "name = '.\\Selector (this is a Test)\\this.txt'",
                    null},

                new string[] {
                    "(size > 100)AND(name='Name (with Parens).txt')",
                    "(size > 100 AND name = 'Name (with Parens).txt')"},

                new string[] {
                    "(size > 100) AND ((name='Name (with Parens).txt')OR(name=*.jpg))",
                    "(size > 100 AND (name = 'Name (with Parens).txt' OR name = '*.jpg'))"},

                new string[] {
                    "name='*.txt' and name!='* *.txt'",
                    "(name = '*.txt' AND name != '* *.txt')"},

                new string[] {
                    "name = *.txt AND name != '* *.txt'",
                    "(name = '*.txt' AND name != '* *.txt')"},
            };


            for (int i=0; i < sPairs.Length; i++)
            {
                var pair = sPairs[i];
                var selector = pair[0];
                var expectedResult = pair[1];
                var fsel = new FileSelector(selector);
                var stringVer = fsel.ToString().Replace("\u00006"," ");
                Assert.AreEqual<string>("FileSelector("+ (expectedResult ?? selector)
                                        +")",
                                        stringVer,
                                        "entry {0}", i);
            }
        }


        [TestMethod]
        public void Selector_SingleQuotesAndSlashes_wi14033()
        {
            var zipFileToCreate = "SingleQuotes.zip";
            var parentDir = "DexMik";

            int nFolders = this._rnd.Next(4)+3;
            TestContext.WriteLine("Creating {0} folders:", nFolders);
            Directory.CreateDirectory(parentDir);
            string[] childFolders = new string[nFolders+1];
            childFolders[0] = parentDir;
            for (int i=0; i < nFolders; i++)
            {
                var b1 = "folder" + (i+1);
                int k = (i > 0) ? this._rnd.Next(i+1) : 0;
                var d1 = Path.Combine(childFolders[k], b1);
                TestContext.WriteLine("  {0}", d1);
                Directory.CreateDirectory(d1);
                childFolders[i+1] = d1;

                int nFiles = this._rnd.Next(3)+2;
                TestContext.WriteLine("  Creating {0} files:", nFiles);
                for (int j=0; j < nFiles; j++)
                {
                    var fn1 = Path.GetRandomFileName();
                    var fname = Path.Combine(d1,fn1);
                    TestContext.WriteLine("    {0}", fn1);
                    TestUtilities.CreateAndFillFileText(fname, this._rnd.Next(10000) + 1000);
                }
                TestContext.WriteLine("");
            }

            // create a zip file using those files
            TestContext.WriteLine("");
            TestContext.WriteLine("Zipping:");
            using (var zip = new ZipFile())
            {
                zip.AddDirectory(parentDir, childFolders[0]);
                zip.Save(zipFileToCreate);
            }

            // list all the entries
            TestContext.WriteLine("");
            TestContext.WriteLine("List of entries:");
            using (var zip = new ZipFile(zipFileToCreate))
            {
                foreach (var e in zip)
                {
                    TestContext.WriteLine("  {0}", e.FileName);
                }
            }
            TestContext.WriteLine("");

            // now select some of the entries
            int m = this._rnd.Next(nFolders)+1;
            TestContext.WriteLine("");
            TestContext.WriteLine("Selecting entries from folder {0}:", m);
            using (var zip = new ZipFile(zipFileToCreate))
            {
                string selectCriteria =
                    String.Format("name = '{0}'",
                                  Path.Combine(childFolders[m], "*.*"));
                TestContext.WriteLine("select:  {0}", selectCriteria);
                var selection1 = zip.SelectEntries(selectCriteria);
                Assert.IsTrue(selection1.Count > 0, "first selection failed.");

                foreach (var item in selection1)
                {
                    TestContext.WriteLine("  {0}", item);
                }

                // Try different formats of the selection string - with
                // and without quotes, with fwd slashes and back
                // slashes.
                string[][] replacementPairs = {
                    new string[] { "\\", "/" }, // backslash to fwdslash
                    new string[] { "'", "" },   // remove single quotes
                    new string[] { "/", "\\" }, // fwdslash to backslash
                };

                for (int k=0; k < 3; k++)
                {
                    selectCriteria = selectCriteria.Replace(replacementPairs[k][0],
                                                            replacementPairs[k][1]);

                    TestContext.WriteLine("");
                    TestContext.WriteLine("Try #{0}: {1}", k+2, selectCriteria);
                    var selection2 = zip.SelectEntries(selectCriteria);
                    foreach (var item in selection2)
                    {
                        TestContext.WriteLine("  {0}", item);
                    }

                    Assert.AreEqual<int>(selection1.Count,
                                         selection2.Count,
                                         "selection verification trial {0} failed.", k);
                }
            }

        }

    }
}
