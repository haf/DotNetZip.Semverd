// SplitArchives.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa.
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
// Time-stamp: <2011-July-14 11:07:00>
//
// ------------------------------------------------------------------
//
// This module defines tests for split (or 'spanned') archives.
//
// ------------------------------------------------------------------

// define REMOTE_FILESYSTEM in order to use a remote filesystem for storage of
// the ZIP64 large archive (which can beb huge). Leave it undefined to simply
// use the local TEMP directory.
//#define REMOTE_FILESYSTEM

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interop=System.Runtime.InteropServices;


using Ionic.Zip.Tests.Utilities;


namespace Ionic.Zip.Tests.Split
{
    /// <summary>
    /// Summary description for ErrorTests
    /// </summary>
    [TestClass]
    public class Split : IonicTestClass
    {
        [Interop.DllImport("kernel32.dll",
                           EntryPoint="CreateSymbolicLinkW",
                           CharSet=Interop.CharSet.Unicode)]
        public static extern int CreateSymbolicLink(string lpSymlinkFileName,
                                                    string lpTargetFileName,
                                                    int dwFlags);


        [TestMethod, Timeout(6 * 60*1000)]
        public void Spanned_Create()
        {
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            _txrx = TestUtilities.StartProgressMonitor("segmentedzip",
                                                       "Segmented Zips",
                                                       "Creating files");
            _txrx.Send("pb 0 max 2");

            int numFiles = _rnd.Next(10) + 8;
            int overflows = 0;
            string msg;

            _txrx.Send("pb 1 max " + numFiles);

            var update = new Action<int,int,Int64>( (x,y,z) => {
                    switch (x)
                    {
                        case 0:
                        _txrx.Send(String.Format("pb 2 max {0}", ((int)z)));
                        break;
                        case 1:
                        msg = String.Format("pb 2 value {0}", ((int)z));
                        _txrx.Send(msg);
                        break;
                        case 2:
                        _txrx.Send("pb 1 step");
                        _txrx.Send("pb 2 value 0");
                        msg = String.Format("status created {0}/{1} files",
                                            y+1,
                                            ((int)z));
                        _txrx.Send(msg);
                        break;
                    }
                });

            _txrx.Send("status creating " + numFiles + " files...");

            string[] filesToZip;
            Dictionary<string, byte[]> checksums;
            CreateLargeFilesWithChecksums(dirToZip, numFiles, update,
                                          out filesToZip, out checksums);
            _txrx.Send("pb 0 step");
            int[] segmentSizes = { 0, 64*1024, 128*1024, 512*1024, 1024*1024,
                                   2*1024*1024, 8*1024*1024, 16*1024*1024,
                                   1024*1024*1024 };

            _txrx.Send("status zipping...");
            _txrx.Send(String.Format("pb 1 max {0}", segmentSizes.Length));

            System.EventHandler<Ionic.Zip.SaveProgressEventArgs> sp = (sender1, e1) =>
                {
                    switch (e1.EventType)
                    {
                        case ZipProgressEventType.Saving_Started:
                        _txrx.Send(String.Format("pb 2 max {0}", filesToZip.Length));
                        _txrx.Send("pb 2 value 0");
                        break;

                        case ZipProgressEventType.Saving_AfterWriteEntry:
                        TestContext.WriteLine("Saved entry {0}, {1} bytes",
                                              e1.CurrentEntry.FileName,
                                              e1.CurrentEntry.UncompressedSize);
                        _txrx.Send("pb 2 step");
                        break;
                    }
                };


            for (int m=0; m < segmentSizes.Length; m++)
            {
                string trialDir = String.Format("trial{0}", m);
                Directory.CreateDirectory(trialDir);
                string zipFileToCreate = Path.Combine(trialDir,
                                                      String.Format("Archive-{0}.zip",m));
                int maxSegSize = segmentSizes[m];

                msg = String.Format("status trial {0}/{1}  (max seg size {2}k)",
                                    m+1, segmentSizes.Length, maxSegSize/1024);
                _txrx.Send(msg);

                TestContext.WriteLine("=======");
                TestContext.WriteLine("Trial {0}", m);
                if (maxSegSize > 0)
                    TestContext.WriteLine("Creating a segmented zip...segsize({0})", maxSegSize);
                else
                    TestContext.WriteLine("Creating a regular zip...");

                var sw = new StringWriter();
                bool aok = false;
                try
                {
                    using (var zip = new ZipFile())
                    {
                        zip.StatusMessageTextWriter = sw;
                        zip.BufferSize = 0x8000;
                        zip.CodecBufferSize = 0x8000;
                        zip.AddDirectory(dirToZip, "files");
                        zip.MaxOutputSegmentSize = maxSegSize;
                        zip.SaveProgress += sp;
                        zip.Save(zipFileToCreate);
                    }
                    aok = true;
                }
                catch (OverflowException)
                {
                    TestContext.WriteLine("Overflow - too many segments...");
                    overflows++;
                }

                if (aok)
                {
                    TestContext.WriteLine("{0}", sw.ToString());

                    // // If you want to see the diskNumber for each entry,
                    // // uncomment the following:
                    // TestContext.WriteLine("Checking info...");
                    // sw = new StringWriter();
                    // //string extractDir = String.Format("ex{0}", m);
                    // using (var zip = ZipFile.Read(zipFileToCreate))
                    // {
                    //     zip.StatusMessageTextWriter = sw;
                    //     foreach (string s in zip.Info.Split('\r','\n'))
                    //     {
                    //         Console.WriteLine("{0}", s);
                    //     }
                    //
                    //     // unnecessary - BasicVerify does this
                    //     //foreach (var e in zip)
                    //     //e.Extract(extractDir);
                    // }
                    // TestContext.WriteLine("{0}", sw.ToString());

                    TestContext.WriteLine("Extracting...");
                    string extractDir = BasicVerifyZip(zipFileToCreate);

                    // also verify checksums
                    VerifyChecksums(Path.Combine(extractDir, "files"), filesToZip, checksums);
                }
                _txrx.Send("pb 1 step");
            }

            _txrx.Send("pb 0 step");

            Assert.IsTrue(overflows < 3, "Too many overflows. Check the test.");
        }



