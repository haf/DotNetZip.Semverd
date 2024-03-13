// LongRunning.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2011 Dino Chiesa
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
// Time-stamp: <2011-June-19 21:51:30>
//
// ------------------------------------------------------------------
//
// This module some long-running unit tests for DotNetZip: tests for
// saving very large numbers of files, very large (multi-GB) files, and so on.
//
// ------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RE = System.Text.RegularExpressions;

using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;
using System.IO;

namespace Ionic.Zip.Tests.LongRunning
{
    /// <summary>
    /// Summary description for LongRunning
    /// </summary>
    [TestClass]
    public class LongRunning : IonicTestClass
    {
        Int64 maxBytesXferred = 0;
        bool _pb1Set;
        bool _pb2Set;
        int _numEntriesSaved = 0;
        int _numEntriesToAdd = 0;
        int _numEntriesAdded = 0;
        int _numFilesToExtract;
        int _spCycles;
        int _epCycles;


        void LNSF_SaveProgress(object sender, Ionic.Zip.SaveProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Saving_Started:
                    _numEntriesSaved = 0;
                    _txrx.Send("status saving started...");
                    _pb1Set = false;
                    break;

                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    _numEntriesSaved++;
                    if (_numEntriesSaved % 64 == 0)
                        _txrx.Send(String.Format("status Compressing {0}", e.CurrentEntry.FileName));
                    if (!_pb1Set)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", e.EntriesTotal));
                        _pb1Set = true;
                    }
                    break;

                case ZipProgressEventType.Saving_EntryBytesRead:
                    Assert.IsTrue(e.BytesTransferred <= e.TotalBytesToTransfer);
                    break;

                case ZipProgressEventType.Saving_AfterWriteEntry:
                    _txrx.Send("pb 1 step");
                    break;

