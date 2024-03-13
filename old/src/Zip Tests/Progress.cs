// Progress.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.
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
// Time-stamp: <2010-January-21 11:14:35>
//
// ------------------------------------------------------------------
//
// This module defines the tests for progress events in DotNetZip.
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
    public class Progress : IonicTestClass
    {
        public Progress() : base() { }

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


        void ReadProgress1(object sender, ReadProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Reading_Started:
                    TestContext.WriteLine("Reading_Started");
                    break;
                case ZipProgressEventType.Reading_Completed:
                    TestContext.WriteLine("Reading_Completed");
                    break;
                case ZipProgressEventType.Reading_BeforeReadEntry:
                    TestContext.WriteLine("Reading_BeforeReadEntry");
                    break;
                case ZipProgressEventType.Reading_AfterReadEntry:
                    TestContext.WriteLine("Reading_AfterReadEntry: {0}",
                                          e.CurrentEntry.FileName);
                    break;
                case ZipProgressEventType.Reading_ArchiveBytesRead:
                    break;
            }
        }



        [TestMethod]
        public void Progress_ReadFile()
        {
            Directory.SetCurrentDirectory(TopLevelDir);
            string  zipFileToCreate = Path.Combine(TopLevelDir, "Progress_ReadFile.zip");
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddFiles(files);
                zip.Save(zipFileToCreate);
            }

            int count = TestUtilities.CountEntries(zipFileToCreate);
            Assert.IsTrue(count>0);

            var options = new ReadOptions {
                    StatusMessageWriter = new StringWriter(),
                    ReadProgress = ReadProgress1
            };
            using (ZipFile zip = ZipFile.Read(zipFileToCreate, options))
            {
                // this should be fine
                zip.RemoveEntry(zip[1]);
                zip.Save();
            }
            TestContext.WriteLine(options.StatusMessageWriter.ToString());
            Assert.AreEqual<Int32>(count, TestUtilities.CountEntries(zipFileToCreate)+1);
        }


        void AddProgress1(object sender, AddProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Adding_Started:
                    TestContext.WriteLine("Adding_Started");
                    break;
                case ZipProgressEventType.Adding_Completed:
                    TestContext.WriteLine("Adding_Completed");
                    break;
                case ZipProgressEventType.Adding_AfterAddEntry:
                    TestContext.WriteLine("Adding_AfterAddEntry: {0}",
                                          e.CurrentEntry.FileName);
                    break;
            }
        }


        [TestMethod]
        public void Progress_AddFiles()
        {
            Directory.SetCurrentDirectory(TopLevelDir);
            string  zipFileToCreate = Path.Combine(TopLevelDir, "Progress_AddFiles.zip");
            string dirToZip = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            var files = TestUtilities.GenerateFilesFlat(dirToZip);

            var sw = new StringWriter();
            using (ZipFile zip = new ZipFile(zipFileToCreate, sw))
            {
                zip.AddProgress += AddProgress1;
                zip.AddFiles(files);
                zip.Save();
            }
            TestContext.WriteLine(sw.ToString());

            int count = TestUtilities.CountEntries(zipFileToCreate);
            Assert.AreEqual<Int32>(count, files.Length);
        }

    }


}
