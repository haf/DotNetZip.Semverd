// Zip64Tests.cs
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
// Time-stamp: <2011-July-10 20:31:24>
//
// ------------------------------------------------------------------
//
// This module defines the tests for the ZIP64 capability within DotNetZip.  These
// tests can take a long time to run, as the files can be quite large - 10g or
// more. Merely generating the content for these tests can take an hour.  Most tests
// in the DotNetZip test suite are self-standing - they generate the content they
// need, and then remove it after completion, either success or failure. With ZIP64,
// because content creation is expensive, for update operations this module uses a
// cache of large zip files.  See _CreateHugeZipfiles().  The method looks for
// large files in a well-known location on the filesystem, in fodderDir.
//
// ------------------------------------------------------------------


using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;


namespace Ionic.Zip.Tests.Zip64
{
    [TestClass]
    public class Zip64Tests : IonicTestClass
    {
        string fodderDir = "c:\\users\\dino\\Downloads";
        string homeDir = System.Environment.GetEnvironmentVariable("TEMP");

        public Zip64Tests() : base() { }

        private static string[] _HugeZipFiles;
        private string[] GetHugeZipFiles()
        {
            if (_HugeZipFiles == null)
            {
                _HugeZipFiles = _CreateHugeZipfiles();
            }
            return _HugeZipFiles;
        }



        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            if (_HugeZipFiles != null)
            {
                // Keep this huge zip file around, because it takes so much
                // time to create it. But Delete the directory if one of the files no
                // longer exists.
                if (!File.Exists(_HugeZipFiles[0]) ||
                    !File.Exists(_HugeZipFiles[1]))
                {
                    //File.Delete(_HugeZipFile);
                    string d= Path.GetDirectoryName(_HugeZipFiles[0]);
                    if (Directory.Exists(d))
                        Directory.Delete(d, true);
                }
            }
        }

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

        Zip64Option[] z64 =
            {
                Zip64Option.Never,
                Zip64Option.AsNecessary,
                Zip64Option.Always,
            };


        private Object LOCK = new Object();