                case ZipProgressEventType.Saving_Completed:
                    _txrx.Send("status Save completed");
                    _pb1Set = false;
                    _txrx.Send("pb 1 max 1");
                    _txrx.Send("pb 1 value 1");
                    break;
            }
        }


        void LNSF_AddProgress(object sender, AddProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Adding_Started:
                    _txrx.Send("status Adding files to the zip...");
                    _pb1Set = false;
                    break;

                case ZipProgressEventType.Adding_AfterAddEntry:
                    if (!_pb1Set)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", _numEntriesToAdd));
                        _pb1Set = true;
                    }
                    if (!e.CurrentEntry.FileName.EndsWith("/"))
                    {
                        _numEntriesAdded++;
                        if (_numEntriesAdded % 64 == 0)
                            _txrx.Send(String.Format("status Adding file {0}/{1} :: {2}",
                                                     _numEntriesAdded, _numEntriesToAdd, e.CurrentEntry.FileName));
                        _txrx.Send("pb 1 step");
                    }
                    break;

                case ZipProgressEventType.Adding_Completed:
                    _txrx.Send("status Added all files");
                    _pb1Set = false;
                    _txrx.Send("pb 1 max 1");
                    _txrx.Send("pb 1 value 1");
                    break;
            }
        }




        [TestMethod, Timeout(120 * 60 * 1000)]
        public void CreateZip_AddDirectory_LargeNumberOfSmallFiles()
        {
            // start the visible progress monitor
            _txrx = TestUtilities.StartProgressMonitor("LargeNumberOfSmallFiles",
                                                       "Large # of Small Files",
                                                       "Creating files");
            int max1 = 0;
            Action<Int16, Int32> progressUpdate = (x, y) => {
                if (x == 0)
                {
                    _txrx.Send(String.Format("pb 1 max {0}", y));
                    max1 = y;
                }
                else if (x == 2)
                {
                    _txrx.Send(String.Format("pb 1 value {0}", y));
                    _txrx.Send(String.Format("status creating directory {0} of {1}",
                                             y, max1));
                }
                else if (x == 4)
                {
                    _txrx.Send(String.Format("status done creating {0} files", y));
                }
            };

            int[][] settings = {
                // sizes and numbers of files/directories to create
                new int[] {71, 21, 97, 27, 200, 200 },
                new int[] {51, 171, 47, 197, 100, 100 },
            };
            _txrx.Send(String.Format("pb 0 max {0}", settings.Length * 2));
            TestContext.WriteLine("============================================");
            TestContext.WriteLine("Test beginning - {0}", DateTime.Now.ToString("G"));
            for (int m = 0; m < settings.Length; m++)
            {
                string zipFileToCreate = String.Format("LrgNumOfSmallFiles-{0}.zip", m);
                string dirToZip =  "zipthis" + m;
                Directory.CreateDirectory(dirToZip);
                TestContext.WriteLine("============================================");
                TestContext.WriteLine("Creating files, cycle {0}...", m);

                int subdirCount = 0;
                int entries =
                    TestUtilities.GenerateFilesOneLevelDeep(TestContext,
                                                            "LargeNumberOfFiles",
                                                            dirToZip,
                                                            settings[m],
                                                            progressUpdate,
                                                            out subdirCount);
                _numEntriesToAdd = entries;  // used in LNSF_AddProgress
                _numEntriesAdded = 0;

                _txrx.Send("pb 0 step");
                TestContext.WriteLine("============================================");
                TestContext.WriteLine("Total of {0} files in {1} subdirs",
                                      entries, subdirCount);
                TestContext.WriteLine("Creating zip - {0}", DateTime.Now.ToString("G"));
                Directory.SetCurrentDirectory(TopLevelDir);
                _pb1Set = false;
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddProgress += LNSF_AddProgress;
                    zip.AddDirectory(Path.GetFileName(dirToZip));
                    _txrx.Send("test Large # of Small Files"); // for good measure
                    zip.BufferSize = 4096;
                    zip.SortEntriesBeforeSaving = true;
                    zip.SaveProgress += LNSF_SaveProgress;
                    zip.Save(zipFileToCreate);
                }

                _txrx.Send("pb 0 step");

                TestContext.WriteLine("Checking zip - {0}", DateTime.Now.ToString("G"));
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), entries);

                _txrx.Send("status cleaning up...");
                // clean up for this cycle
                Directory.Delete(dirToZip, true);
            }
            TestContext.WriteLine("============================================");
            TestContext.WriteLine("Test end - {0}", DateTime.Now.ToString("G"));
        }



        void LF_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            string msg;
            switch (e.EventType)
            {
                case ZipProgressEventType.Saving_Started:
                    _txrx.Send("status saving started...");
                    _pb1Set = false;
                    //_txrx.Send(String.Format("pb1 max {0}", e.EntriesTotal));
                    //_txrx.Send("pb2 max 1");
                    break;

                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    _txrx.Send(String.Format("status Compressing {0}", e.CurrentEntry.FileName));
                    if (!_pb1Set)
                    {
                        _txrx.Send(String.Format("pb 1 max {0}", e.EntriesTotal));
                        _pb1Set = true;
                    }
                    _pb2Set = false;
                    _spCycles = 0;
                    break;

                case ZipProgressEventType.Saving_EntryBytesRead:
                    _spCycles++;
                    if ((_spCycles % 32) == 0)
                    {
                        if (!_pb2Set)
                        {
                            _txrx.Send(String.Format("pb 2 max {0}", e.TotalBytesToTransfer));
                            _pb2Set = true;
                        }
                        _txrx.Send(String.Format("status Saving {0} :: [{1}/{2}mb] ({3:N0}%)",
                                                 e.CurrentEntry.FileName,
                                                 e.BytesTransferred / (1024 * 1024), e.TotalBytesToTransfer / (1024 * 1024),
                                                 ((double)e.BytesTransferred) / (0.01 * e.TotalBytesToTransfer)
                                                 ));
                        msg = String.Format("pb 2 value {0}", e.BytesTransferred);
                        _txrx.Send(msg);
                        Assert.IsTrue(e.BytesTransferred <= e.TotalBytesToTransfer);
                    }
                    if (maxBytesXferred < e.BytesTransferred)
                        maxBytesXferred = e.BytesTransferred;
                    break;

                case ZipProgressEventType.Saving_AfterWriteEntry:
                    _txrx.Send("pb 1 step");
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



        void LF_ExtractProgress(object sender, ExtractProgressEventArgs e)
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
                    _epCycles=0;
                    break;

                case ZipProgressEventType.Extracting_EntryBytesWritten:
                    _epCycles++;
                    if ((_epCycles % 32) == 0)
                    {
                        if (!_pb2Set)
                        {
                            _txrx.Send(String.Format("pb 2 max {0}", e.TotalBytesToTransfer));
                            _pb2Set = true;
                        }
                        _txrx.Send(String.Format("status Extracting {0} :: [{1}/{2}mb] ({3:N0}%)",
                                                 e.CurrentEntry.FileName,
                                                 e.BytesTransferred/(1024*1024),
                                                 e.TotalBytesToTransfer/(1024*1024),
                                                 ((double)e.BytesTransferred / (0.01 * e.TotalBytesToTransfer))
                                                 ));
                        string msg = String.Format("pb 2 value {0}", e.BytesTransferred);
                        _txrx.Send(msg);
                    }
                        if (maxBytesXferred < e.BytesTransferred)
                            maxBytesXferred = e.BytesTransferred;
                    break;

                case ZipProgressEventType.Extracting_AfterExtractEntry:
                    _txrx.Send("pb 1 step");
                    break;
            }
        }


        void InjectNoise(string fileName)
        {
            var finfo = new FileInfo(fileName);
            var flen = finfo.Length;
            var segmentSize = flen/128;
            var bytes = new byte[Math.Min(segmentSize,128)];
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                while (fs.Position < flen - segmentSize - 8)
                {
                var t = _rnd.Next((int)segmentSize) + 8;
                fs.Seek(t, SeekOrigin.Current);
                _rnd.NextBytes(bytes);
                fs.Write(bytes, 0, bytes.Length/2+_rnd.Next(bytes.Length/2));
                }
            }
        }



        [TestMethod, Timeout(60 * 60 * 1000)]
        public void LargeFile_WithProgress()
        {
            // This test checks the Int64 limits in progress events (Save + Extract)
            TestContext.WriteLine("Test beginning {0}", System.DateTime.Now.ToString("G"));

            _txrx = TestUtilities.StartProgressMonitor("LargeFile_WithProgress",
                                                       "Large File Save and Extract",
                                                       "Creating a large file...");

            _txrx.Send("bars 3");
            System.Threading.Thread.Sleep(120);
            _txrx.Send("pb 0 max 3");

            string zipFileToCreate = "LargeFile_WithProgress.zip";
            string unpackDir = "unpack";
            string dirToZip = "LargeFile";
            string baseName = "LargeFile.bin";
            Directory.CreateDirectory(dirToZip);

            Int64 minFileSize = 0x7FFFFFFFL + _rnd.Next(1000000);
            TestContext.WriteLine("Creating a large file, size>={0}", minFileSize);
            string filename = Path.Combine(dirToZip, baseName);
            _txrx.Send(String.Format("pb 1 max {0}", minFileSize));

            Action<Int64> progressUpdate = (x) =>
                {
                    _txrx.Send(String.Format("pb 1 value {0}", x));
                    _txrx.Send(String.Format("status Creating a large file, ({0}/{1}mb) ({2:N0}%)",
                                             x / (1024 * 1024), minFileSize / (1024 * 1024),
                                             ((double)x) / (0.01 * minFileSize)));
                };

            // this will take about a minute
            TestUtilities.CreateAndFillFileBinaryZeroes(filename, minFileSize, progressUpdate);

            InjectNoise(filename);

            _txrx.Send("pb 0 step");
            var finfo = new FileInfo(filename);
            var actualFileSize = finfo.Length;

            TestContext.WriteLine("File Create complete {0}", System.DateTime.Now.ToString("G"));

            maxBytesXferred = 0;
            using (ZipFile zip1 = new ZipFile())
            {
                zip1.SaveProgress += LF_SaveProgress;
                zip1.Comment = "This is the comment on the zip archive.";
                zip1.AddEntry("Readme.txt", "This is some content.");
                zip1.AddDirectory(dirToZip, dirToZip);
                zip1.BufferSize = 65536 * 8;      // 512k
                zip1.CodecBufferSize = 65536 * 2; // 128k
                zip1.Save(zipFileToCreate);
            }

            _txrx.Send("pb 0 step");
            TestContext.WriteLine("Save complete {0}", System.DateTime.Now.ToString("G"));
            Assert.AreEqual<Int64>(actualFileSize, maxBytesXferred);
            var chk1 = TestUtilities.ComputeChecksum(filename);

            // remove the large file before extracting
            Directory.Delete(dirToZip, true);

            _pb1Set = _pb2Set = false;
            maxBytesXferred = 0;
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
            {
                _numFilesToExtract = zip2.Entries.Count;
                zip2.ExtractProgress += LF_ExtractProgress;
                zip2.BufferSize = 65536 * 8;
                zip2.ExtractAll(unpackDir);
            }

            _txrx.Send("pb 0 step");

            TestContext.WriteLine("Extract complete {0}", System.DateTime.Now.ToString("G"));
            Assert.AreEqual<Int64>(actualFileSize, maxBytesXferred);
            var exFile = Path.Combine(unpackDir, Path.Combine(dirToZip, baseName));
            var chk2 = TestUtilities.ComputeChecksum(exFile);

            string s1 = TestUtilities.CheckSumToString(chk1);
            string s2 = TestUtilities.CheckSumToString(chk2);
            Assert.AreEqual<string>(s1,s2);
            TestContext.WriteLine("     Checksums match ({0}).\n", s2);
            TestContext.WriteLine("Test complete {0}", System.DateTime.Now.ToString("G"));
        }


   }

}