// IonicTestClass.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa.
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
// Time-stamp: <2011-July-26 10:04:54>
//
// ------------------------------------------------------------------
//
// This module defines the base class for DotNetZip test classes.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Linq;
using System.IO;
using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ionic.Zip.Tests.Utilities
{
    [TestClass]
    public class IonicTestClass
    {
        protected System.Random _rnd;
        protected System.Collections.Generic.List<string> _FilesToRemove;
        protected static string CurrentDir = null;
        protected string TopLevelDir = null;
        private string _wzunzip = null;
        private string _wzzip = null;
        private string _sevenzip = null;
        private string _zipit = null;
        private string _infozipzip = null;
        private string _infozipunzip = null;
        private bool? _ZipitIsPresent;
        private bool? _WinZipIsPresent;
        private bool? _SevenZipIsPresent;
        private bool? _InfoZipIsPresent;

        protected Ionic.CopyData.Transceiver _txrx;


        public IonicTestClass()
        {
            _rnd = new System.Random();
            _FilesToRemove = new System.Collections.Generic.List<string>();
        }

        #region Context
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion



        #region Test Init and Cleanup
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void BaseClassInitialize(TestContext testContext)
        {
            CurrentDir = Directory.GetCurrentDirectory();
            Assert.AreNotEqual<string>(Path.GetFileName(CurrentDir), "Temp", "at startup");
        }

        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //


        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            if (CurrentDir == null) CurrentDir = Directory.GetCurrentDirectory();
            TestUtilities.Initialize(out TopLevelDir);
            _FilesToRemove.Add(TopLevelDir);
            Directory.SetCurrentDirectory(TopLevelDir);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            // The CWD of the monitoring process is the CurrentDir,
            // therefore this test must shut down the monitoring process
            // FIRST, to allow the deletion of the directory.
            if (_txrx!=null)
            {
                try
                {
                    _txrx.Send("stop");
                    _txrx = null;
                }
                catch { }
            }

            TestUtilities.Cleanup(CurrentDir, _FilesToRemove);
        }

        #endregion



        internal string Exec(string program, string args)
        {
            return Exec(program, args, true);
        }

        internal string Exec(string program, string args, bool waitForExit)
        {
            return Exec(program, args, waitForExit, true);
        }

        internal string Exec(string program, string args, bool waitForExit, bool emitOutput)
        {
            if (program == null)
                throw new ArgumentException("program");

            if (args == null)
                throw new ArgumentException("args");

            // Microsoft.VisualStudio.TestTools.UnitTesting
            this.TestContext.WriteLine("running command: {0} {1}", program, args);

            string output;
            int rc = TestUtilities.Exec_NoContext(program, args, waitForExit, out output);

            if (rc != 0)
                throw new Exception(String.Format("Non-zero RC {0}: {1}", program, output));

            if (emitOutput)
                this.TestContext.WriteLine("output: {0}", output);
            else
                this.TestContext.WriteLine("A-OK. (output suppressed)");

            return output;
        }


        public class AsyncReadState
        {
            public System.IO.Stream s;
            public byte[] buf= new byte[1024];
        }


        internal int ExecRedirectStdOut(string program, string args, string outFile)
        {
            if (program == null)
                throw new ArgumentException("program");

            if (args == null)
                throw new ArgumentException("args");

            this.TestContext.WriteLine("running command: {0} {1}", program, args);

            Stream fs = File.Create(outFile);
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        FileName = program,
                        CreateNoWindow = true,
                        Arguments = args,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                };

                p.Start();

                var stdout = p.StandardOutput.BaseStream;
                var rs = new AsyncReadState { s = stdout };
                Action<System.IAsyncResult> readAsync1 = null;
                var readAsync = new Action<System.IAsyncResult>( (ar) => {
                        AsyncReadState state = (AsyncReadState) ar.AsyncState;
                        int n = state.s.EndRead(ar);
                        if (n > 0)
                        {
                            fs.Write(state.buf, 0, n);
                            state.s.BeginRead(state.buf,
                                              0,
                                              state.buf.Length,
                                              new System.AsyncCallback(readAsync1),
                                              state);
                        }
                    });
                readAsync1 = readAsync; // ??

                // kickoff
                stdout.BeginRead(rs.buf,
                                 0,
                                 rs.buf.Length,
                                 new System.AsyncCallback(readAsync),
                                 rs);

                p.WaitForExit();

                this.TestContext.WriteLine("Process exited, rc={0}", p.ExitCode);

                return p.ExitCode;
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
        }


        protected string sevenZip
        {
            get { return SevenZipIsPresent ? _sevenzip : null; }
        }

        protected string zipit
        {
            get { return ZipitIsPresent ? _zipit : null; }
        }

        protected string infoZip
        {
            get { return InfoZipIsPresent ? _infozipzip : null; }
        }

        protected string infoZipUnzip
        {
            get { return InfoZipIsPresent ? _infozipunzip : null; }
        }

        protected string wzzip
        {
            get { return WinZipIsPresent ? _wzzip : null; }
        }

        protected string wzunzip
        {
            get { return WinZipIsPresent ? _wzunzip : null; }
        }

        protected bool ZipitIsPresent
        {
            get
            {
                if (_ZipitIsPresent == null)
                {
                    string sourceDir = CurrentDir;
                    for (int i = 0; i < 3; i++)
                        sourceDir = Path.GetDirectoryName(sourceDir);

                    _zipit =
                        Path.Combine(sourceDir, "Tools\\Zipit\\bin\\Debug\\Zipit.exe");

                    _ZipitIsPresent = new Nullable<bool>(File.Exists(_zipit));
                }
                return _ZipitIsPresent.Value;
            }
        }

        protected bool WinZipIsPresent
        {
            get
            {
                if (_WinZipIsPresent == null)
                {
                    string progfiles = null;
                    if (_wzunzip == null || _wzzip == null)
                    {
                        progfiles = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                        _wzunzip = Path.Combine(progfiles, "winzip\\wzunzip.exe");
                        _wzzip = Path.Combine(progfiles, "winzip\\wzzip.exe");
                    }
                    _WinZipIsPresent = new Nullable<bool>(File.Exists(_wzunzip) && File.Exists(_wzzip));
                }
                return _WinZipIsPresent.Value;
            }
        }

        protected bool SevenZipIsPresent
        {
            get
            {
                if (_SevenZipIsPresent == null)
                {
                    string progfiles = null;
                    if (_sevenzip == null)
                    {
                        progfiles = System.Environment.GetEnvironmentVariable("ProgramFiles");
                        _sevenzip = Path.Combine(progfiles, "7-zip\\7z.exe");
                    }
                    _SevenZipIsPresent = new Nullable<bool>(File.Exists(_sevenzip));
                }
                return _SevenZipIsPresent.Value;
            }
        }


        protected bool InfoZipIsPresent
        {
            get
            {
                if (_InfoZipIsPresent == null)
                {
                    string progfiles = null;
                    if (_infozipzip == null)
                    {
                        progfiles = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                        _infozipzip = Path.Combine(progfiles, "infozip.org\\zip.exe");
                        _infozipunzip = Path.Combine(progfiles, "infozip.org\\unzip.exe");
                    }
                    _InfoZipIsPresent = new Nullable<bool>(File.Exists(_infozipzip) &&
                                                           File.Exists(_infozipunzip));
                }
                return _InfoZipIsPresent.Value;
            }
        }

        internal string BasicVerifyZip(string zipfile)
        {
            return BasicVerifyZip(zipfile, null);
        }


        internal string BasicVerifyZip(string zipfile, string password)
        {
            return BasicVerifyZip(zipfile, password, true);
        }

        internal string BasicVerifyZip(string zipfile, string password, bool emitOutput)
        {
            return BasicVerifyZip(zipfile, password, emitOutput, null);
        }


        internal string BasicVerifyZip(string zipfile, string password, bool emitOutput,
                                       EventHandler<ExtractProgressEventArgs> extractProgress)
        {
            // basic verification of the zip file - can it be extracted?
            // The extraction tool will verify checksums and passwords, as appropriate
#if NOT
            if (WinZipIsPresent)
            {
                TestContext.WriteLine("Verifying zip file {0} with WinZip", zipfile);
                string args = (password == null)
                    ? String.Format("-t {0}", zipfile)
                    : String.Format("-s{0} -t {1}", password, zipfile);

                string wzunzipOut = this.Exec(wzunzip, args, true, emitOutput);
            }
            else
#endif
            {
                TestContext.WriteLine("Verifying zip file {0} with DotNetZip", zipfile);
                ReadOptions options = new ReadOptions();
                if (emitOutput)
                    options.StatusMessageWriter = new StringWriter();

                string extractDir = "verify";
                int c = 0;
                while (Directory.Exists(extractDir + c)) c++;
                extractDir += c;

                using (ZipFile zip2 = ZipFile.Read(zipfile, options))
                {
                    zip2.Password = password;
                    if (extractProgress != null)
                        zip2.ExtractProgress += extractProgress;
                    zip2.ExtractAll(extractDir);
                }
                // emit output, as desired
                if (emitOutput)
                    TestContext.WriteLine("{0}",options.StatusMessageWriter.ToString());

                return extractDir;
            }
        }



        internal static void CreateFilesAndChecksums(string subdir,
                                                     out string[] filesToZip,
                                                     out Dictionary<string, byte[]> checksums)
        {
            CreateFilesAndChecksums(subdir, 0, 0, out filesToZip, out checksums);
        }


        internal static void CreateFilesAndChecksums(string subdir,
                                                     int numFiles,
                                                     int baseSize,
                                                     out string[] filesToZip,
                                                     out Dictionary<string, byte[]> checksums)
        {
            // create a bunch of files
            filesToZip = TestUtilities.GenerateFilesFlat(subdir, numFiles, baseSize);
            DateTime atMidnight = new DateTime(DateTime.Now.Year,
                                               DateTime.Now.Month,
                                               DateTime.Now.Day);
            DateTime fortyFiveDaysAgo = atMidnight - new TimeSpan(45, 0, 0, 0);

            // get checksums for each one
            checksums = new Dictionary<string, byte[]>();

            var rnd = new System.Random();
            foreach (var f in filesToZip)
            {
                if (rnd.Next(3) == 0)
                    File.SetLastWriteTime(f, fortyFiveDaysAgo);
                else
                    File.SetLastWriteTime(f, atMidnight);

                var key = Path.GetFileName(f);
                var chk = TestUtilities.ComputeChecksum(f);
                checksums.Add(key, chk);
            }
        }

        protected static void CreateLargeFilesWithChecksums
            (string subdir,
             int numFiles,
             Action<int,int,Int64> update,
             out string[] filesToZip,
             out Dictionary<string,byte[]> checksums)
        {
            var rnd = new System.Random();
            // create a bunch of files
            filesToZip = TestUtilities.GenerateFilesFlat(subdir,
                                                         numFiles,
                                                         256 * 1024,
                                                         3 * 1024 * 1024,
                                                         update);

            var dates = new DateTime[rnd.Next(6) + 7];
             // midnight
            dates[0] = new DateTime(DateTime.Now.Year,
                                    DateTime.Now.Month,
                                    DateTime.Now.Day);

            for (int i=1; i < dates.Length; i++)
            {
                dates[i] = DateTime.Now -
                    new TimeSpan(rnd.Next(300),
                                 rnd.Next(23),
                                 rnd.Next(60),
                                 rnd.Next(60));
            }

            // get checksums for each one
            checksums = new Dictionary<string, byte[]>();

            foreach (var f in filesToZip)
            {
                File.SetLastWriteTime(f, dates[rnd.Next(dates.Length)]);
                var key = Path.GetFileName(f);
                var chk = TestUtilities.ComputeChecksum(f);
                checksums.Add(key, chk);
            }
        }



        protected void VerifyChecksums(string extractDir,
            System.Collections.Generic.IEnumerable<String> filesToCheck,
            Dictionary<string, byte[]> checksums)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("Verify checksums...");
            int count = 0;
            foreach (var fqPath in filesToCheck)
            {
                var f = Path.GetFileName(fqPath);
                var extractedFile = Path.Combine(extractDir, f);
                Assert.IsTrue(File.Exists(extractedFile), "File does not exist ({0})", extractedFile);
                var chk = TestUtilities.ComputeChecksum(extractedFile);
                Assert.AreEqual<String>(TestUtilities.CheckSumToString(checksums[f]),
                                        TestUtilities.CheckSumToString(chk),
                                        String.Format("Checksums for file {0} do not match.", f));
                count++;
            }

            if (checksums.Count < count)
            {
                TestContext.WriteLine("There are {0} more extracted files than checksums", count - checksums.Count);
                foreach (var file in filesToCheck)
                {
                    if (!checksums.ContainsKey(file))
                        TestContext.WriteLine("Missing: {0}", Path.GetFileName(file));
                }
            }

            if (checksums.Count > count)
            {
                TestContext.WriteLine("There are {0} more checksums than extracted files", checksums.Count - count);
                foreach (var file in checksums.Keys)
                {
                    var selection = from f in filesToCheck where Path.GetFileName(f).Equals(file) select f;

                    if (selection.Count() == 0)
                        TestContext.WriteLine("Missing: {0}", Path.GetFileName(file));
                }
            }


            Assert.AreEqual<Int32>(checksums.Count, count, "There's a mismatch between the checksums and the filesToCheck.");
        }
    }


}