        /// <summary>
        ///   Create 2 large zip64 zip files - one from DNZ, one from WinZip.  Each
        ///   test that updates a zip file should use both. There are slight
        ///   differences between a zip from DNZ and one from WinZip, specifically
        ///   regarding metadata. These differences should be inconsequential for
        ///   updates of zips, and that is what some of the zip64 tests seek to
        ///   verify.
        /// </summary>
        private string[] _CreateHugeZipfiles()
        {
            string[] zipsToCreate = { "Zip64Test-createdBy-WZ.zip",
                                      "Zip64Test-createdBy-DNZ.zip" };

            // lock in case more than one test calls this at a time.
            lock (LOCK)
            {
                TestContext.WriteLine("CreateHugeZipFiles");
                TestContext.WriteLine("Start - " + DateTime.Now.ToString("G"));
                // STEP 1:
                // look for existing directories, and re-use the large zip files
                // there, if it exists, and if it is large enough.
                string testDir = null;
                var filesToAdd = new List<string>();
                var oldDirs = Directory.GetDirectories(homeDir, "*.Zip64Tests");
                string zipFileToCreate = null;
                List<int> found = null;

                // first pass to check if any dirs, have both files,
                // second pass to check if any dirs have one file plus fodder files.
                for (int pass=0; pass < 2; pass++)
                {
                    foreach (var dir in oldDirs)
                    {
                        found = new List<int>();
                        for (int m=0; m < zipsToCreate.Length; m++)
                        {
                            zipFileToCreate = Path.Combine(dir, zipsToCreate[m]);
                            if (File.Exists(zipFileToCreate))
                            {
                                TestContext.WriteLine("File exists: {0}", zipFileToCreate);
                                FileInfo fi = new FileInfo(zipFileToCreate);
                                if (fi.Length < (long)System.UInt32.MaxValue)
                                {
                                    TestContext.WriteLine("deleting file (too small): {0}", zipFileToCreate);
                                    File.Delete(zipFileToCreate);
                                }
                                else found.Add(m);
                            }
                        }

                        int fc = found.Count();
                        switch (fc)
                        {
                            case 0:
                            case 1:
                                // check for fodder files
                                testDir = dir;
                                string fdir = Path.Combine(dir,"dir");
                                if (Directory.Exists(fdir))
                                {
                                    var fodderFiles = Directory.GetFiles(fdir, "*.txt");
                                    if (fodderFiles == null || fodderFiles.Length <= 6)
                                        try { Directory.Delete(dir, true); } catch { }
                                }
                                else try { Directory.Delete(dir, true); } catch { }
                                break;
                            case 2:
                                // found both large zips, so use them.
                                zipsToCreate[0] = Path.Combine(dir, zipsToCreate[0]);
                                zipsToCreate[1] = Path.Combine(dir, zipsToCreate[1]);
                                TestContext.WriteLine("Using the existing zips in: {0}", dir);
                                return zipsToCreate;
                        }

                        if (pass == 1 && Directory.Exists(dir) && fc==1)
                        {
                            // on pass 2, take 1st dir with at least one zip
                            break;
                        }
                    }
                }

                // remember the current directory so we can restore later
                string originalDir = Directory.GetCurrentDirectory();
                // CurrentDir is the dir that holds the test temp directory (or directorIES)
                Directory.SetCurrentDirectory(CurrentDir);

                if (!Directory.Exists(testDir))
                {
                    // create the dir if it does not exist
                    string pname = Path.GetFileName(TestUtilities.GenerateUniquePathname("Zip64Tests"));
                    testDir = Path.Combine(homeDir, pname);
                    Directory.CreateDirectory(testDir);
                    Directory.SetCurrentDirectory(testDir);
                }
                else
                {
                    Directory.SetCurrentDirectory(testDir);
                    string fdir = Path.Combine(testDir,"dir");
                    filesToAdd.AddRange(Directory.GetFiles(fdir, "*.txt"));
                }

                TestContext.WriteLine("Creating new zip files...");

                // create a huge ZIP64 archive with a true 64-bit offset.
                _txrx = TestUtilities.StartProgressMonitor("Zip64_Setup",
                                                           "Zip64 Test Setup",
                                                           "Creating files");

                //Directory.SetCurrentDirectory(testDir);

                // create a directory with some files in it, to zip
                string dirToZip = "dir";
                Directory.CreateDirectory(dirToZip);
                _txrx.Send("pb 0 step");
                System.Threading.Thread.Sleep(120);
                _txrx.Send("bars 3");
                System.Threading.Thread.Sleep(220);
                _txrx.Send("pb 0 max 4");
                System.Threading.Thread.Sleep(220);
                int numFilesToAdd = _rnd.Next(4) + 7;
                _txrx.Send("pb 1 max " + numFilesToAdd);

                // These params define the size range for the large, random text
                // files that are created below. Creating files this size takes
                // about 1 minute per file
                 _sizeBase =   0x16000000;
                 _sizeRandom = 0x20000000;
                //_sizeBase =   0x160000;
                //_sizeRandom = 0x200000;
                if (filesToAdd.Count() == 0)
                {
                    int n;
                    var buf = new byte[2048];
                    for (int i = 0; i < numFilesToAdd; i++)
                    {
                        System.Threading.Thread.Sleep(220);
                        _txrx.Send("title Zip64 Create Huge Zip files"); // in case it was missed
                        System.Threading.Thread.Sleep(220);
                        int fnameLength = _rnd.Next(25) + 6;
                        string filename = TestUtilities.GenerateRandomName(fnameLength) +
                            ".txt";
                        _txrx.Send(String.Format("status create {0} ({1}/{2})", filename, i+1, numFilesToAdd));
                        int totalSize = _sizeBase + _rnd.Next(_sizeRandom);
                        System.Threading.Thread.Sleep(220);
                        _txrx.Send(String.Format("pb 2 max {0}", totalSize));
                        System.Threading.Thread.Sleep(220);
                        _txrx.Send("pb 2 value 0");
                        int writtenSoFar = 0;
                        int cycles = 0;
                        using (var input = new Ionic.Zip.Tests.Utilities.RandomTextInputStream(totalSize))
                        {
                            using (var output = File.Create(Path.Combine(dirToZip,filename)))
                            {
                                while ((n = input.Read(buf,0,buf.Length)) > 0)
                                {
                                    output.Write(buf,0,n);
                                    writtenSoFar+=n;
                                    cycles++;
                                    if (cycles % 640 == 0)
                                    {
                                        _txrx.Send(String.Format("pb 2 value {0}", writtenSoFar));
                                    }
                                }
                            }
                        }
                        filesToAdd.Add(filename);
                        _txrx.Send("pb 1 step");
                    }
                }

                _txrx.Send("pb 0 step");
                System.Threading.Thread.Sleep(220);
                _txrx.Send("pb 1 value 0");

                // Add links to a few very large files into the same directory.  We
                // do this because creating such large files from nothing would take
                // a very very long time.
                if (CreateLinksToLargeFiles(dirToZip))
                    return null;

                Directory.SetCurrentDirectory(testDir); // again

                if (found == null || !found.Contains(0))
                {
                    // create a zip file, using WinZip
                    // This will take 50 minutes or so, no progress updates possible.
                    System.Threading.Thread.Sleep(220);
                    _txrx.Send("title Zip64 Create Huge Zip files"); // in case it was missed
                    System.Threading.Thread.Sleep(220);
                    _txrx.Send("pb 2 value 0");
                    zipFileToCreate = zipsToCreate[0];
                    zipsToCreate[0] = Path.Combine(testDir, zipsToCreate[0]);

                    // wzzip.exe will create a zip64 file automatically, as necessary.
                    // There is no explicit switch required.
                    // switches:
                    //   -a   add
                    //   -r   recurse
                    //   -p   store folder names
                    //   -yx  store extended timestamps
                    var sb1 = new System.Text.StringBuilder();
                    sb1.Append("-a -p -r -yx \"")
                        .Append(zipFileToCreate)
                        .Append("\" \"")
                        .Append(dirToZip)
                        .Append("\" ");

                    string args = sb1.ToString();
                    System.Threading.Thread.Sleep(220);
                    _txrx.Send("status wzzip.exe " + args);
                    TestContext.WriteLine("Exec: wzzip {0}", args);
                    string wzzipOut = this.Exec(wzzip, args);
                    TestContext.WriteLine("Done with wzzip.exe");
                    _txrx.Send("status wzzip.exe: Done");
                }

                if (found == null || !found.Contains(1))
                {
                    // Create a zip file using DotNetZip
                    // This will take 50 minutes or so.
                    // pb1 and pb2 will be set in the {Add,Save}Progress handlers
                    _txrx.Send("pb 0 step");
                    System.Threading.Thread.Sleep(120);
                    _txrx.Send("status Saving the zip...");
                    System.Threading.Thread.Sleep(120);
                    _txrx.Send(String.Format("pb 1 max {0}", numFilesToAdd + Directory.GetFiles(dirToZip).Length));
                    _testTitle = "Zip64 Create Huge Zip files"; // used in Zip64_SaveProgress
                    _pb1Set = false;
                    zipFileToCreate = Path.Combine(testDir, zipsToCreate[1]);
                    zipsToCreate[1] = zipFileToCreate;
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.SaveProgress += Zip64SaveProgress;
                        zip.AddProgress += Zip64AddProgress;
                        zip.UpdateDirectory(dirToZip, "");
                        foreach (var e in zip)
                        {
                            if (e.FileName.EndsWith(".pst") ||
                                e.FileName.EndsWith(".ost") ||
                                e.FileName.EndsWith(".zip"))
                                e.CompressionMethod = CompressionMethod.None;
                        }

                        zip.UseZip64WhenSaving = Zip64Option.Always;
                        // use large buffer to speed up save for large files:
                        zip.BufferSize = 1024 * 756;
                        zip.CodecBufferSize = 1024 * 128;
                        zip.Save(zipFileToCreate);
                    }
                }

                _txrx.Send("pb 0 step");
                System.Threading.Thread.Sleep(120);

                // Delete the fodder dir only if we have both zips.
                // This is helpful when modifying or editing this method.
                // With repeated runs you don't have to re-create all the data
                // each time.
                if (File.Exists(zipsToCreate[0]) && File.Exists(zipsToCreate[1]))
                    Directory.Delete(dirToZip, true);

                _txrx.Send("pb 0 step");
                System.Threading.Thread.Sleep(120);

                _txrx.Send("stop");

                // restore the cwd:
                Directory.SetCurrentDirectory(originalDir);

                TestContext.WriteLine("All done - " +
                                  DateTime.Now.ToString("G"));

                return zipsToCreate;
            }
        }



        private bool CreateLinksToLargeFiles(string dirForLinks)
        {
            var candidates = from fi in (from fn in Directory.GetFiles(fodderDir)
                                         select new FileInfo(fn))
                where fi.Length > 0x10000000
                orderby fi.Length descending
                select fi;

            if (candidates.Count() < 3)
            {
                TestContext.WriteLine("Found {0} files, not enough to proceed.",
                                      candidates.Count());
                return true;
            }
            TestContext.WriteLine("Found {0} large files, which seems enough to proceed.",
                                  candidates.Count());

            string current = Directory.GetCurrentDirectory();
            _txrx.Send("status Creating links");
            string subdir = Path.Combine(dirForLinks, "00-largelinks");
            if (Directory.Exists(subdir))
                Directory.Delete(subdir, true);
            Directory.CreateDirectory(subdir);
            Directory.SetCurrentDirectory(subdir);
            var w = System.Environment.GetEnvironmentVariable("Windir");
            Assert.IsTrue(Directory.Exists(w), "%windir% does not exist ({0})", w);
            var fsutil = Path.Combine(Path.Combine(w, "system32"), "fsutil.exe");
            Assert.IsTrue(File.Exists(fsutil), "fsutil.exe does not exist ({0})", fsutil);
            string ignored;
            Int64 totalLength = 0;
            int cycle = 0;
            const Int64 threshold = (long)(11L * 1024 * 1024 * 1024);
            while (totalLength < threshold)
            {
                cycle++;
                foreach (var fi in candidates)
                {
                    string cmd = String.Format("hardlink create \"{0}-Copy{1}{2}\" \"{3}\"",
                                               Path.GetFileNameWithoutExtension(fi.Name),
                                               cycle,
                                               Path.GetExtension(fi.Name),
                                               Path.Combine(fodderDir,fi.Name));
                    TestContext.WriteLine("Command: fsutil {0}", cmd);
                    _txrx.Send("status " + cmd);
                    TestUtilities.Exec_NoContext(fsutil, cmd, out ignored);
                    totalLength += fi.Length;
                    if (totalLength > threshold)
                        break; // enough
                }
            }
            Directory.SetCurrentDirectory(current);
            return false;
        }




        [TestMethod]
        public void Zip64_Create()
        {
            Zip64Option[] Options = { Zip64Option.Always,
                                      Zip64Option.Never,
                                      Zip64Option.AsNecessary };
            for (int k = 0; k < Options.Length; k++)
            {
                string filename = null;
                Directory.SetCurrentDirectory(TopLevelDir);
                TestContext.WriteLine("\n\n==================Trial {0}...", k);
                string zipFileToCreate = String.Format("Zip64_Create-{0}.zip", k);

                TestContext.WriteLine("Creating file {0}", zipFileToCreate);
                TestContext.WriteLine("  ZIP64 option: {0}", Options[k].ToString());
                int entries = _rnd.Next(5) + 13;
                var checksums = new Dictionary<string, string>();
                using (ZipFile zip1 = new ZipFile())
                {
                    for (int i = 0; i < entries; i++)
                    {
                        if (_rnd.Next(2) == 1)
                        {
                            filename = Path.Combine(TopLevelDir, String.Format("Data{0}.bin", i));
                            int filesize = _rnd.Next(44000) + 5000;
                            TestUtilities.CreateAndFillFileBinary(filename, filesize);
                        }
                        else
                        {
                            filename = Path.Combine(TopLevelDir, String.Format("Data{0}.txt", i));
                            int filesize = _rnd.Next(44000) + 5000;
                            TestUtilities.CreateAndFillFileText(filename, filesize);
                        }
                        zip1.AddFile(filename, "");

                        var chk = TestUtilities.ComputeChecksum(filename);
                        checksums.Add(Path.GetFileName(filename), TestUtilities.CheckSumToString(chk));
                    }

                    zip1.UseZip64WhenSaving = Options[k];
                    zip1.Comment = String.Format("This archive uses zip64 option: {0}", Options[k].ToString());
                    zip1.Save(zipFileToCreate);

                    if (Options[k] == Zip64Option.Always)
                        Assert.IsTrue(zip1.OutputUsedZip64.Value);
                    else if (Options[k] == Zip64Option.Never)
                        Assert.IsFalse(zip1.OutputUsedZip64.Value);
                }

                BasicVerifyZip(zipFileToCreate);

                TestContext.WriteLine("---------------Reading {0}...", zipFileToCreate);
                using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                {
                    string extractDir = String.Format("extract{0}", k);
                    foreach (var e in zip2)
                    {
                        TestContext.WriteLine(" Entry: {0}  c({1})  unc({2})", e.FileName, e.CompressedSize, e.UncompressedSize);

                        e.Extract(extractDir);
                        filename = Path.Combine(extractDir, e.FileName);
                        string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(filename));
                        Assert.IsTrue(checksums.ContainsKey(e.FileName), "Checksum is missing");
                        Assert.AreEqual<string>(checksums[e.FileName], actualCheckString, "Checksums for ({0}) do not match.", e.FileName);
                        TestContext.WriteLine("     Checksums match ({0}).\n", actualCheckString);
                    }
                }
            }
        }



        [TestMethod]
        public void Zip64_Convert()
        {
            string trialDescription = "Trial {0}/{1}:  create archive as 'zip64={2}', then open it and re-save with 'zip64={3}'";
            Zip64Option[] z64a = {
                Zip64Option.Never,
                Zip64Option.Always,
                Zip64Option.AsNecessary};

            // ??
            for (int u = 0; u < 2; u++)
            {
                for (int m = 0; m < z64a.Length; m++)
                {
                    for (int n = 0; n < z64a.Length; n++)
                    {
                        int k = m * z64a.Length + n;

                        string filename = null;
                        Directory.SetCurrentDirectory(TopLevelDir);
                        TestContext.WriteLine("\n\n==================Trial {0}...", k);

                        TestContext.WriteLine(trialDescription, k, (z64a.Length * z64a.Length) - 1, z64a[m], z64a[n]);

                        string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("Zip64_Convert-{0}.A.zip", k));

                        int entries = _rnd.Next(8) + 6;
                        //int entries = 2;
                        TestContext.WriteLine("Creating file {0}, zip64={1}, {2} entries",
                                              Path.GetFileName(zipFileToCreate), z64a[m].ToString(), entries);

                        var checksums = new Dictionary<string, string>();
                        using (ZipFile zip1 = new ZipFile())
                        {
                            for (int i = 0; i < entries; i++)
                            {
                                if (_rnd.Next(2) == 1)
                                {
                                    filename = Path.Combine(TopLevelDir, String.Format("Data{0}.bin", i));
                                    TestUtilities.CreateAndFillFileBinary(filename, _rnd.Next(44000) + 5000);
                                }
                                else
                                {
                                    filename = Path.Combine(TopLevelDir, String.Format("Data{0}.txt", i));
                                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(44000) + 5000);
                                }
                                zip1.AddFile(filename, "");

                                var chk = TestUtilities.ComputeChecksum(filename);
                                checksums.Add(Path.GetFileName(filename), TestUtilities.CheckSumToString(chk));
                            }

                            TestContext.WriteLine("---------------Saving to {0} with Zip64={1}...",
                                                  Path.GetFileName(zipFileToCreate), z64a[m].ToString());
                            zip1.UseZip64WhenSaving = z64a[m];
                            zip1.Comment = String.Format("This archive uses Zip64Option={0}", z64a[m].ToString());
                            zip1.Save(zipFileToCreate);
                        }


                        Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entries,
                                             "The Zip file has the wrong number of entries.");


                        string newFile = zipFileToCreate.Replace(".A.", ".B.");
                        using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                        {
                            TestContext.WriteLine("---------------Extracting {0} ...",
                                                  Path.GetFileName(zipFileToCreate));
                            string extractDir = String.Format("extract-{0}-{1}.A", k, u);
                            foreach (var e in zip2)
                            {
                                TestContext.WriteLine(" {0}  crc({1:X8})  c({2:X8}) unc({3:X8})", e.FileName, e.Crc, e.CompressedSize, e.UncompressedSize);

                                e.Extract(extractDir);
                                filename = Path.Combine(extractDir, e.FileName);
                                string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(filename));
                                Assert.IsTrue(checksums.ContainsKey(e.FileName), "Checksum is missing");
                                Assert.AreEqual<string>(checksums[e.FileName], actualCheckString, "Checksums for ({0}) do not match.", e.FileName);
                            }

                            if (u==1)
                            {
                                TestContext.WriteLine("---------------Updating:  Renaming an entry...");
                                zip2[4].FileName += ".renamed";

                                string entriesToRemove = (_rnd.Next(2) == 0) ? "*.txt" : "*.bin";
                                TestContext.WriteLine("---------------Updating:  Removing {0} entries...", entriesToRemove);
                                zip2.RemoveSelectedEntries(entriesToRemove);
                            }

                            TestContext.WriteLine("---------------Saving to {0} with Zip64={1}...",
                                                  Path.GetFileName(newFile), z64a[n].ToString());

                            zip2.UseZip64WhenSaving = z64a[n];
                            zip2.Comment = String.Format("This archive uses Zip64Option={0}", z64a[n].ToString());
                            zip2.Save(newFile);
                        }



                        using (ZipFile zip3 = ZipFile.Read(newFile))
                        {
                            TestContext.WriteLine("---------------Extracting {0} ...",
                                                  Path.GetFileName(newFile));
                            string extractDir = String.Format("extract-{0}-{1}.B", k, u);
                            foreach (var e in zip3)
                            {
                                TestContext.WriteLine(" {0}  crc({1:X8})  c({2:X8}) unc({3:X8})", e.FileName, e.Crc, e.CompressedSize, e.UncompressedSize);

                                e.Extract(extractDir);
                                filename = Path.Combine(extractDir, e.FileName);
                                string actualCheckString = TestUtilities.CheckSumToString(TestUtilities.ComputeChecksum(filename));
                                if (!e.FileName.EndsWith(".renamed"))
                                {
                                    Assert.IsTrue(checksums.ContainsKey(e.FileName), "Checksum is missing");
                                    Assert.AreEqual<string>(checksums[e.FileName], actualCheckString, "Checksums for ({0}) do not match.", e.FileName);
                                }
                            }
                        }
                    }
                }
            }
        }


        bool _pb2Set;
        bool _pb1Set;
        string _testTitle;
        int _sizeBase;
        int _sizeRandom;
        int _numSaving;
        int _totalToSave;
        int _spCycles;
        private void Zip64SaveProgress(object sender, SaveProgressEventArgs e)
        {
            string msg;
            switch (e.EventType)
            {
                case ZipProgressEventType.Saving_Started:
                    _txrx.Send("status saving started...");
                    _pb1Set = false;
                    _numSaving= 1;
                    break;

                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    _txrx.Send(String.Format("status Compressing {0}", e.CurrentEntry.FileName));
                    _spCycles = 0;
                    if (!_pb1Set)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", e.EntriesTotal));
                        _pb1Set = true;
                    }
                    _totalToSave = e.EntriesTotal;
                    _pb2Set = false;
                    break;

                case ZipProgressEventType.Saving_EntryBytesRead:
                    _spCycles++;
                    if ((_spCycles % 128) == 0)
                    {
                        if (!_pb2Set)
                        {
                            _txrx.Send(String.Format("pb 2 max {0}", e.TotalBytesToTransfer));
                            _pb2Set = true;
                        }
                        _txrx.Send(String.Format("status Saving entry {0}/{1} :: {2} :: {3}/{4}mb {5:N0}%",
                                                 _numSaving, _totalToSave,
                                                 e.CurrentEntry.FileName,
                                                 e.BytesTransferred/(1024*1024), e.TotalBytesToTransfer/(1024*1024),
                                                 ((double)e.BytesTransferred) / (0.01 * e.TotalBytesToTransfer)));
                        msg = String.Format("pb 2 value {0}", e.BytesTransferred);
                        _txrx.Send(msg);
                    }
                    break;

                case ZipProgressEventType.Saving_AfterWriteEntry:
                    _txrx.Send("test " +  _testTitle); // just in case it was missed
                    _txrx.Send("pb 1 step");
                    _numSaving++;
                    break;

                case ZipProgressEventType.Saving_Completed:
                    _txrx.Send("status Save completed");
                    _pb1Set = false;
                    _pb2Set = false;
                    _txrx.Send("pb 1 max 1");
                    _txrx.Send("pb 1 value 1");
                    break;
            }
        }



        void Zip64AddProgress(object sender, AddProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Adding_Started:
                    _txrx.Send("status Adding files to the zip...");
                    break;
                case ZipProgressEventType.Adding_AfterAddEntry:
                    _txrx.Send(String.Format("status Adding file {0}",
                                             e.CurrentEntry.FileName));
                    break;
                case ZipProgressEventType.Adding_Completed:
                    _txrx.Send("status Added all files");
                    break;
            }
        }



        private int _numExtracted;
        private int _epCycles;
        private int _numFilesToExtract;
        void Zip64ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Extracting_BeforeExtractEntry:
                    if (!_pb1Set)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", _numFilesToExtract));
                        _pb1Set = true;
                    }
                    _pb2Set = false;
                    _epCycles = 0;
                    break;

                case ZipProgressEventType.Extracting_EntryBytesWritten:
                    _epCycles++;
                    if ((_epCycles % 512) == 0)
                    {
                        if (!_pb2Set)
                        {
                            _txrx.Send(String.Format("pb 2 max {0}", e.TotalBytesToTransfer));
                            _pb2Set = true;
                        }
                        _txrx.Send(String.Format("status {0} entry {1}/{2} :: {3} :: {4}/{5}mb ::  {6:N0}%",
                                                 verb,
                                                 _numExtracted, _numFilesToExtract,
                                                 e.CurrentEntry.FileName,
                                                 e.BytesTransferred/(1024*1024),
                                                 e.TotalBytesToTransfer/(1024*1024),
                                                 ((double)e.BytesTransferred) / (0.01 * e.TotalBytesToTransfer)
                                                 ));
                        string msg = String.Format("pb 2 value {0}", e.BytesTransferred);
                        _txrx.Send(msg);
                    }
                    break;

                case ZipProgressEventType.Extracting_AfterExtractEntry:
                    _numExtracted++;
                    if (_numFilesToExtract < 1024 || (_numExtracted % 128) == 0)
                    {
                        _txrx.Send("test " +  _testTitle); // just in case it was missed
                        while (_numExtracted > _numFilesToExtract) _numExtracted--;
                        _txrx.Send("pb 1 value " + _numExtracted);
                        if (_numExtracted == _numFilesToExtract)
                        {
                            _txrx.Send("status All done " + verb);
                        }
                        else
                        {
                            _txrx.Send(String.Format("status {0} entry {1}/{2} {3:N0}%",
                                                     verb,
                                                     _numExtracted, _numFilesToExtract,
                                                     _numExtracted / (0.01 *_numFilesToExtract)));
                        }
                    }
                    break;
            }
        }



        string verb;

        private void Zip64VerifyZip(string zipfile)
        {
            _pb1Set = false;
            Stream bitBucket = Stream.Null;
            TestContext.WriteLine("");
            TestContext.WriteLine("Checking file {0}", zipfile);
            verb = "Verifying";
            using (ZipFile zip = ZipFile.Read(zipfile))
            {
                // large buffer better for large files
                zip.BufferSize = 65536*4; // 65536 * 8 = 512k
                _numFilesToExtract = zip.Entries.Count;
                _numExtracted= 1;
                zip.ExtractProgress += Zip64ExtractProgress;
                foreach (var s in zip.EntryFileNames)
                {
                    TestContext.WriteLine("  Entry: {0}", s);
                    zip[s].Extract(bitBucket);
                }
            }
            System.Threading.Thread.Sleep(0x500);
        }



        [Timeout(3 * 60*60*1000), TestMethod] // 60*60*1000 = 1hr
        public void Zip64_Update_WZ()
        {
            // this should take about an hour
            string[] zipFilesToUpdate = GetHugeZipFiles();
            Assert.IsFalse(zipFilesToUpdate == null, "No files were created.");
            // this should take a little less than an hour
            Zip64UpdateAddFiles(zipFilesToUpdate[0], "WinZip");
        }


        [Timeout(3 * 60*60*1000), TestMethod] // 60*60*1000 = 1hr
        public void Zip64_Update_DNZ()
        {
            // this should take about an hour
            string[] zipFilesToUpdate = GetHugeZipFiles();
            Assert.IsFalse(zipFilesToUpdate == null, "No files were created.");
            // this should take a little less than an hour
            Zip64UpdateAddFiles(zipFilesToUpdate[1], "DNZ");
        }


        private void Zip64UpdateAddFiles(string zipFileToUpdate, string marker)
        {
            _txrx = TestUtilities.StartProgressMonitor("Zip64-Update",
                                                       "Zip64 Update - " + marker,
                                                       "Starting up...");

            int numUpdates = 2;
            int baseSize = _rnd.Next(0x1000ff) + 80000;
            System.Threading.Thread.Sleep(120);
            // pb 0: numUpdates + before+after verify steps
            _txrx.Send( String.Format("pb 0 max {0}", numUpdates + 2));
            string workingZipFile = "Z64Update." + marker + ".zip";
            _testTitle = "Zip64 Update - " + marker + " - initial verify";
            _txrx.Send("pb 0 value 0");
            Assert.IsTrue(File.Exists(zipFileToUpdate),
                          "The required ZIP file does not exist ({0})",
                          zipFileToUpdate);

            // make sure the zip is larger than the 4.2gb size
            FileInfo fi = new FileInfo(zipFileToUpdate);
            Assert.IsTrue(fi.Length > (long)System.UInt32.MaxValue,
                          "The zip file ({0}) is not large enough.",
                          zipFileToUpdate);

            // this usually takes 10-12 minutes
            TestContext.WriteLine("Verifying the zip - " +
                                  DateTime.Now.ToString("G"));
            _txrx.Send("status Verifying the zip");
            Zip64VerifyZip(zipFileToUpdate);

            _txrx.Send("pb 0 value 1");

            var sw = new StringWriter();
            for (int j=0; j < numUpdates; j++)
            {
                _testTitle = String.Format("Zip64 Update - {0} - ({1}/{2})",
                                           marker, j+1, numUpdates);
                _pb1Set = false;
                System.Threading.Thread.Sleep(220);
                _txrx.Send("test " + _testTitle);
                // create another folder with a single file in it
                string subdir = String.Format("newfolder-{0}", j);
                Directory.CreateDirectory(subdir);
                string fileName = Path.Combine(subdir,
                                               System.Guid.NewGuid().ToString() + ".txt");
                long size = baseSize + _rnd.Next(28000);
                TestUtilities.CreateAndFillFileText(fileName, size);

                TestContext.WriteLine("");
                TestContext.WriteLine("Updating the zip file, cycle {0}...{1}",
                                      j, DateTime.Now.ToString("G"));
                _txrx.Send("status Updating the zip file...");
                // update the zip with that new folder+file
                // will take a long time for large files
                using (ZipFile zip = ZipFile.Read(zipFileToUpdate))
                {
                    zip.SaveProgress += Zip64SaveProgress;
                    zip.StatusMessageTextWriter = sw;
                    zip.UpdateDirectory(subdir, subdir);
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.BufferSize = 65536*8; // 65536 * 8 = 512k
                    zip.Save(workingZipFile);
                }

                TestContext.WriteLine("Save complete "  +
                                      DateTime.Now.ToString("G"));
                zipFileToUpdate = workingZipFile; // for subsequent updates
                // emit status into the log if available
                string status = sw.ToString();
                if (status != null && status != "")
                {
                    var lines = status.Split('\n');
                    TestContext.WriteLine("status: ("
                                          + DateTime.Now.ToString("G") + ")");
                    foreach (string line in lines)
                        TestContext.WriteLine(line);
                }

                _txrx.Send("pb 0 value " + (j+2));
            }

            System.Threading.Thread.Sleep(120);

            // make sure the zip is larger than the 4.2gb size
            fi = new FileInfo(workingZipFile);
            Assert.IsTrue(fi.Length > (long)System.UInt32.MaxValue,
                          "The zip file ({0}) is not large enough.",
                          workingZipFile);

            TestContext.WriteLine("");
            TestContext.WriteLine("Verifying the zip again... " +
                                  DateTime.Now.ToString("G"));
            _txrx.Send("status Verifying the zip again...");
            _testTitle = String.Format("Zip64 Update - {0} - final verify",
                                       marker);
            _pb1Set = false;
            System.Threading.Thread.Sleep(220);
            _txrx.Send("test " + _testTitle);
            Zip64VerifyZip(workingZipFile);

            _txrx.Send( String.Format("pb 0 value {0}", numUpdates + 1));
        }



        [TestMethod]
        public void Zip64_Winzip_Unzip_OneFile()
        {
            string testBin = TestUtilities.GetTestBinDir(CurrentDir);
            string fileToZip = Path.Combine(testBin, "Ionic.Zip.dll");

            Directory.SetCurrentDirectory(TopLevelDir);

            for (int p=0; p < compLevels.Length; p++)
            {
                for (int n=0; n < crypto.Length; n++)
                {
                    for (int m=0; m < z64.Length; m++)
                    {
                        string zipFile = String.Format("WZ-Unzip.OneFile.{0}.{1}.{2}.zip",
                                                       compLevels[p].ToString(),
                                                       crypto[n].ToString(),
                                                       z64[m].ToString());
                        string password = Path.GetRandomFileName();

                        TestContext.WriteLine("=================================");
                        TestContext.WriteLine("Creating {0}...", Path.GetFileName(zipFile));
                        TestContext.WriteLine("Encryption:{0}  Zip64:{1} pw={2}  compLevel={3}",
                                              crypto[n].ToString(), z64[m].ToString(), password, compLevels[p].ToString());

                        using (var zip = new ZipFile())
                        {
                            zip.Comment = String.Format("Encryption={0}  Zip64={1}  pw={2}",
                                                        crypto[n].ToString(), z64[m].ToString(), password);
                            zip.Encryption = crypto[n];
                            zip.Password = password;
                            zip.CompressionLevel = compLevels[p];
                            zip.UseZip64WhenSaving = z64[m];
                            zip.AddFile(fileToZip, "file");
                            zip.Save(zipFile);
                        }

                        TestContext.WriteLine("Unzipping with WinZip...");

                        string extractDir = String.Format("extract.{0}.{1}.{2}",p,n,m);
                        Directory.CreateDirectory(extractDir);

                        // this will throw if the command has a non-zero exit code.
                        this.Exec(wzunzip,
                                  String.Format("-s{0} -d {1} {2}\\", password, zipFile, extractDir));
                    }
                }
            }
        }



        [Timeout((int)(1 * 60*60*1000)), TestMethod] // in milliseconds.
        public void Zip64_Winzip_Unzip_Huge()
        {
            string[] zipFilesToExtract = GetHugeZipFiles(); // may take a long time
            int baseSize = _rnd.Next(0x1000ff) + 80000;

            _txrx = TestUtilities.StartProgressMonitor("Zip64-WinZip-Unzip",
                                                       "Zip64 WinZip unzip Huge",
                                                       "Setting up...");

            string extractDir = "extract";
            Directory.SetCurrentDirectory(TopLevelDir);
            Directory.CreateDirectory(extractDir);

            _txrx.Send("pb 0 max " + zipFilesToExtract.Length);

            for (int k=0; k < zipFilesToExtract.Length; k++)
            {
                string zipFileToExtract = zipFilesToExtract[k];
                Assert.IsTrue(File.Exists(zipFileToExtract),
                              "required ZIP file does not exist ({0})",
                              zipFileToExtract);

                System.Threading.Thread.Sleep(120);
                _txrx.Send("pb 1 max 3");
                System.Threading.Thread.Sleep(120);
                _txrx.Send("pb 1 value 0");

                // make sure the zip is larger than the 4.2gb size
                FileInfo fi = new FileInfo(zipFileToExtract);
                Assert.IsTrue(fi.Length > (long)System.UInt32.MaxValue,
                              "The zip file ({0}) is not large enough.",
                              zipFileToExtract);

                _txrx.Send("pb 1 step");
                _txrx.Send("status Counting entries in the zip file...");

                int numEntries = TestUtilities.CountEntries(zipFileToExtract);

                _txrx.Send("status Using WinZip to list the entries...");

                // Examine and unpack the zip archive via WinZip
                // first, examine the zip entry metadata:
                string wzzipOut = this.Exec(wzzip, "-vt " + zipFileToExtract);
                TestContext.WriteLine(wzzipOut);

                int x = 0;
                int y = 0;
                int wzzipEntryCount=0;
                string textToLookFor= "Filename: ";
                TestContext.WriteLine("================");
                TestContext.WriteLine("Files listed by WinZip:");
                while (true)
                {
                    x = wzzipOut.IndexOf(textToLookFor, y);
                    if (x < 0) break;
                    y = wzzipOut.IndexOf("\n", x);
                    string name = wzzipOut.Substring(x + textToLookFor.Length, y-x-1).Trim();
                    TestContext.WriteLine("  {0}", name);
                    if (!name.EndsWith("\\"))
                    {
                        wzzipEntryCount++;
                        if (wzzipEntryCount > numEntries * 3) throw new Exception("too many entries!");
                    }
                }

                TestContext.WriteLine("================");
                Assert.AreEqual(numEntries, wzzipEntryCount,
                                "Unexpected number of entries found by WinZip.");

                _txrx.Send("pb 1 step");
                System.Threading.Thread.Sleep(120);

                _txrx.Send(String.Format("pb 1 max {0}", numEntries*2));
                x=0; y = 0;
                _txrx.Send("status Extracting the entries...");
                int nCycles = 0;
                while (true)
                {
                    _txrx.Send("test Zip64 WinZip unzip - " +
                               Path.GetFileName(zipFileToExtract));
                    x = wzzipOut.IndexOf(textToLookFor, y);
                    if (x < 0) break;
                    if (nCycles > numEntries * 4) throw new Exception("too many entries?");
                    y = wzzipOut.IndexOf("\n", x);
                    string name = wzzipOut.Substring(x + textToLookFor.Length, y-x-1).Trim();
                    if (!name.EndsWith("\\"))
                    {
                        nCycles++;
                        _txrx.Send(String.Format("status Extracting {1}/{2} :: {0}", name, nCycles, wzzipEntryCount));
                        this.Exec(wzunzip,
                                  String.Format("-d {0} {1}\\ \"{2}\"",
                                                Path.GetFileName(zipFileToExtract),
                                                extractDir, name));
                        string path = Path.Combine(extractDir, name);
                        _txrx.Send("pb 1 step");
                        Assert.IsTrue(File.Exists(path), "extracted file ({0}) does not exist", path);
                        File.Delete(path);
                        System.Threading.Thread.Sleep(120);
                        _txrx.Send("pb 1 step");
                    }
                }

                _txrx.Send("pb 0 step");
                System.Threading.Thread.Sleep(120);
            }
        }




        private void CreateLargeFiles(int numFilesToAdd, int baseSize, string dir)
        {
            bool firstFileDone = false;
            string fileName = "";
            long fileSize = 0;

            _txrx.Send(String.Format("pb 1 max {0}", numFilesToAdd));

            Action<Int64> progressUpdate = (x) =>
                {
                    _txrx.Send(String.Format("pb 2 value {0}", x));
                    _txrx.Send(String.Format("status Creating {0}, [{1}/{2}] ({3:N0}%)",
                                             fileName, x, fileSize, ((double)x)/ (0.01 * fileSize) ));
                };

            // It takes some time to create a large file. And we need
            // a bunch of them.
            for (int i = 0; i < numFilesToAdd; i++)
            {
                // randomly select binary or text
                int n = _rnd.Next(2);
                fileName = string.Format("Pippo{0}.{1}", i, (n==0) ? "bin" : "txt" );
                if (i != 0)
                {
                    int x = _rnd.Next(6);
                    if (x != 0)
                    {
                        string folderName = string.Format("folder{0}", x);
                        fileName = Path.Combine(folderName, fileName);
                        if (!Directory.Exists(Path.Combine(dir, folderName)))
                            Directory.CreateDirectory(Path.Combine(dir, folderName));
                    }
                }
                fileName = Path.Combine(dir, fileName);
                // first file is 2x larger
                fileSize = (firstFileDone) ? (baseSize + _rnd.Next(0x880000)) : (2*baseSize);
                _txrx.Send(String.Format("pb 2 max {0}", fileSize));
                if (n==0)
                    TestUtilities.CreateAndFillFileBinary(fileName, fileSize, progressUpdate);
                else
                    TestUtilities.CreateAndFillFileText(fileName, fileSize, progressUpdate);
                firstFileDone = true;
                _txrx.Send("pb 1 step");
            }
        }



        [Timeout((int)(4 * 60*60*1000)), TestMethod] // 60*60*1000 == 1 hr
        public void Zip64_Winzip_Zip_Huge()
        {
            // This TestMethod tests if DNZ can read a huge (>4.2gb) zip64 file
            // created by winzip.
            int baseSize = _rnd.Next(80000) + 0x1000ff;
            _txrx = TestUtilities.StartProgressMonitor("Zip64-WinZip-Zip-Huge",
                                                       "Zip64_Winzip_Zip_Huge()",
                                                       "Creating links");
            string contentDir = "fodder";
            //Directory.SetCurrentDirectory(TopLevelDir);
            Directory.CreateDirectory(contentDir);

            _txrx.Send("pb 0 max 5");

            if (CreateLinksToLargeFiles(contentDir))
                return;

            TestContext.WriteLine("Creating large files..." +
                                  DateTime.Now.ToString("G"));

            CreateLargeFiles(_rnd.Next(4) + 4, baseSize, contentDir);
            _txrx.Send("pb 0 step");

            TestContext.WriteLine("Creating a new Zip with winzip - " +
                                  DateTime.Now.ToString("G"));

            var fileList = Directory.GetFiles(contentDir, "*.*", SearchOption.AllDirectories);

            // create a zip archive via WinZip
            string wzzipOut= null;
            string zipFileToCreate = "Zip64-WinZip-Zip-Huge.zip";
            int nCycles= 0;
            _txrx.Send(String.Format("pb 1 max {0}", fileList.Length));
            System.Threading.Thread.Sleep(120);
            _txrx.Send("pb 2 value 0");

            // Add one file at a time, invoking wzzip.exe for each. After it
            // completes, delete the just-added file. This allows coarse-grained
            // status updates in the progress window.  Not sure about the exact
            // impact on disk space, or execution time; my observation is that the
            // time-cost to add one entry increases, as the zip file gets
            // larger. Each successive cycle takes a little longer.  It's tolerable
            // I guess.  A tradeoff to get visual progress feedback.
            foreach (var filename in fileList)
            {
                nCycles++;
                _txrx.Send(String.Format("status wzzip.exe adding {0}...({1}/{2})", filename, nCycles, fileList.Length+1));
                wzzipOut = this.Exec(wzzip, String.Format("-a -p -r -yx \"{0}\" \"{1}\"",
                                                          zipFileToCreate,
                                                          filename));
                TestContext.WriteLine(wzzipOut);
                _txrx.Send("pb 1 step");
                System.Threading.Thread.Sleep(420);
                File.Delete(filename);
            }

            // Create one additional small text file and add it to the zip.  For
            // this test, it must be added last, at the end of the ZIP file.
            TestContext.WriteLine("Inserting one additional file with wzzip.exe - " +
                                  DateTime.Now.ToString("G"));
            nCycles++;
            var newfile = Path.Combine(contentDir, "zzz-" + Path.GetRandomFileName() + ".txt");
            _txrx.Send(String.Format("status adding {0}...({1}/{2})", newfile, nCycles, fileList.Length+1));
            int filesize = _rnd.Next(50000) + 440000;
            TestUtilities.CreateAndFillFileText(newfile, filesize);
            wzzipOut = this.Exec(wzzip, String.Format("-a -p -r -yx \"{0}\" \"{1}\"", zipFileToCreate, newfile));
            TestContext.WriteLine(wzzipOut);
            _txrx.Send("pb 1 step");
            System.Threading.Thread.Sleep(120);
            File.Delete(newfile);

            _txrx.Send("pb 0 step");
            System.Threading.Thread.Sleep(120);

            // make sure the zip is larger than the 4.2gb size
            FileInfo fi = new FileInfo(zipFileToCreate);
            Assert.IsTrue(fi.Length > (long)System.UInt32.MaxValue,
                          "The zip file ({0}) is not large enough.",
                          zipFileToCreate);

            // Now use DotNetZip to extract the large zip file to the bit bucket.
            TestContext.WriteLine("Verifying the new Zip with DotNetZip - " +
                                  DateTime.Now.ToString("G"));
            _txrx.Send("status Verifying the zip");
            verb = "verifying";
            _testTitle = "Zip64 Winzip Zip Huge - final verify";
            Zip64VerifyZip(zipFileToCreate);

            _txrx.Send("pb 0 step");
            System.Threading.Thread.Sleep(120);
        }



        [TestMethod, Timeout((int)(2 * 60*60*1000))] // 60*60*1000 = 1 hr
        public void Zip64_Winzip_Setup()
        {
            // Not really a test.  This thing just sets up the big zip file.
            TestContext.WriteLine("This test merely checks for existence of two large zip");
            TestContext.WriteLine("files, in a well-known place, and creates them as");
            TestContext.WriteLine("necessary. The zips are then used by other tests.");
            TestContext.WriteLine(" ");
            GetHugeZipFiles(); // usually takes about an hour
        }


        private void EmitStatus(String s)
        {
            TestContext.WriteLine("status:");
            foreach (string line in s.Split('\n'))
                TestContext.WriteLine(line);
        }


        [TestMethod, Timeout(1 * 60*60*1000)]
        public void Zip64_Over_4gb()
        {
            Int64 desiredSize= System.UInt32.MaxValue;
            desiredSize+= System.Int32.MaxValue/4;
            desiredSize+= _rnd.Next(0x1000000);
            _testTitle = "Zip64 Create/Zip/Extract a file > 4.2gb";

            _txrx = TestUtilities.StartProgressMonitor("Zip64-Over-4gb",
                                                       _testTitle,
                                                       "starting up...");

            string zipFileToCreate = Path.Combine(TopLevelDir, "Zip64_Over_4gb.zip");
            Directory.SetCurrentDirectory(TopLevelDir);
            string nameOfFodderFile="VeryVeryLargeFile.txt";
            string nameOfExtractedFile = nameOfFodderFile + ".extracted";

            // Steps in this test: 4
            _txrx.Send("pb 0 max 4");

            TestContext.WriteLine("");
            TestContext.WriteLine("Creating a large file..." +
                                  DateTime.Now.ToString("G"));

            // create a very large file
            Action<Int64> progressUpdate = (x) =>
                {
                    _txrx.Send(String.Format("pb 1 value {0}", x));
                    _txrx.Send(String.Format("status Creating {0}, [{1}/{2}mb] ({3:N0}%)",
                                             nameOfFodderFile,
                                             x/(1024*1024),
                                             desiredSize/(1024*1024),
                                             ((double)x)/ (0.01 * desiredSize)));
                };

            // This takes a few minutes...
            _txrx.Send(String.Format("pb 1 max {0}", desiredSize));
            TestUtilities.CreateAndFillFileText(nameOfFodderFile,
                                                desiredSize,
                                                progressUpdate);

            // make sure it is larger than 4.2gb
            FileInfo fi = new FileInfo(nameOfFodderFile);
            Assert.IsTrue(fi.Length > (long)System.UInt32.MaxValue,
                          "The fodder file ({0}) is not large enough.",
                          nameOfFodderFile);

            TestContext.WriteLine("");
            TestContext.WriteLine("computing checksum..." +
                                  DateTime.Now.ToString("G"));
            _txrx.Send("status computing checksum...");
            var chk1 = TestUtilities.ComputeChecksum(nameOfFodderFile);

            _txrx.Send("pb 0 step");

            var sw = new StringWriter();
            using (var zip = new ZipFile())
            {
                zip.StatusMessageTextWriter = sw;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.BufferSize = 65536*8; // 65536 * 8 = 512k
                zip.SaveProgress += Zip64SaveProgress;
                var e = zip.AddFile(nameOfFodderFile, "");
                _txrx.Send("status Saving......");
                TestContext.WriteLine("zipping one file......" +
                                  DateTime.Now.ToString("G"));
                zip.Save(zipFileToCreate);
            }

            EmitStatus(sw.ToString());

            File.Delete(nameOfFodderFile);
            TestContext.WriteLine("");
            TestContext.WriteLine("Extracting the zip..." +
                                  DateTime.Now.ToString("G"));
            _txrx.Send("status Extracting the file...");
            _txrx.Send("pb 0 step");

            var options = new ReadOptions { StatusMessageWriter= new StringWriter() };
            verb = "Extracting";
            _pb1Set = false;
            using (var zip = ZipFile.Read(zipFileToCreate, options))
            {
                Assert.AreEqual<int>(1, zip.Entries.Count,
                                     "Incorrect number of entries in the zip file");
                zip.ExtractProgress += Zip64ExtractProgress;
                _numFilesToExtract = zip.Entries.Count;
                _numExtracted= 1;
                ZipEntry e = zip[0];
                e.FileName = nameOfExtractedFile;
                _txrx.Send("status extracting......");
                e.Extract();
            }

            EmitStatus(options.StatusMessageWriter.ToString());
            _txrx.Send("pb 0 step");
            System.Threading.Thread.Sleep(120);

            TestContext.WriteLine("");
            TestContext.WriteLine("computing checksum..." +
                                  DateTime.Now.ToString("G"));
            _txrx.Send("status computing checksum...");
            var chk2 = TestUtilities.ComputeChecksum(nameOfExtractedFile);
            Assert.AreEqual<String>(TestUtilities.CheckSumToString(chk1),
                                    TestUtilities.CheckSumToString(chk2),
                                    "Checksum mismatch");
            _txrx.Send("pb 0 step");
        }


        [TestMethod, Timeout(1 * 60*60*1000)]
        public void Z64_ManyEntries_NoEncryption_DefaultCompression_AsNecessary()
        {
            _Zip64_Over65534Entries(Zip64Option.AsNecessary, EncryptionAlgorithm.None, Ionic.Zlib.CompressionLevel.Default);
        }

        [TestMethod, Timeout(1 * 60*60*1000)]
        public void Z64_ManyEntries_PkZipEncryption_DefaultCompression_AsNecessary()
        {
            _Zip64_Over65534Entries(Zip64Option.AsNecessary, EncryptionAlgorithm.PkzipWeak, Ionic.Zlib.CompressionLevel.Default);
        }

        [TestMethod, Timeout(2 * 60*60*1000)]
        public void Z64_ManyEntries_WinZipEncryption_DefaultCompression_AsNecessary()
        {
            _Zip64_Over65534Entries(Zip64Option.AsNecessary, EncryptionAlgorithm.WinZipAes256, Ionic.Zlib.CompressionLevel.Default);
        }


        [TestMethod, Timeout(1 * 60*60*1000)]
        public void Z64_ManyEntries_NoEncryption_DefaultCompression_Always()
        {
            _Zip64_Over65534Entries(Zip64Option.Always, EncryptionAlgorithm.None, Ionic.Zlib.CompressionLevel.Default);
        }

        [TestMethod, Timeout(1 * 60*60*1000)]
        public void Z64_ManyEntries_PkZipEncryption_DefaultCompression_Always()
        {
            _Zip64_Over65534Entries(Zip64Option.Always, EncryptionAlgorithm.PkzipWeak, Ionic.Zlib.CompressionLevel.Default);
        }

        [TestMethod, Timeout(2 * 60*60*1000)]
        public void Z64_ManyEntries_WinZipEncryption_DefaultCompression_Always()
        {
            _Zip64_Over65534Entries(Zip64Option.Always, EncryptionAlgorithm.WinZipAes256, Ionic.Zlib.CompressionLevel.Default);
        }




        [TestMethod, Timeout(30 * 60*1000)]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Z64_ManyEntries_NOZIP64()
        {
            _Zip64_Over65534Entries(Zip64Option.Never, EncryptionAlgorithm.None, Ionic.Zlib.CompressionLevel.Default);
        }


        void _Zip64_Over65534Entries(Zip64Option z64option,
                                            EncryptionAlgorithm encryption,
                                            Ionic.Zlib.CompressionLevel compression)
        {
            // Emitting a zip file with > 65534 entries requires the use of ZIP64 in
            // the central directory.
            int numTotalEntries = _rnd.Next(4616)+65534;
            //int numTotalEntries = _rnd.Next(461)+6534;
            //int numTotalEntries = _rnd.Next(46)+653;
            string enc = encryption.ToString();
            if (enc.StartsWith("WinZip")) enc = enc.Substring(6);
            else if (enc.StartsWith("Pkzip")) enc = enc.Substring(0,5);
            string zipFileToCreate = String.Format("Zip64.ZF_Over65534.{0}.{1}.{2}.zip",
                                                   z64option.ToString(),
                                                   enc,
                                                   compression.ToString());

            _testTitle = String.Format("ZipFile #{0} 64({1}) E({2}), C({3})",
                                       numTotalEntries,
                                       z64option.ToString(),
                                       enc,
                                       compression.ToString());
            _txrx = TestUtilities.StartProgressMonitor(zipFileToCreate,
                                                       _testTitle,
                                                       "starting up...");

            _txrx.Send("pb 0 max 4"); // 3 stages: AddEntry, Save, Verify
            _txrx.Send("pb 0 value 0");

            string password = Path.GetRandomFileName();

            string statusString = String.Format("status Encrypt:{0} Compress:{1}...",
                                                enc,
                                                compression.ToString());

            int numSaved = 0;
            var saveProgress = new EventHandler<SaveProgressEventArgs>( (sender, e) => {
                    switch (e.EventType)
                    {
                        case ZipProgressEventType.Saving_Started:
                        _txrx.Send("status saving...");
                        _txrx.Send("pb 1 max " + numTotalEntries);
                        numSaved= 0;
                        break;

                        case ZipProgressEventType.Saving_AfterWriteEntry:
                        numSaved++;
                        if ((numSaved % 128) == 0)
                        {
                            _txrx.Send("pb 1 value " + numSaved);
                            _txrx.Send(String.Format("status Saving entry {0}/{1} ({2:N0}%)",
                                                     numSaved, numTotalEntries,
                                                     numSaved / (0.01 * numTotalEntries)
                                                     ));
                        }
                        break;

                        case ZipProgressEventType.Saving_Completed:
                        _txrx.Send("status Save completed");
                        _txrx.Send("pb 1 max 1");
                        _txrx.Send("pb 1 value 1");
                        break;
                    }
                });

            string contentFormatString =
                "This is the content for entry #{0}.\r\n\r\n" +
                "AAAAAAA BBBBBB AAAAA BBBBB AAAAA BBBBB AAAAA\r\n"+
                "AAAAAAA BBBBBB AAAAA BBBBB AAAAA BBBBB AAAAA\r\n";
            _txrx.Send(statusString);
            int dirCount= 0;
            using (var zip = new ZipFile())
            {
                _txrx.Send(String.Format("pb 1 max {0}", numTotalEntries));
                _txrx.Send("pb 1 value 0");

                zip.Password = password;
                zip.Encryption = encryption;
                zip.CompressionLevel = compression;
                zip.SaveProgress += saveProgress;
                zip.UseZip64WhenSaving = z64option;
                // save space when saving the file:
                zip.EmitTimesInWindowsFormatWhenSaving = false;
                zip.EmitTimesInUnixFormatWhenSaving = false;

                // add files:
                for (int m=0; m<numTotalEntries; m++)
                {
                    if (_rnd.Next(7)==0)
                    {
                        string entryName = String.Format("{0:D5}", m);
                        zip.AddDirectoryByName(entryName);
                        dirCount++;
                    }
                    else
                    {
                        string entryName = String.Format("{0:D5}.txt", m);
                        if (_rnd.Next(12)==0)
                        {
                            string contentBuffer = String.Format(contentFormatString, m);
                            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(contentBuffer);
                            zip.AddEntry(entryName, contentBuffer);
                        }
                        else
                            zip.AddEntry(entryName, Stream.Null);
                    }

                    if (m % 1024 == 0)
                    {
                        _txrx.Send("pb 1 value " + m);
                        string msg = String.Format("status adding entry {0}/{1}  ({2:N0}%)",
                                            m, numTotalEntries, (m/(0.01*numTotalEntries)));
                        _txrx.Send(msg);
                    }
                }

                _txrx.Send("pb 0 step");
                _txrx.Send(statusString + " Saving...");
                zip.Save(zipFileToCreate);
            }

            _txrx.Send("pb 0 step");
            _txrx.Send("pb 1 value 0");
            _txrx.Send("status Reading...");

            // verify the zip by unpacking.
            _numFilesToExtract = numTotalEntries;
            _numExtracted= 1;
            _pb1Set = false;
            verb = "verify";
            BasicVerifyZip(zipFileToCreate, password, false, Zip64ExtractProgress);

            _txrx.Send("pb 0 step");
            _txrx.Send("status successful extract, now doing final count...");
            _txrx.Send("pb 1 value 0");
            Assert.AreEqual<int>(numTotalEntries-dirCount,
                                 TestUtilities.CountEntries(zipFileToCreate));
            _txrx.Send("pb 0 step");
        }



        [Timeout(3 * 60*60*1000), TestMethod]    // 60*60*1000 = 1 hr
        public void Zip64_UpdateEntryComment_wi9214_WZ()
        {
            // Should take 2.5 hrs when creating the huge files, about 1 hr when the
            // files already exist.
            string[] zipFilesToUpdate = GetHugeZipFiles();
            Assert.IsTrue(File.Exists(zipFilesToUpdate[0]),
                          "The required ZIP file does not exist ({0})",
                          zipFilesToUpdate[0]);
            Z64UpdateHugeZipWithComment(zipFilesToUpdate[0], "WinZip");
        }

        [Timeout(3 * 60*60*1000), TestMethod]    // 60*60*1000 = 1 hr
        public void Zip64_UpdateEntryComment_wi9214_DNZ()
        {
            // Should take 2.5 hrs when creating the huge files, about 1 hr when the
            // files already exist.
            string[] zipFilesToUpdate = GetHugeZipFiles();
            Assert.IsTrue(File.Exists(zipFilesToUpdate[1]),
                          "The required ZIP file does not exist ({0})",
                          zipFilesToUpdate[1]);
            Z64UpdateHugeZipWithComment(zipFilesToUpdate[1], "DNZ");
        }


        void Z64UpdateHugeZipWithComment(string zipFileToUpdate, string marker)
        {
            // Test whether opening a huge zip64 file and then re-saving, allows the
            // file to remain valid.  Must use a huge zip64 file over 4gb in order
            // to verify this, and at least one entry must be > 4gb uncompressed.
            // Want to check that UseZip64WhenSaving is automatically selected as
            // appropriate.
            string zipFileToCreate = "update-huge-zip.zip";

            string newComment = "This is an updated comment on the first entry"+
                                "in the zip that is larger than 4gb. (" +
                                DateTime.Now.ToString("u") +")";

            // start the progress monitor
            _txrx = TestUtilities.StartProgressMonitor("Zip64-update",
                                                       "Zip64 update",
                                                       "Checking file...");

            // make sure the zip is larger than the 4.2gb size
            FileInfo fi = new FileInfo(zipFileToUpdate);
            Assert.IsTrue(fi.Length > (long)System.UInt32.MaxValue,
                          "The zip file ({0}) is not large enough.",
                          zipFileToUpdate);
            TestContext.WriteLine("Verifying the zip file..." +
                                  DateTime.Now.ToString("G"));
            System.Threading.Thread.Sleep(220);
            _txrx.Send("status Verifying the zip");
            System.Threading.Thread.Sleep(220);
            _txrx.Send("title Zip64 Update Entry Comment wi9214");

            // According to workitem 9214, the comment must be modified
            // on an entry that is larger than 4gb (uncompressed).
            // Check that here.
            using (ZipFile zip = ZipFile.Read(zipFileToUpdate))
            {
                ZipEntry bigEntry = null;
                foreach (var e2 in zip)
                {
                    if (e2.UncompressedSize > (long)System.UInt32.MaxValue)
                    {
                        bigEntry = e2;
                        break;
                    }
                }
                Assert.IsTrue(bigEntry != null &&
                              bigEntry.UncompressedSize > (long)System.UInt32.MaxValue,
                              "Minimum size constraint not met.");
            }

            // Verify the zip is correct, can be extracted.
            // This will take some time for a large zip.
            // (+1 to _numFilesToExtract for the directory)
            _numFilesToExtract = TestUtilities.CountEntries(zipFileToUpdate) + 1;
            _numExtracted= 1;
            verb = "extract";
            _pb1Set = false;

            // _testTitle is used in Zip64{Save,Extract}Progress
            _testTitle = "Zip64 Update Entry Comment - " + marker;

            var extract1 = BasicVerifyZip(zipFileToUpdate, null, false, Zip64ExtractProgress);

            _txrx.Send("status removing the extract directory...");
            Directory.Delete(extract1, true);

            TestContext.WriteLine("Updating the zip file..."
                                  + DateTime.Now.ToString("G"));
            _txrx.Send("status Updating the zip file...");

            // update the zip with one small change: a new comment on
            // the biggest entry.
            var sw = new StringWriter();
            using (ZipFile zip = ZipFile.Read(zipFileToUpdate))
            {
                // required: the option must be set automatically and intelligently
                Assert.IsTrue(zip.UseZip64WhenSaving == Zip64Option.Always,
                              "The UseZip64WhenSaving option is set incorrectly ({0})",
                              zip.UseZip64WhenSaving);

                // according to workitem 9214, the comment must be modified
                // on an entry that is larger than 4gb (uncompressed)
                ZipEntry bigEntry = null;
                foreach (var e2 in zip)
                {
                    if (e2.UncompressedSize > (long)System.UInt32.MaxValue)
                    {
                        bigEntry = e2;
                        break;
                    }
                }
                // redundant with the check above, but so what?
                Assert.IsTrue(bigEntry != null &&
                              bigEntry.UncompressedSize > (long)System.UInt32.MaxValue,
                              "Minimum size constraint not met.");

                bigEntry.Comment = newComment;
                zip.SaveProgress += Zip64SaveProgress;
                zip.StatusMessageTextWriter = sw;
                zip.Save(zipFileToCreate);
            }
            string status = sw.ToString();
            if (status != null && status != "")
            {
                var lines = status.Split('\n');
                TestContext.WriteLine("status: ("
                                      + DateTime.Now.ToString("G") + ")");
                foreach (string line in lines)
                    TestContext.WriteLine(line);
            }

            TestContext.WriteLine("Verifying the updated zip... "
                                  + DateTime.Now.ToString("G"));
            _txrx.Send("status Verifying the updated zip");
            Zip64VerifyZip(zipFileToCreate); // can take an hour or more

            // finally, verify that the modified comment is correct.
            _txrx.Send("status checking the updated comment");
            using (ZipFile zip = ZipFile.Read(zipFileToCreate))
            {
                // check that the z64 option is set automatically and intelligently
                Assert.IsTrue(zip.UseZip64WhenSaving == Zip64Option.Always,
                              "The UseZip64WhenSaving option is set incorrectly ({0})",
                              zip.UseZip64WhenSaving);

                ZipEntry e = null;
                foreach (var e2 in zip)
                {
                    if (e2.UncompressedSize > (long)System.UInt32.MaxValue)
                    {
                        e = e2;
                        break;
                    }
                }
                Assert.IsTrue(e != null &&  e.UncompressedSize > (long)System.UInt32.MaxValue,
                              "No entry in the zip file is large enough.");
                Assert.AreEqual<String>(newComment, e.Comment, "The comment on the entry is unexpected.");
                TestContext.WriteLine("The comment on the entry is {0}", e.Comment);
            }
            TestContext.WriteLine("All done... "
                                  + DateTime.Now.ToString("G"));
        }



    }

}