        bool _pb1Set;
        bool _pb2Set;
        int _numExtracted;
        int _numFilesToExtract;
        int _nCycles;
        void ExtractProgress(object sender, ExtractProgressEventArgs e)
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
                    _nCycles = 0;
                    break;

                case ZipProgressEventType.Extracting_EntryBytesWritten:
                    if (!_pb2Set)
                    {
                        _txrx.Send(String.Format("pb 2 max {0}", e.TotalBytesToTransfer));
                        _pb2Set = true;
                    }
                    // for performance, don't update the progress monitor every time.
                    _nCycles++;
                    if (_nCycles % 64 == 0)
                    {
                    _txrx.Send(String.Format("status Extracting entry {0}/{1} :: {2} :: {3}/{4}mb ::  {5:N0}%",
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
                    _txrx.Send("pb 1 step");
                    break;
            }
        }



        [TestMethod]
        [Timeout(90 * 60*1000)]
        public void Create_LargeSegmentedArchive()
        {
            // There was a claim that large archives (around or above
            // 1gb) did not work well with archive splitting.  This test
            // covers that case.

#if REMOTE_FILESYSTEM
            string parentDir = Path.Combine("t:\\tdir", Path.GetFileNameWithoutExtension(TopLevelDir));
            _FilesToRemove.Add(parentDir);
            Directory.CreateDirectory(parentDir);
            string zipFileToCreate = Path.Combine(parentDir,
                                                  "Create_LargeSegmentedArchive.zip");
#else
            string zipFileToCreate = Path.Combine(TopLevelDir, "Create_LargeSegmentedArchive.zip");
#endif
            TestContext.WriteLine("Creating file {0}", zipFileToCreate);

            // This file will "cache" the randomly generated text, so we
            // don't have to generate more than once. You know, for
            // speed.
            string cacheFile = Path.Combine(TopLevelDir, "cacheFile.txt");

            // int maxSegSize = 4*1024*1024;
            // int sizeBase =   20 * 1024 * 1024;
            // int sizeRandom = 1 * 1024 * 1024;
            // int numFiles = 3;

            // int maxSegSize = 80*1024*1024;
            // int sizeBase =   320 * 1024 * 1024;
            // int sizeRandom = 20 * 1024 * 1024 ;
            // int numFiles = 5;

            int maxSegSize = 120*1024*1024;
            int sizeBase =   420 * 1024 * 1024;
            int sizeRandom = 20 * 1024 * 1024;
            int numFiles = _rnd.Next(5) + 11;

            TestContext.WriteLine("The zip will contain {0} files", numFiles);

            int numSaving= 0, totalToSave = 0, numSegs= 0;
            long sz = 0;


            // There are a bunch of Action<T>'s here.  This test method originally
            // used ZipFile.AddEntry overload that accepts an opener/closer pair.
            // It conjured content for the files out of a RandomTextGenerator
            // stream.  This worked, but was very very slow. So I took a new
            // approach to use a WriteDelegate, and still contrive the data, but
            // cache it for entries after the first one. This makes things go much
            // faster.
            //
            // But, when using the WriteDelegate, the SaveProgress events of
            // flavor ZipProgressEventType.Saving_EntryBytesRead do not get
            // called. Therefore the progress updates are done from within the
            // WriteDelegate itself. The SaveProgress events for SavingStarted,
            // BeforeWriteEntry, and AfterWriteEntry do get called.  As a result
            // this method uses 2 delegates: one for writing and one for the
            // SaveProgress events.

            WriteDelegate writer = (name, stream) =>
                {
                    Stream input = null;
                    Stream cache = null;
                    try
                    {
                        // use a cahce file as the content.  The entry
                        // name will vary but we'll get the content for
                        // each entry from the a single cache file.
                        if (File.Exists(cacheFile))
                        {
                            input = File.Open(cacheFile,
                                              FileMode.Open,
                                              FileAccess.ReadWrite,
                                              FileShare.ReadWrite);
                            // Make the file slightly shorter with each
                            // successive entry, - just to shake things
                            // up a little.  Also seek forward a little.
                            var fl = input.Length;
                            input.SetLength(fl - _rnd.Next(sizeRandom/2) + 5201);
                            input.Seek(_rnd.Next(sizeRandom/2), SeekOrigin.Begin);
                        }
                        else
                        {
                            sz = sizeBase + _rnd.Next(sizeRandom);
                            input = new Ionic.Zip.Tests.Utilities.RandomTextInputStream((int)sz);
                            cache = File.Create(cacheFile);
                        }
                        _txrx.Send(String.Format("pb 2 max {0}", sz));
                        _txrx.Send("pb 2 value 0");
                        var buffer = new byte[8192];
                        int n;
                        Int64 totalWritten = 0;
                        int nCycles = 0;
                        using (input)
                        {
                            while ((n= input.Read(buffer,0, buffer.Length))>0)
                            {
                                stream.Write(buffer,0,n);
                                if (cache!=null)
                                    cache.Write(buffer,0,n);
                                totalWritten += n;
                                // for performance, don't update the
                                // progress monitor every time.
                                nCycles++;
                                if (nCycles % 312 == 0)
                                {
                                    _txrx.Send(String.Format("pb 2 value {0}", totalWritten));
                                    _txrx.Send(String.Format("status Saving entry {0}/{1} {2} :: {3}/{4}mb {5:N0}%",
                                                             numSaving, totalToSave,
                                                             name,
                                                             totalWritten/(1024*1024), sz/(1024*1024),
                                                             ((double)totalWritten) / (0.01 * sz)));
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (cache!=null) cache.Dispose();
                    }
                };

            EventHandler<SaveProgressEventArgs> sp = (sender1, e1) =>
                {
                    switch (e1.EventType)
                    {
                        case ZipProgressEventType.Saving_Started:
                        numSaving= 0;
                        break;

                        case ZipProgressEventType.Saving_BeforeWriteEntry:
                        _txrx.Send("test Large Segmented Zip");
                        _txrx.Send(String.Format("status saving {0}", e1.CurrentEntry.FileName));
                        totalToSave = e1.EntriesTotal;
                        numSaving++;
                        break;

                        // case ZipProgressEventType.Saving_EntryBytesRead:
                        // if (!_pb2Set)
                        // {
                        //     _txrx.Send(String.Format("pb 2 max {0}", e1.TotalBytesToTransfer));
                        //     _pb2Set = true;
                        // }
                        // _txrx.Send(String.Format("status Saving entry {0}/{1} {2} :: {3}/{4}mb {5:N0}%",
                        //                          numSaving, totalToSave,
                        //                          e1.CurrentEntry.FileName,
                        //                          e1.BytesTransferred/(1024*1024), e1.TotalBytesToTransfer/(1024*1024),
                        //                          ((double)e1.BytesTransferred) / (0.01 * e1.TotalBytesToTransfer)));
                        // string msg = String.Format("pb 2 value {0}", e1.BytesTransferred);
                        // _txrx.Send(msg);
                        // break;

                        case ZipProgressEventType.Saving_AfterWriteEntry:
                        TestContext.WriteLine("Saved entry {0}, {1} bytes", e1.CurrentEntry.FileName,
                                              e1.CurrentEntry.UncompressedSize);
                        _txrx.Send("pb 1 step");
                        _pb2Set = false;
                        break;
                    }
                };

            _txrx = TestUtilities.StartProgressMonitor("largesegmentedzip", "Large Segmented ZIP", "Creating files");

            _txrx.Send("bars 3");
            _txrx.Send("pb 0 max 2");
            _txrx.Send(String.Format("pb 1 max {0}", numFiles));

            // build a large zip file out of thin air
            var sw = new StringWriter();
            using (ZipFile zip = new ZipFile())
            {
                zip.StatusMessageTextWriter = sw;
                zip.BufferSize = 256 * 1024;
                zip.CodecBufferSize = 128 * 1024;
                zip.MaxOutputSegmentSize = maxSegSize;
                zip.SaveProgress += sp;

                for (int i = 0; i < numFiles; i++)
                {
                    string filename = TestUtilities.GetOneRandomUppercaseAsciiChar() +
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".txt";
                    zip.AddEntry(filename, writer);
                }
                zip.Save(zipFileToCreate);

                numSegs = zip.NumberOfSegmentsForMostRecentSave;
            }

#if REMOTE_FILESYSTEM
            if (((long)numSegs*maxSegSize) < (long)(1024*1024*1024L))
            {
                _FilesToRemove.Remove(parentDir);
                Assert.IsTrue(false, "There were not enough segments in that zip.  numsegs({0}) maxsize({1}).", numSegs, maxSegSize);
            }
#endif
            _txrx.Send("status Verifying the zip ...");

            _txrx.Send("pb 0 step");
            _txrx.Send("pb 1 value 0");
            _txrx.Send("pb 2 value 0");

            ReadOptions options = new ReadOptions
            {
                StatusMessageWriter = new StringWriter()
            };

            string extractDir = "verify";
            int c = 0;
            while (Directory.Exists(extractDir + c)) c++;
            extractDir += c;

            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate, options))
            {
                _numFilesToExtract = zip2.Entries.Count;
                _numExtracted= 1;
                _pb1Set= false;
                zip2.ExtractProgress += ExtractProgress;
                zip2.ExtractAll(extractDir);
            }

            string status = options.StatusMessageWriter.ToString();
            TestContext.WriteLine("status:");
            foreach (string line in status.Split('\n'))
                TestContext.WriteLine(line);
        }



        [TestMethod]
        [ExpectedException(typeof(Ionic.Zip.ZipException))]
        public void Spanned_InvalidSegmentSize()
        {
            string zipFileToCreate = "InvalidSegmentSize.zip";
            int segSize = 65536/3 + _rnd.Next(65536/2);
            using (var zip = new ZipFile())
            {
                zip.MaxOutputSegmentSize = segSize;
                zip.Save(zipFileToCreate);
            }
        }



        string _fodderDir = null;
        /// <summary>
        ///   Finds or creates and fills a cache directory of fodder files.
        /// </summary>
        /// <returns>the name of the cache directory</returns>
        String CreateSomeFiles()
        {
            string fodderName = "fodder";
            if (_fodderDir != null)
            {
                CreateSymbolicLink(fodderName, _fodderDir, 1);
                return fodderName;
            }

            int baseNumFiles = 12;
            int baseSize = 0x100000;
            int numFilesToAdd = baseNumFiles + _rnd.Next(4);
            string tmpDir = System.Environment.GetEnvironmentVariable("TEMP");
            var oldDirs = Directory.GetDirectories(tmpDir, "*.SplitArchives");
            string fodderDir;
            foreach (var dir in oldDirs)
            {
                TestContext.WriteLine("Considering directory: {0}", dir);
                fodderDir = Path.Combine(dir, fodderName);
                if (!Directory.Exists(fodderDir))
                {
                    Directory.Delete(dir, true);
                }
                else
                {
                    var fodderFiles = Directory.GetFiles(fodderDir, "*.txt");
                    if (fodderFiles.Length < baseNumFiles)
                    {
                        Directory.Delete(dir, true);
                    }
                    else
                    {
                        _fodderDir = fodderDir;
                        CreateSymbolicLink(fodderName, _fodderDir, 1);
                        return fodderName;
                    }

                }
            }

            // upon reaching here, no directories exist that contain suitable
            // fodder for these tests.  Create the directory, and a few
            // fodder files.

            string pname = Path.GetFileName(TestUtilities.GenerateUniquePathname("SplitArchives"));

            string cacheDir = Path.Combine(tmpDir, pname);
            Directory.CreateDirectory(cacheDir);
            fodderDir = Path.Combine(cacheDir, fodderName);
            Directory.CreateDirectory(fodderDir);

            for (int i=0; i < numFilesToAdd; i++)
            {
                int fileSize = baseSize + _rnd.Next(baseSize/2);
                string fileName = Path.Combine(fodderDir, string.Format("Pippo{0}.txt", i));
                TestUtilities.CreateAndFillFileText(fileName, fileSize);
            }
            _fodderDir = fodderDir;
            CreateSymbolicLink(fodderName, _fodderDir, 1);
            return fodderName;
        }



        [TestMethod]
        [Timeout(5 * 60*1000)] // to protect against stuck file locks
        public void Spanned_Resave_wi13915()
        {
            TestContext.WriteLine("Creating fodder files... {0}",
                                  DateTime.Now.ToString("G"));
            var contentDir = CreateSomeFiles();
            var filesToAdd = new List<String>(Directory.GetFiles(contentDir));
            int[] segSizes = { 128, 256, 512 };

            // Three passes:
            // pass 1: save as regular, then resave as segmented.
            // pass 2: save as segmented, then resave as regular.
            // pass 3: save as segmented, then resave as another segmented archive.
            for (int m=0; m < 3; m++)
            {
                // for various segment sizes
                for (int k=0; k < segSizes.Length; k++)
                {
                    string trialDir = String.Format("trial.{0}.{1}", m, k);
                    Directory.CreateDirectory(trialDir);
                    string zipFile1 = Path.Combine(trialDir, "InitialSave."+m+"."+k+".zip");
                    string zipFile2 = Path.Combine(trialDir, "Updated."+m+"."+k+".zip");
                    TestContext.WriteLine("");
                    TestContext.WriteLine("Creating zip... T({0},{1})...{2}",
                                          m, k, DateTime.Now.ToString("G"));
                    using (var zip1 = new ZipFile())
                    {
                        zip1.AddFiles(filesToAdd, "");
                        if (m!=0)
                            zip1.MaxOutputSegmentSize = segSizes[k] * 1024;
                        zip1.Save(zipFile1);
                    }

                    TestContext.WriteLine("");
                    TestContext.WriteLine("Re-saving...");
                    using (var zip2 = ZipFile.Read(zipFile1))
                    {
                        if (m==0)
                            zip2.MaxOutputSegmentSize = segSizes[k] * 1024;
                        else if (m==2)
                            zip2.MaxOutputSegmentSize = 1024 * (3 * segSizes[k])/2;

                        zip2.Save(zipFile2);
                    }

                    TestContext.WriteLine("");
                    TestContext.WriteLine("Extracting...");
                    string extractDir = Path.Combine(trialDir,"extract");
                    Directory.CreateDirectory(extractDir);
                    using (var zip3 = ZipFile.Read(zipFile2))
                    {
                        foreach (var e in zip3)
                        {
                            TestContext.WriteLine(" {0}", e.FileName);
                            e.Extract(extractDir);
                        }
                    }

                    string[] filesUnzipped = Directory.GetFiles(extractDir);
                    Assert.AreEqual<int>(filesToAdd.Count, filesUnzipped.Length,
                                         "Incorrect number of files extracted.");
                }
            }
        }





        [TestMethod]
        [Timeout(5 * 60*1000)] // to protect against stuck file locks
        public void Spanned_WinZip_Unzip_wi13691()
        {
            if (!WinZipIsPresent)
                throw new Exception("winzip is not present");

            TestContext.WriteLine("Creating fodder files... {0}",
                                  DateTime.Now.ToString("G"));
            var contentDir = CreateSomeFiles();
            var filesToAdd = new List<String>(Directory.GetFiles(contentDir));
            int[] segSizes = { 128, 256, 512 };

            // Save as segmented, then read/extract with winzip unzip
            // for various segment sizes.
            for (int k=0; k < segSizes.Length; k++)
            {
                string trialDir = String.Format("trial.{0}", k);
                Directory.CreateDirectory(trialDir);
                string zipFile1 = Path.Combine(trialDir, "InitialSave."+k+".zip");
                TestContext.WriteLine("");
                TestContext.WriteLine("Creating zip... T({0})...{1}",
                                      k, DateTime.Now.ToString("G"));

                using (var zip1 = new ZipFile())
                {
                    zip1.AddFiles(filesToAdd, "");
                    zip1.MaxOutputSegmentSize = segSizes[k] * 1024;
                    zip1.Save(zipFile1);
                }

                TestContext.WriteLine("");
                TestContext.WriteLine("Extracting...");
                string extractDir = Path.Combine(trialDir,"extract");
                Directory.CreateDirectory(extractDir);
                string args = String.Format("-d -yx {0} \"{1}\"",
                                            zipFile1, extractDir);
                this.Exec(wzunzip, args);

                string[] filesUnzipped = Directory.GetFiles(extractDir);
                Assert.AreEqual<int>(filesToAdd.Count, filesUnzipped.Length,
                                     "Incorrect number of files extracted, trail {0}", k);
            }
        }


#if INFOZIP_UNZIP_SUPPORTS_SPLIT_ARCHIVES

        [TestMethod]
        [Timeout(5 * 60*1000)] // to protect against stuck file locks
        public void Spanned_InfoZip_Unzip_wi13691()
        {
            if (!InfoZipIsPresent)
                throw new Exception("InfoZip is not present");

            TestContext.WriteLine("Creating fodder files... {0}",
                                  DateTime.Now.ToString("G"));
            var contentDir = CreateSomeFiles();
            var filesToAdd = new List<String>(Directory.GetFiles(contentDir));
            int[] segSizes = { 128, 256, 512 };

            // Save as segmented, then read/extract with winzip unzip
            // for various segment sizes.
            for (int k=0; k < segSizes.Length; k++)
            {
                string trialDir = String.Format("trial.{0}", k);
                Directory.CreateDirectory(trialDir);
                string zipFile1 = Path.Combine(trialDir, "InitialSave."+k+".zip");
                TestContext.WriteLine("");
                TestContext.WriteLine("Creating zip... T({0})...{1}",
                                      k, DateTime.Now.ToString("G"));
                using (var zip1 = new ZipFile())
                {
                    zip1.AddFiles(filesToAdd, "");
                    zip1.MaxOutputSegmentSize = segSizes[k] * 1024;
                    zip1.Save(zipFile1);
                }

                TestContext.WriteLine("");
                TestContext.WriteLine("Extracting...");
                string extractDir = Path.Combine(trialDir,"extract");
                Directory.CreateDirectory(extractDir);
                string args = String.Format("{0} -d {1}",
                                            zipFile1,
                                            extractDir);
                this.Exec(infoZipUnzip, args);

                string[] filesUnzipped = Directory.GetFiles(extractDir);
                Assert.AreEqual<int>(filesToAdd.Count, filesUnzipped.Length,
                                     "Incorrect number of files extracted, trail {0}", k);
            }
        }
#endif


        [TestMethod]
        [Timeout(5 * 60*1000)] // to protect against stuck file locks
        public void Spanned_WinZip_Zip_wi13691()
        {
            if (!WinZipIsPresent)
                throw new Exception("WinZip is not present");

            TestContext.WriteLine("Creating fodder files... {0}",
                                  DateTime.Now.ToString("G"));
            var contentDir = CreateSomeFiles();
            var filesToAdd = new List<String>(Directory.GetFiles(contentDir));
            int[] segSizes = { 128, 256, 512 };

            // Save as segmented, then read/extract with winzip unzip
            // for various segment sizes.
            for (int k=0; k < segSizes.Length; k++)
            {
                string trialDir = String.Format("trial.{0}", k);
                Directory.CreateDirectory(trialDir);
                string nameOfParts = Path.Combine(trialDir, "InitialSave."+k);
                string zipFile1 = nameOfParts + ".zip";
                TestContext.WriteLine("");
                TestContext.WriteLine("Creating zip... T({0})...{1}",
                                      k, DateTime.Now.ToString("G"));

                // with WinZip, must produce a segmented zip in two
                // steps: first create the regular zip, then split it.

                // step 1: create the regular zip
                string args = String.Format("-a -p -r -yx step1.zip \"{0}\"", contentDir);
                string wzzipOut = this.Exec(wzzip, args);

                // step 2: split the existing zip
                // "wzzip -ys[size] archivetosplit.zip nameofparts "
                args = String.Format("-ys{0} step1.zip {1}",
                                            segSizes[k], zipFile1 //nameOfParts
                                     );
                wzzipOut = this.Exec(wzzip, args);


                TestContext.WriteLine("");
                TestContext.WriteLine("Extracting...");
                string extractDir = Path.Combine(trialDir,"extract");
                Directory.CreateDirectory(extractDir);
                using (var zip1 = ZipFile.Read(zipFile1))
                {
                    foreach (var e in zip1)
                    {
                        TestContext.WriteLine(" {0}", e.FileName);
                        e.Extract(extractDir);
                    }
                }

                string[] filesUnzipped = Directory.GetFiles(extractDir);
                Assert.AreEqual<int>(filesToAdd.Count, filesUnzipped.Length,
                                     "Incorrect number of files extracted, trail {0}", k);
            }
        }



        [TestMethod]
        [Timeout(5 * 60*1000)] // to protect against stuck file locks
        public void Spanned_InfoZip_Zip_wi13691()
        {
            if (!InfoZipIsPresent)
                throw new Exception("InfoZip is not present");

            TestContext.WriteLine("Creating fodder files... {0}",
                                  DateTime.Now.ToString("G"));
            var contentDir = CreateSomeFiles();
            var filesToAdd = new List<String>(Directory.GetFiles(contentDir));
            int[] segSizes = { 128, 256, 512 };

            // Save as segmented, then read/extract with winzip unzip
            // for various segment sizes.
            for (int k=0; k < segSizes.Length; k++)
            {
                string trialDir = String.Format("trial.{0}", k);
                Directory.CreateDirectory(trialDir);
                string zipFile1 = Path.Combine(trialDir, "InitialSave."+k+".zip");
                TestContext.WriteLine("");
                TestContext.WriteLine("Creating zip... T({0})...{1}",
                                      k, DateTime.Now.ToString("G"));

                string args = String.Format("-s {0}k {1} -r {2}",
                                            segSizes[k],
                                            zipFile1,
                                            contentDir);
                this.Exec(infoZip, args);

                TestContext.WriteLine("");
                TestContext.WriteLine("Extracting...");
                string extractDir = Path.Combine(trialDir,"extract");
                Directory.CreateDirectory(extractDir);
                using (var zip1 = ZipFile.Read(zipFile1))
                {
                    foreach (var e in zip1)
                    {
                        TestContext.WriteLine(" {0}", e.FileName);
                        e.Extract(extractDir);
                    }
                }

                string[] filesUnzipped =
                    Directory.GetFiles(Path.Combine(extractDir, contentDir));
                Assert.AreEqual<int>(filesToAdd.Count, filesUnzipped.Length,
                                     "Incorrect number of files extracted, trail {0}", k);
            }

        }


    }

}
