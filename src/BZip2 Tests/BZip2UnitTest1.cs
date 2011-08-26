using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Ionic.BZip2;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Ionic.BZip2.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        private System.Random rnd;

        public UnitTest1()
        {
            this.rnd = new System.Random();
            FilesToRemove = new System.Collections.Generic.List<string>();
        }

        static UnitTest1()
        {
            string lorem = TestStrings["LoremIpsum"];
            LoremIpsumWords = lorem.Split(" ".ToCharArray(),
                                          System.StringSplitOptions.RemoveEmptyEntries);
        }


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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        private string CurrentDir = null;
        private string TopLevelDir = null;
        protected System.Collections.Generic.List<string> FilesToRemove;

        // Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            CurrentDir = System.IO.Directory.GetCurrentDirectory();
            Assert.AreNotEqual<string>(System.IO.Path.GetFileName(CurrentDir), "Temp", "at start");

            string parentDir = System.Environment.GetEnvironmentVariable("TEMP");

            TopLevelDir = System.IO.Path.Combine(parentDir, String.Format("Ionic.ZlibTest-{0}.tmp", System.DateTime.Now.ToString("yyyyMMMdd-HHmmss")));
            System.IO.Directory.CreateDirectory(TopLevelDir);
            System.IO.Directory.SetCurrentDirectory(TopLevelDir);

            FilesToRemove.Add(TopLevelDir);
        }


        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
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
            FilesToRemove.Clear();
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

        #region Helpers
        private static void CopyStream(System.IO.Stream src, System.IO.Stream dest)
        {
            byte[] buffer = new byte[4096];
            int n;
            while ((n = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, n);
            }
        }

        private static string GetTestDependentDir(string startingPoint, string subdir)
        {
            var location = startingPoint;
            for (int i = 0; i < 3; i++)
                location = Path.GetDirectoryName(location);

            location = Path.Combine(location, subdir);
            return location;
        }

        private static string GetTestBinDir(string startingPoint)
        {
            return GetTestDependentDir(startingPoint, "BZip2 Tests\\bin\\Debug");
        }

        private string GetContentFile(string fileName)
        {
            string testBin = GetTestBinDir(CurrentDir);
            string path = Path.Combine(testBin, String.Format("Resources\\{0}", fileName));
            Assert.IsTrue(File.Exists(path), "file ({0}) does not exist", path);
            return path;
        }

        private static Int32 GetCrc(string fname)
        {
            using (var fs1 = File.OpenRead(fname))
            {
                var checker = new Ionic.Crc.CRC32(true);
                return checker.GetCrc32(fs1);
            }
        }

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
            int rc = Exec_NoContext(program, args, waitForExit, out output);

            if (rc != 0)
                throw new Exception(String.Format("Non-zero RC {0}: {1}", program, output));

            if (emitOutput)
                this.TestContext.WriteLine("output: {0}", output);
            else
                this.TestContext.WriteLine("A-OK. (output suppressed)");

            return output;
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
                //output = CleanWzzipOut(output); // just in case
                return p.ExitCode;
            }
            else
            {
                p.Start();
            }
            output = "";
            return 0;
        }


        void CreateAndFillTextFile(string filename, Int64 minimumSize)
        {
            // fill the file with text data, selecting one word at a time
            int L = LoremIpsumWords.Length - 2;
            Int64 bytesRemaining = minimumSize;
            using (StreamWriter sw = File.CreateText(filename))
            {
                do
                {
                    // pick a word at random
                    int n = this.rnd.Next(L);
                    int batchLength = LoremIpsumWords[n].Length +
                        LoremIpsumWords[n+1].Length +
                        LoremIpsumWords[n+2].Length + 3;
                    sw.Write(LoremIpsumWords[n]);
                    sw.Write(" ");
                    sw.Write(LoremIpsumWords[n+1]);
                    sw.Write(" ");
                    sw.Write(LoremIpsumWords[n+2]);
                    sw.Write(" ");
                    bytesRemaining -= batchLength;
                } while (bytesRemaining > 0);
            }
        }

        #endregion


        [TestMethod]
        [Timeout(15 * 60*1000)] // 60*1000 = 1min
        public void BZ_LargeParallel()
        {
            string filename = "LargeFile.txt";
            int minSize = 0x6000000 + this.rnd.Next(0x6000000);
            TestContext.WriteLine("Creating large file, minimum {0} bytes", minSize);

            CreateAndFillTextFile(filename, minSize);

            Func<Stream,Stream>[] getBzStream = {
                new Func<Stream,Stream>( s0 => {
                        return new Ionic.BZip2.BZip2OutputStream(s0);
                    }),
                new Func<Stream,Stream>( s1 => {
                        return new Ionic.BZip2.ParallelBZip2OutputStream(s1);
                    })
            };

            var ts = new TimeSpan[2];
            for (int k=0; k < 2; k++)
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                TestContext.WriteLine("Trial {0}", k);
                stopwatch.Start();
                string bzFname = Path.GetFileNameWithoutExtension(filename) +
                    "." + k + Path.GetExtension(filename) + ".bz2";
                using (Stream input = File.OpenRead(filename),
                       output = File.Create(bzFname),
                       compressor = getBzStream[k](output))
                {
                    CopyStream(input, compressor);
                }
                stopwatch.Stop();
                ts[k] = stopwatch.Elapsed;
                TestContext.WriteLine("Trial complete {0} : {1}", k, ts[k]);
            }

            Assert.IsTrue(ts[1]<ts[0],
                          "Parallel compression took MORE time.");
        }




        [TestMethod]
        [Timeout(15 * 60*1000)] // 60*1000 = 1min
        public void BZ_Basic()
        {
            TestContext.WriteLine("Creating fodder file.");
            // select a random text string
            var line = TestStrings.ElementAt(this.rnd.Next(0, TestStrings.Count)).Value;
            int n = 4000 + this.rnd.Next(1000); // number of iters
            var fname = "Pippo.txt";
            // emit many many lines into a text file:
            using (var sw = new StreamWriter(File.Create(fname)))
            {
                for (int k=0; k < n; k++)
                {
                    sw.WriteLine(line);
                }
            }
            int crcOriginal = GetCrc(fname);
            int blockSize = 0;

            Func<Stream,Stream>[] getBzStream = {
                new Func<Stream,Stream>( s0 => {
                        var decorator = new Ionic.BZip2.BZip2OutputStream(s0, blockSize);
                        return decorator;
                    }),
                new Func<Stream,Stream>( s1 => {
                        var decorator = new Ionic.BZip2.ParallelBZip2OutputStream(s1, blockSize);
                        return decorator;
                    })
            };

            int[] blockSizes = { 1,2,3,4,5,6,7,8,9 };

            for (int k=0; k < getBzStream.Length; k++)
            {
                for (int m=0; m < blockSizes.Length; m++)
                {
                    blockSize = blockSizes[m];
                    var getStream = getBzStream[k];
                    var root = Path.GetFileNameWithoutExtension(fname);
                    var ext = Path.GetExtension(fname);
                    // compress into bz2
                    var bzFname = String.Format("{0}.{1}.blocksize{2}{3}.bz2",
                                                root,
                                                (k==0)?"SingleThread":"MultiThread",
                                                blockSize, ext);

                    TestContext.WriteLine("Compress cycle ({0},{1})", k,m);
                    TestContext.WriteLine("file {0}", bzFname);
                    using (var fs = File.OpenRead(fname))
                    {
                        using (var output = File.Create(bzFname))
                        {
                            using (var compressor = getStream(output))
                            {
                                CopyStream(fs, compressor);
                            }
                        }
                    }

                    TestContext.WriteLine("Decompress");
                    var decompressedFname = Path.GetFileNameWithoutExtension(bzFname);
                    using (Stream fs = File.OpenRead(bzFname),
                           output = File.Create(decompressedFname),
                           decompressor = new Ionic.BZip2.BZip2InputStream(fs))
                    {
                        CopyStream(decompressor, output);
                    }

                    TestContext.WriteLine("Check CRC");
                    int crcDecompressed = GetCrc(decompressedFname);
                    Assert.AreEqual<int>(crcOriginal, crcDecompressed,
                                         "CRC mismatch {0:X8} != {1:X8}",
                                         crcOriginal, crcDecompressed);
                    TestContext.WriteLine("");

                    // just for the sake of disk space economy:
                    File.Delete(decompressedFname);
                    File.Delete(bzFname);
                }
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void BZ_Error_1()
        {
            var bzbin = GetTestDependentDir(CurrentDir, "Tools\\BZip2\\bin\\Debug");
            var dnzBzip2exe = Path.Combine(bzbin, "bzip2.exe");
            string decompressedFname = "ThisWillNotWork.txt";
            using (Stream input = File.OpenRead(dnzBzip2exe),
                   decompressor = new Ionic.BZip2.BZip2InputStream(input),
                   output = File.Create(decompressedFname))
                CopyStream(decompressor, output);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void BZ_Error_2()
        {
            string decompressedFname = "ThisWillNotWork.txt";
            using (Stream input = new MemoryStream(), // empty stream
                   decompressor = new Ionic.BZip2.BZip2InputStream(input),
                   output = File.Create(decompressedFname))
                CopyStream(decompressor, output);
        }


        [TestMethod]
        public void BZ_Utility()
        {
            var bzbin = GetTestDependentDir(CurrentDir, "Tools\\BZip2\\bin\\Debug");
            var dnzBzip2exe = Path.Combine(bzbin, "bzip2.exe");
            Assert.IsTrue(File.Exists(dnzBzip2exe), "Bzip2.exe is missing {0}",
                          dnzBzip2exe);
            var unxBzip2exe = "\\bin\\bzip2.exe";
            Assert.IsTrue(File.Exists(unxBzip2exe), "Bzip2.exe is missing {0}",
                          unxBzip2exe);

            foreach (var key in TestStrings.Keys)
            {
                int count = this.rnd.Next(18) + 4;
                TestContext.WriteLine("Doing string {0}", key);
                var s =  TestStrings[key];
                var fname = String.Format("Pippo-{0}.txt", key);
                using (var sw = new StreamWriter(File.Create(fname)))
                {
                    for (int k=0; k < count; k++)
                    {
                        sw.WriteLine(s);
                    }
                }

                int crcOriginal = GetCrc(fname);

                string args = fname + " -keep -v";
                TestContext.WriteLine("Exec: bzip2 {0}", args);
                string bzout = this.Exec(dnzBzip2exe, args);

                var bzfile = fname + ".bz2";
                Assert.IsTrue(File.Exists(bzfile), "File is missing. {0}",
                              bzfile);

                File.Delete(fname);
                Assert.IsTrue(!File.Exists(fname), "The delete failed. {0}",
                              fname);

                System.Threading.Thread.Sleep(1200);

                args = "-dfk "+ bzfile;
                TestContext.WriteLine("Exec: bzip2 {0}", args);
                bzout = this.Exec(unxBzip2exe, args);
                Assert.IsTrue(File.Exists(fname), "File is missing. {0}",
                              fname);

                int crcDecompressed = GetCrc(fname);
                Assert.AreEqual<int>(crcOriginal, crcDecompressed,
                                     "CRC mismatch {0:X8}!={1:X8}",
                                     crcOriginal, crcDecompressed);
            }
        }


        [TestMethod]
        public void BZ_Samples()
        {
            string testBin = GetTestBinDir(CurrentDir);
            string resourceDir = Path.Combine(testBin, "Resources");
            var filesToDecompress = Directory.GetFiles(resourceDir, "*.bz2");

            Assert.IsTrue(filesToDecompress.Length > 2,
                          "There are not enough sample files");

            foreach (var filename in filesToDecompress)
            {
                TestContext.WriteLine("Decompressing {0}", filename);
                var outFname = filename + ".decompressed";
                TestContext.WriteLine("Decompressing to {0}", outFname);

                using (var fs = File.OpenRead(filename))
                {
                    using (var output = File.Create(outFname))
                    {
                        using (var decompressor = new Ionic.BZip2.BZip2InputStream(fs))
                        {
                            CopyStream(decompressor, output);
                        }
                    }
                }
                TestContext.WriteLine("");
            }
        }




        internal static Dictionary<String,String> TestStrings = new Dictionary<String,String>() {
            {"LetMeDoItNow", "I expect to pass through the world but once. Any good therefore that I can do, or any kindness I can show to any creature, let me do it now. Let me not defer it, for I shall not pass this way again. -- Anonymous, although some have attributed it to Stephen Grellet" },

            {"UntilHeExtends", "Until he extends the circle of his compassion to all living things, man will not himself find peace. - Albert Schweitzer, early 20th-century German Nobel Peace Prize-winning mission doctor and theologian." },

        {"WhatWouldThingsHaveBeenLike","'What would things have been like [in Russia] if during periods of mass arrests people had not simply sat there, paling with terror at every bang on the downstairs door and at every step on the staircase, but understood they had nothing to lose and had boldly set up in the downstairs hall an ambush of half a dozen people?' -- Alexander Solzhenitsyn"
                },

            {"GoPlacidly",
            @"Go placidly amid the noise and haste, and remember what peace there may be in silence.

As far as possible, without surrender, be on good terms with all persons. Speak your truth quietly and clearly; and listen to others, even to the dull and the ignorant, they too have their story. Avoid loud and aggressive persons, they are vexations to the spirit.

If you compare yourself with others, you may become vain and bitter; for always there will be greater and lesser persons than yourself. Enjoy your achievements as well as your plans. Keep interested in your own career, however humble; it is a real possession in the changing fortunes of time.

Exercise caution in your business affairs, for the world is full of trickery. But let this not blind you to what virtue there is; many persons strive for high ideals, and everywhere life is full of heroism. Be yourself. Especially, do not feign affection. Neither be cynical about love, for in the face of all aridity and disenchantment it is perennial as the grass.

Take kindly to the counsel of the years, gracefully surrendering the things of youth. Nurture strength of spirit to shield you in sudden misfortune. But do not distress yourself with imaginings. Many fears are born of fatigue and loneliness.

Beyond a wholesome discipline, be gentle with yourself. You are a child of the universe, no less than the trees and the stars; you have a right to be here. And whether or not it is clear to you, no doubt the universe is unfolding as it should.

Therefore be at peace with God, whatever you conceive Him to be, and whatever your labors and aspirations, in the noisy confusion of life, keep peace in your soul.

With all its sham, drudgery and broken dreams, it is still a beautiful world.

Be cheerful. Strive to be happy.

Max Ehrmann c.1920
"},

            {"IhaveaDream", @"Let us not wallow in the valley of despair, I say to you today, my friends.

And so even though we face the difficulties of today and tomorrow, I still have a dream. It is a dream deeply rooted in the American dream.

I have a dream that one day this nation will rise up and live out the true meaning of its creed: 'We hold these truths to be self-evident, that all men are created equal.'

I have a dream that one day on the red hills of Georgia, the sons of former slaves and the sons of former slave owners will be able to sit down together at the table of brotherhood.

I have a dream that one day even the state of Mississippi, a state sweltering with the heat of injustice, sweltering with the heat of oppression, will be transformed into an oasis of freedom and justice.

I have a dream that my four little children will one day live in a nation where they will not be judged by the color of their skin but by the content of their character.

I have a dream today!

I have a dream that one day, down in Alabama, with its vicious racists, with its governor having his lips dripping with the words of 'interposition' and 'nullification' -- one day right there in Alabama little black boys and black girls will be able to join hands with little white boys and white girls as sisters and brothers.

I have a dream today!

I have a dream that one day every valley shall be exalted, and every hill and mountain shall be made low, the rough places will be made plain, and the crooked places will be made straight; 'and the glory of the Lord shall be revealed and all flesh shall see it together.'2
"},

            {            "LoremIpsum",
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
                         "\n"}
        };

        static string[] LoremIpsumWords;

        private const int WORKING_BUFFER_SIZE = 0x4000;

    }



}
