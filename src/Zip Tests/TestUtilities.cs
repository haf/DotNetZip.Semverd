// TestUtilities.cs
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
// Time-stamp: <2011-July-26 16:19:47>
//
// ------------------------------------------------------------------
//
// This module defines some utility classes used by the unit tests for
// DotNetZip.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ionic.Zip.Tests.Utilities
{
    class TestUtilities
    {
        static System.Random _rnd;
        static string cdir;
        static TestUtilities()
        {
            _rnd = new System.Random();
            LoremIpsumWords = LoremIpsum.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            cdir = Directory.GetCurrentDirectory();
        }



        #region Test Init and Cleanup

        internal static void Initialize(out string TopLevelDir)
        {
            if (cdir == null) cdir = Directory.GetCurrentDirectory();

            TopLevelDir = TestUtilities.GenerateUniquePathname("tmp");
            Directory.CreateDirectory(TopLevelDir);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(TopLevelDir));
        }

        internal static void Cleanup(string CurrentDir, List<String> FilesToRemove)
        {
            Assert.AreNotEqual<string>(Path.GetFileName(CurrentDir), "Temp", "at finish");
            Directory.SetCurrentDirectory(CurrentDir);
            IOException GotException = null;
            int Tries = 0;
            do
            {
                try
                {
                    GotException = null;
                    foreach (string filename in FilesToRemove)
                    {
                        if (Directory.Exists(filename))
                        {
                            // turn off any ReadOnly attributes
                            ClearReadOnly(filename);
                            Directory.Delete(filename, true);
                        }
                        if (File.Exists(filename))
                        {
                            File.Delete(filename);
                        }
                    }
                    Tries++;
                }
                catch (IOException ioexc)
                {
                    GotException = ioexc;
                    // use an backoff interval before retry
                    System.Threading.Thread.Sleep(200 * Tries);
                }
            } while ((GotException != null) && (Tries < 4));
            if (GotException != null) throw GotException;
        }


        public static void ClearReadOnly(string dirname)
        {
            // don't traverse reparse points
            if ((File.GetAttributes(dirname) & FileAttributes.ReparsePoint) != 0)
                return;

            foreach (var d in Directory.GetDirectories(dirname))
            {
                ClearReadOnly(d); // recurse
            }

            foreach (var f in Directory.GetFiles(dirname))
            {
                // clear ReadOnly and System attributes
                var a = File.GetAttributes(f);
                if ((a & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    a ^= FileAttributes.ReadOnly;
                    File.SetAttributes(f, a);
                }
                if ((a & FileAttributes.System) == FileAttributes.System)
                {
                    a ^= FileAttributes.System;
                    File.SetAttributes(f, a);
                }
            }
        }



        #endregion


        #region Helper methods

        internal static string TrimVolumeAndSwapSlashes(string pathName)
        {
            //return (((pathname[1] == ':') && (pathname[2] == '\\')) ? pathname.Substring(3) : pathname)
            //    .Replace('\\', '/');
            if (String.IsNullOrEmpty(pathName)) return pathName;
            if (pathName.Length < 2) return pathName.Replace('\\', '/');
            return (((pathName[1] == ':') && (pathName[2] == '\\')) ? pathName.Substring(3) : pathName)
                .Replace('\\', '/');
        }

        internal static DateTime RoundToEvenSecond(DateTime source)
        {
            // round to nearest second:
            if ((source.Second % 2) == 1)
                source += new TimeSpan(0, 0, 1);

            DateTime dtRounded = new DateTime(source.Year, source.Month, source.Day, source.Hour, source.Minute, source.Second);
            //if (source.Millisecond >= 500) dtRounded = dtRounded.AddSeconds(1);
            return dtRounded;
        }


        /// <summary>
        ///   count occurrences of sample in string s.
        /// </summary>
        internal static int CountOccurrences(string s, string sample)
        {
            int nFound = 0;
            int n = 0;
            do
            {
                n = s.IndexOf(sample,n);
                if (n>0) nFound++;
                n++;
            } while (n>0);
            return nFound;
        }


        internal static void CreateAndFillFileText(string filename, Int64 size)
        {
            CreateAndFillFileText(filename, size, null);
        }


        internal static void CreateAndFillFileText(string filename,
                                                   Int64 size,
                                                   Action<Int64> update)
        {
            Int64 bytesRemaining = size;

            if (size > 128 * 1024)
            {
                var rnd = new System.Random();
                RandomTextGenerator rtg = new RandomTextGenerator();
                int chunkSize = 48 * 1024;
                int variationSize = 2 * 1024;
                var newLinePair = Encoding.ASCII.GetBytes("\n\n");
                int nCycles = 0;
                // fill the file with text data, selecting large blocks at a time
                var fodder = new byte[32][];
                using (var fs = File.Create(filename))
                {
                    do
                    {
                        int n = rnd.Next(fodder.Length);
                        if (fodder[n] == null)
                        {
                            string generatedText = rtg.Generate(chunkSize);
                            fodder[n] = Encoding.ASCII.GetBytes(generatedText);
                        }

                        var bytes = fodder[n];
                        int len = bytes.Length - rnd.Next(variationSize);
                        fs.Write(bytes,0,len);
                        bytesRemaining -= len;
                        fs.Write(newLinePair, 0, newLinePair.Length);
                        bytesRemaining -= newLinePair.Length;
                        nCycles++;
                        if ((nCycles % 1024) == 0)
                        {
                            if (update != null)
                                update(size - bytesRemaining);
                        }
                    } while (bytesRemaining > 0);
                }
            }
            else
            {
                // fill the file with text data, selecting one word at a time
                using (StreamWriter sw = File.CreateText(filename))
                {
                    do
                    {
                        // pick a word at random
                        string selectedWord = LoremIpsumWords[_rnd.Next(LoremIpsumWords.Length)];
                        if (bytesRemaining < selectedWord.Length + 1)
                        {
                            sw.Write(selectedWord.Substring(0, (int)bytesRemaining));
                            bytesRemaining = 0;
                        }
                        else
                        {
                            sw.Write(selectedWord);
                            sw.Write(" ");
                            bytesRemaining -= (selectedWord.Length + 1);
                        }
                        if (update != null)
                            update(size - bytesRemaining);

                    } while (bytesRemaining > 0);
                    sw.Close();
                }
            }
        }

        internal static void CreateAndFillFileText(string Filename,
                                                   string Line,
                                                   Int64 size)
        {
            CreateAndFillFileText(Filename, Line, size, null);
        }


        internal static void CreateAndFillFileText(string Filename,
                                                   string Line,
                                                   Int64 size,
                                                   System.Action<Int64> update)
        {
            Int64 bytesRemaining = size;
            // fill the file by repeatedly writing out the same line
            using (StreamWriter sw = File.CreateText(Filename))
            {
                do
                {
                    if (bytesRemaining < Line.Length + 2)
                    {
                        if (bytesRemaining == 1)
                            sw.Write(" ");
                        else if (bytesRemaining == 1)
                            sw.WriteLine();
                        else
                            sw.WriteLine(Line.Substring(0, (int)bytesRemaining - 2));
                        bytesRemaining = 0;
                    }
                    else
                    {
                        sw.WriteLine(Line);
                        bytesRemaining -= (Line.Length + 2);
                    }
                    if (update != null)
                        update(size - bytesRemaining);
                } while (bytesRemaining > 0);
                sw.Close();
            }
        }

        internal static void CreateAndFillFileBinary(string Filename, Int64 size)
        {
            _CreateAndFillBinary(Filename, size, false, null);
        }

        internal static void CreateAndFillFileBinary(string Filename, Int64 size, System.Action<Int64> update)
        {
            _CreateAndFillBinary(Filename, size, false, update);
        }

        internal static void CreateAndFillFileBinaryZeroes(string Filename, Int64 size, System.Action<Int64> update)
        {
            _CreateAndFillBinary(Filename, size, true, update);
        }

        delegate void ProgressUpdate(System.Int64 bytesXferred);

        private static void _CreateAndFillBinary(string filename, Int64 size, bool zeroes, System.Action<Int64> update)
        {
            Int64 bytesRemaining = size;
            // fill with binary data
            int sz = 65536 * 8;
            if (size < sz) sz = (int)size;
            byte[] buffer = new byte[sz];
            int nCycles = 0;
            using (var fileStream = File.Create(filename))
            {
                while (bytesRemaining > 0)
                {
                    int sizeOfChunkToWrite = (bytesRemaining > buffer.Length) ? buffer.Length : (int)bytesRemaining;
                    if (!zeroes) _rnd.NextBytes(buffer);
                    fileStream.Write(buffer, 0, sizeOfChunkToWrite);
                    bytesRemaining -= sizeOfChunkToWrite;
                    nCycles++;
                    if (size > 1024*1024)
                    {
                        if ((nCycles % 256) == 0)
                        {
                            if (update != null)
                                update(size - bytesRemaining);
                        }
                    }
                }
                fileStream.Close();
            }
        }


        internal static void CreateAndFillFile(string filename, Int64 size)
        {
            if (size == 0)
                File.Create(filename);
            else if (_rnd.Next(2) == 0)
                CreateAndFillFileText(filename, size);
            else
                CreateAndFillFileBinary(filename, size);
        }

        internal enum FileFlavor
        {
            Text = 0, Binary = 1,
        }

        internal static void CreateAndFillFile(string filename,
                                               Int64 size,
                                               FileFlavor flavor)
        {
            if (size == 0)
                File.Create(filename);
            else if (flavor == FileFlavor.Text)
                CreateAndFillFileText(filename, size);
            else
                CreateAndFillFileBinary(filename, size);
        }

        internal static string CreateUniqueFile(string extension, string ContainingDirectory)
        {
            //string nameOfFileToCreate = GenerateUniquePathname(extension, ContainingDirectory);
            string nameOfFileToCreate = Path.Combine(ContainingDirectory, String.Format("{0}.{1}", Path.GetRandomFileName(), extension));
            // create an empty file
            using (var fs = File.Create(nameOfFileToCreate)) { }
            return nameOfFileToCreate;
        }

        internal static string CreateUniqueFile(string extension)
        {
            return CreateUniqueFile(extension, null);
        }

        internal static string CreateUniqueFile(string extension, Int64 size)
        {
            return CreateUniqueFile(extension, null, size);
        }

        internal static string CreateUniqueFile(string extension, string ContainingDirectory, Int64 size)
        {
            //string fileToCreate = GenerateUniquePathname(extension, ContainingDirectory);
            string nameOfFileToCreate = Path.Combine(ContainingDirectory, String.Format("{0}.{1}", Path.GetRandomFileName(), extension));
            CreateAndFillFile(nameOfFileToCreate, size);
            return nameOfFileToCreate;
        }

        static System.Reflection.Assembly _a = null;
        private static System.Reflection.Assembly _MyAssembly
        {
            get
            {
                if (_a == null)
                {
                    _a = System.Reflection.Assembly.GetExecutingAssembly();
                }
                return _a;
            }
        }

        internal static string GenerateUniquePathname(string extension)
        {
            return GenerateUniquePathname(extension, null);
        }

        internal static string GenerateUniquePathname(string extension, string ContainingDirectory)
        {
            string candidate = null;
            String AppName = _MyAssembly.GetName().Name;

            string parentDir = (ContainingDirectory == null) ? System.Environment.GetEnvironmentVariable("TEMP") :
                ContainingDirectory;
            if (parentDir == null) return null;

            int index = 0;
            do
            {
                index++;
                string Name = String.Format("{0}-{1}-{2}.{3}",
                                            AppName, System.DateTime.Now.ToString("yyyyMMMdd-HHmmss"), index, extension);
                candidate = Path.Combine(parentDir, Name);
            } while (File.Exists(candidate));

            // this file/path does not exist.  It can now be created, as
            // file or directory.
            return candidate;
        }

        internal static int CountEntries(string zipfile)
        {
            int entries = 0;
            using (ZipFile zip = ZipFile.Read(zipfile))
            {
                foreach (ZipEntry e in zip)
                    if (!e.IsDirectory) entries++;
            }
            return entries;
        }


        internal static string GetCheckSumString(string filename)
        {
            return CheckSumToString(ComputeChecksum(filename));
        }

        internal static string CheckSumToString(byte[] checksum)
        {
            var sb = new System.Text.StringBuilder();
            foreach (byte b in checksum)
                sb.Append(b.ToString("x2").ToLower());
            return sb.ToString();
        }

        internal static byte[] ComputeChecksum(string filename)
        {
            var _md5 = System.Security.Cryptography.MD5.Create();
            using (FileStream fs = File.OpenRead(filename))
            {
                return _md5.ComputeHash(fs);
            }
        }

        private static char GetOneRandomPasswordChar()
        {
            const int range = 126 - 33;
            const int start = 33;
            char x = '\0';
            do
            {
                x = (char)(_rnd.Next(range) + start);

            } while (x == '^' || x == '&' || x == '"' || x == '>' || x == '<');
            return x;
        }

        internal static string GenerateRandomPassword()
        {
            int length = _rnd.Next(22) + 12;
            return GenerateRandomPassword(length);
        }

        internal static string GenerateRandomPassword(int length)
        {
            char[] a = new char[length];
            for (int i = 0; i < length; i++)
            {
                a[i] = GetOneRandomPasswordChar();
            }

            string result = new System.String(a);
            return result;
        }


        public static string GenerateRandomAsciiString()
        {
            return GenerateRandomAsciiString(_rnd.Next(14));
        }

        public static string GenerateRandomName()
        {
            return
                GenerateRandomUpperString(1) +
                GenerateRandomLowerString(_rnd.Next(9) + 3);
        }

        public static string GenerateRandomName(int length)
        {
            return
                GenerateRandomUpperString(1) +
                GenerateRandomLowerString(length - 1);
        }

        public static string GenerateRandomAsciiString(int length)
        {
            return GenerateRandomAsciiStringImpl(length, 0);
        }

        public static string GenerateRandomUpperString()
        {
            return GenerateRandomAsciiStringImpl(_rnd.Next(10) + 3, 65);
        }

        public static string GenerateRandomUpperString(int length)
        {
            return GenerateRandomAsciiStringImpl(length, 65);
        }

        public static string GenerateRandomLowerString(int length)
        {
            return GenerateRandomAsciiStringImpl(length, 97);
        }

        public static string GenerateRandomLowerString()
        {
            return GenerateRandomAsciiStringImpl(_rnd.Next(9) + 4, 97);
        }

        private static string GenerateRandomAsciiStringImpl(int length, int delta)
        {
            bool WantRandomized = (delta == 0);

            string result = "";
            char[] a = new char[length];

            for (int i = 0; i < length; i++)
            {
                if (WantRandomized)
                    delta = (_rnd.Next(2) == 0) ? 65 : 97;
                a[i] = GetOneRandomAsciiChar(delta);
            }

            result = new System.String(a);
            return result;
        }



        private static char GetOneRandomAsciiChar(int delta)
        {
            // delta == 65 means uppercase
            // delta == 97 means lowercase
            return (char)(_rnd.Next(26) + delta);
        }

        public static char GetOneRandomLowercaseAsciiChar()
        {
            return (char)(_rnd.Next(26) + 97);
        }

        public static char GetOneRandomUppercaseAsciiChar()
        {
            return (char)(_rnd.Next(26) + 65);
        }




        internal static int GenerateFilesOneLevelDeep(TestContext tc,
                                                      string testName,
                                                      string dirToZip,
                                                      Action<Int16, Int32> update,
                                                      out int subdirCount)
        {
            int[] settings = { 7, 6, 17, 23, 4000, 4000 }; // to randomly set dircount, filecount, and filesize
            return GenerateFilesOneLevelDeep(tc, testName, dirToZip, settings, update, out subdirCount);
        }


        internal static int GenerateFilesOneLevelDeep(TestContext tc,
                                                      string testName,
                                                      string dirToZip,
                                                      int[] settings,
                                                      Action<Int16, Int32> update,
                                                      out int subdirCount)
        {
            int entriesAdded = 0;
            String filename = null;

            subdirCount = _rnd.Next(settings[0]) + settings[1];
            if (update != null)
                update(0, subdirCount);
            tc.WriteLine("{0}: Creating {1} subdirs.", testName, subdirCount);
            for (int i = 0; i < subdirCount; i++)
            {
                string subdir = Path.Combine(dirToZip, String.Format("dir{0:D4}", i));
                Directory.CreateDirectory(subdir);

                int filecount = _rnd.Next(settings[2]) + settings[3];
                if (update != null)
                    update(1, filecount);
                tc.WriteLine(":: Subdir {0}, Creating {1} files.", i, filecount);
                for (int j = 0; j < filecount; j++)
                {
                    int n = _rnd.Next(2);
                    filename = String.Format("file{0:D4}.{1}", j, (n == 0) ? "txt" : "bin");
                    TestUtilities.CreateAndFillFile(Path.Combine(subdir, filename),
                                                    _rnd.Next(settings[4]) + settings[5],
                                                    (FileFlavor)n);
                    entriesAdded++;
                    if (update != null)
                        update(3, j + 1);
                }
                if (update != null)
                    update(2, i + 1);
            }
            if (update != null)
                update(4, entriesAdded);
            return entriesAdded;
        }


        internal static string[] GenerateFilesFlat(string subdir)
        {
            return GenerateFilesFlat(subdir, 0);
        }

        internal static string[] GenerateFilesFlat(string subdir, int numFilesToCreate)
        {
            return GenerateFilesFlat(subdir, numFilesToCreate, 0, 0);
        }

        internal static string[] GenerateFilesFlat(string subdir, int numFilesToCreate, int size)
        {
            return GenerateFilesFlat(subdir, numFilesToCreate, size, size);
        }

        internal static string[] GenerateFilesFlat(string subdir,
                                                   int numFilesToCreate,
                                                   int lowSize, int highSize)
        {
            return GenerateFilesFlat(subdir, numFilesToCreate, lowSize, highSize,
                                     null);
        }

        internal static string[] GenerateFilesFlat(string subdir,
                                                   int numFilesToCreate,
                                                   int lowSize,
                                                   int highSize,
                                                   Action<Int32,Int32,Int64> update)
        {
            if (numFilesToCreate==0)
                numFilesToCreate = _rnd.Next(23) + 14;

            if (lowSize == highSize && lowSize == 0)
            {
                lowSize = 5000;
                highSize = 39000;
            }
            if (!Directory.Exists(subdir))
                Directory.CreateDirectory(subdir);

            int i = 0;
            Action<Int64> byteUpdate = null;
            if (update != null)
            {
                byteUpdate = new Action<Int64>( x => {
                        update(1,i,x);
                    });
            }

            string[] filesToZip = new string[numFilesToCreate];
            for (i = 0; i < numFilesToCreate; i++)
            {
                filesToZip[i] = Path.Combine(subdir, String.Format("testfile{0:D3}.txt", i));
                var sz = _rnd.Next(highSize - lowSize) + lowSize;
                if (update != null)
                    update(0, i, sz);
                TestUtilities.CreateAndFillFileText(filesToZip[i],
                                                    sz,
                                                    byteUpdate);
                if (update != null) update(2,i,numFilesToCreate);
            }
            return filesToZip;
        }


        internal static string GetTestBinDir(string startingPoint)
        {
            return GetTestDependentDir(startingPoint, "Zip Tests\\bin\\Debug");
        }

        internal static string GetTestSrcDir(string startingPoint)
        {
            return GetTestDependentDir(startingPoint, "Zip Tests");
        }

        private static string GetTestDependentDir(string startingPoint, string subdir)
        {
            var location = startingPoint;
            for (int i = 0; i < 3; i++)
                location = Path.GetDirectoryName(location);

            location = Path.Combine(location, subdir);
            return location;
        }


        internal static Ionic.CopyData.Transceiver
            StartProgressMonitor(string progressChannel, string title, string initialStatus)
        {
            string testBin = TestUtilities.GetTestBinDir(cdir);
            string progressMonitorTool = Path.Combine(testBin, "Resources\\UnitTestProgressMonitor.exe");
            string requiredDll = Path.Combine(testBin, "Resources\\Ionic.CopyData.dll");
            Assert.IsTrue(File.Exists(progressMonitorTool), "progress monitor tool does not exist ({0})",  progressMonitorTool);
            Assert.IsTrue(File.Exists(requiredDll), "required DLL does not exist ({0})",  requiredDll);

            // start the progress monitor
            string ignored;
            //this.Exec(progressMonitorTool, String.Format("-channel {0}", progressChannel), false);
            TestUtilities.Exec_NoContext(progressMonitorTool, String.Format("-channel {0}", progressChannel), false, out ignored);

            var txrx = new Ionic.CopyData.Transceiver();
            System.Threading.Thread.Sleep(1000);
            txrx.Channel = progressChannel;
            System.Threading.Thread.Sleep(450);
            txrx.Send("test " + title);
            System.Threading.Thread.Sleep(120);
            txrx.Send("status " + initialStatus);
            return txrx;
        }

        internal static int Exec_NoContext(string program, string args, out string output)
        {
            return Exec_NoContext(program, args, true, out output);
        }


        internal static int Exec_NoContext(string program, string args, bool waitForExit, out string output)
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
                    }

                };

            if (waitForExit)
            {
                StringBuilder sb = new StringBuilder();
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                // must read at least one of the stderr or stdout asynchronously,
                // to avoid deadlock
                Action<Object, System.Diagnostics.DataReceivedEventArgs> stdErrorRead = (o, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                        sb.Append(e.Data);
                };

                p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(stdErrorRead);
                p.Start();
                p.BeginErrorReadLine();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (sb.Length > 0)
                    output += sb.ToString();
                output = CleanWzzipOut(output); // just in case
                return p.ExitCode;
            }
            else
            {
                p.Start();
            }
            output = "";
            return 0;
        }


        /// <summary>
        ///   The WinZip command-line tools emit dots and backspaces in the output.
        ///   For a large zip file, the output can be 1mb or more, of which 99% is
        ///   dots and backspaces. This method trims them from the output, making it
        ///   suitable for printing into the TestContext output.
        /// </summary>
        protected static string CleanWzzipOut(string txt)
        {
            int previousLength = 0;
            int cycles = 0;
            do
            {
                // wzzip.exe can generate long sequences of dots, followed by long
                // sequences of backspaces.  Don't want to replace two backspaces
                // with the empty string, so replace a sequence of a non-backspace
                // char followed by backspace with the empty string.  Do it in
                // cycles to handle those long sequences.

                cycles++;
                previousLength = txt.Length;
                txt = Regex.Replace(txt, "[^\u0008]\u0008", "");
            } while (previousLength != txt.Length && cycles < 80);

            return txt;
        }

        #endregion

        internal static string LoremIpsum =
            "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Integer " +
            "vulputate, nibh non rhoncus euismod, erat odio pellentesque lacus, sit " +
            "amet convallis mi augue et odio. Phasellus cursus urna facilisis " +
            "quam. Suspendisse nec metus et sapien scelerisque euismod. Nullam " +
            "molestie sem quis nisl. Fusce pellentesque, ante sed semper egestas, sem " +
            "nulla vestibulum nulla, quis sollicitudin leo lorem elementum " +
            "wisi. Aliquam vestibulum nonummy orci. Sed in dolor sed enim ullamcorper " +
            "accumsan. Duis vel nibh. Class aptent taciti sociosqu ad litora torquent " +
            "per conubia nostra, per inceptos hymenaeos. Sed faucibus, enim sit amet " +
            "venenatis laoreet, nisl elit posuere est, ut sollicitudin tortor velit " +
            "ut ipsum. Aliquam erat volutpat. Phasellus tincidunt vehicula " +
            "eros. Curabitur vitae erat. " +
            "\n " +
            "Quisque pharetra lacus quis sapien. Duis id est non wisi sagittis " +
            "adipiscing. Nulla facilisi. Etiam quam erat, lobortis eu, facilisis nec, " +
            "blandit hendrerit, metus. Fusce hendrerit. Nunc magna libero, " +
            "sollicitudin non, vulputate non, ornare id, nulla.  Suspendisse " +
            "potenti. Nullam in mauris. Curabitur et nisl vel purus vehicula " +
            "sodales. Class aptent taciti sociosqu ad litora torquent per conubia " +
            "nostra, per inceptos hymenaeos. Cum sociis natoque penatibus et magnis " +
            "dis parturient montes, nascetur ridiculus mus. Donec semper, arcu nec " +
            "dignissim porta, eros odio tempus pede, et laoreet nibh arcu et " +
            "nisl. Morbi pellentesque eleifend ante. Morbi dictum lorem non " +
            "ante. Nullam et augue sit amet sapien varius mollis. " +
            "\n " +
            "Nulla erat lorem, fringilla eget, ultrices nec, dictum sed, " +
            "sapien. Aliquam libero ligula, porttitor scelerisque, lobortis nec, " +
            "dignissim eu, elit. Etiam feugiat, dui vitae laoreet faucibus, tellus " +
            "urna molestie purus, sit amet pretium lorem pede in erat.  Ut non libero " +
            "et sapien porttitor eleifend. Vestibulum ante ipsum primis in faucibus " +
            "orci luctus et ultrices posuere cubilia Curae; In at lorem et lacus " +
            "feugiat iaculis. Nunc tempus eros nec arcu tristique egestas. Quisque " +
            "metus arcu, pretium in, suscipit dictum, bibendum sit amet, " +
            "mauris. Aliquam non urna. Suspendisse eget diam. Aliquam erat " +
            "volutpat. In euismod aliquam lorem. Mauris dolor nisl, consectetuer sit " +
            "amet, suscipit sodales, rutrum in, lorem. Nunc nec nisl. Nulla ante " +
            "libero, aliquam porttitor, aliquet at, imperdiet sed, diam. Pellentesque " +
            "tincidunt nisl et ipsum. Suspendisse purus urna, semper quis, laoreet " +
            "in, vestibulum vel, arcu. Nunc elementum eros nec mauris. " +
            "\n " +
            "Vivamus congue pede at quam. Aliquam aliquam leo vel turpis. Ut " +
            "commodo. Integer tincidunt sem a risus. Cras aliquam libero quis " +
            "arcu. Integer posuere. Nulla malesuada, wisi ac elementum sollicitudin, " +
            "libero libero molestie velit, eu faucibus est ante eu libero. Sed " +
            "vestibulum, dolor ac ultricies consectetuer, tellus risus interdum diam, " +
            "a imperdiet nibh eros eget mauris. Donec faucibus volutpat " +
            "augue. Phasellus vitae arcu quis ipsum ultrices fermentum. Vivamus " +
            "ultricies porta ligula. Nullam malesuada. Ut feugiat urna non " +
            "turpis. Vivamus ipsum. Vivamus eleifend condimentum risus. Curabitur " +
            "pede. Maecenas suscipit pretium tortor. Integer pellentesque. " +
            "\n " +
            "Mauris est. Aenean accumsan purus vitae ligula. Lorem ipsum dolor sit " +
            "amet, consectetuer adipiscing elit. Nullam at mauris id turpis placerat " +
            "accumsan. Sed pharetra metus ut ante. Aenean vel urna sit amet ante " +
            "pretium dapibus. Sed nulla. Sed nonummy, lacus a suscipit semper, erat " +
            "wisi convallis mi, et accumsan magna elit laoreet sem. Nam leo est, " +
            "cursus ut, molestie ac, laoreet id, mauris. Suspendisse auctor nibh. " +
            "\n";

        static string[] LoremIpsumWords;


    }



    public static class Extensions
    {

        public static IEnumerable<string> SplitByWords(this string subject)
        {
            List<string> tokens = new List<string>();
            Regex regex = new Regex(@"\s+");
            tokens.AddRange(regex.Split(subject));

            return tokens;
        }

        // Capitalize
        public static string Capitalize(this string subject)
        {
            if (subject.Length < 2) return subject.ToUpper();
            return subject.Substring(0, 1).ToUpper() +
                subject.Substring(1);
        }

        // TrimPunctuation
        public static string TrimPunctuation(this string subject)
        {
            while (subject.EndsWith(".") ||
                   subject.EndsWith(",") ||
                   subject.EndsWith(";") ||
                   subject.EndsWith("?") ||
                   subject.EndsWith("!"))
                subject = subject.Substring(0, subject.Length - 1);
            return subject;
        }
    }


}
