// QuickCompress.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 by Dino Chiesa
// All rights reserved!
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
// Purpose: Demonstrate compression and decompression with the easy
// helper methods in the Ionic.Zlib namespace.
//
// ------------------------------------------------------------------
//

using System;
using System.Text;
using System.Reflection;
using Ionic.Zlib;
using System.Security.Cryptography;
using System.Diagnostics;


// to allow fast ngen
[assembly: AssemblyTitle("QuickCompress.cs")]
[assembly: AssemblyDescription("Demonstrate compression and decompression using the helper methods in the IOnic.Zlib namespace")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Dino Chiesa")]
[assembly: AssemblyProduct("DotNetZip Examples")]
[assembly: AssemblyCopyright("Copyright © Dino Chiesa 2009-2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.1.1.1")]


namespace Ionic.ToolsAndTests
{

    /// <summary>
    ///   Holds the results of the compression.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Compression algorithms can be measured two ways: size of the
    ///     result, and the time required to compress.  This small class
    ///     just holds the measured result for a particular compression
    ///     algorithm.
    ///   </para>
    /// </remarks>
    public class CompressionTrialResult
    {
        public string Label;
        public int Cycles;
        public byte[] CompressedData;
        public TimeSpan TimeForManyCycles;

        public void Show()
        {
            Console.WriteLine("{0}", this.Label);
            Console.WriteLine("  Compressed Size      : {0}", this.CompressedData.Length);
            Console.WriteLine("  Time for {0} cycles: {1:N1}s",
                              this.Cycles,
                              this.TimeForManyCycles.TotalSeconds);
        }
    }


    public class QuickCompress
    {
        internal static string GoPlacidly =
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
";


        /// <summary>
        ///   Perform one trial, measuring the effectiveness and speed
        ///   of compression.
        /// </summary>
        ///
        /// <param name='label'>the label for the trial</param>
        ///
        /// <param name='compressor'>a function that accepts a string,
        /// and returns a byte array representing the compressed
        /// form</param>
        ///
        /// <param name='decompressor'>a function that accepts a byte
        /// array, and decompresses it, returning a string. </param>
        ///
        /// <param name='s'>the string to compress and decompress</param>
        ///
        /// <param name='nCycles'>the number of cycles to time</param>
        ///
        /// <returns>the CompressionTrialResult describing the trial results</returns>
        ///
        /// <remarks>
        /// </remarks>
        public CompressionTrialResult DoTrial(string label,
                                              Func<string, byte[]> compressor,
                                              Func<byte[],string> decompressor,
                                              string s,
                                              int nCycles)
        {
            byte[] compressed = compressor(s);

            // verify that the compression decompresses correctly
            string uncompressed = decompressor(compressed);
            if (s.Length != uncompressed.Length)
                throw new Exception("decompression failed.");

            // compress the same thing 1000 times, and measure the time
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i=0; i < nCycles; i++)
                compressed = compressor(s);

            stopwatch.Stop();

            var result = new CompressionTrialResult
            {
                Label = label,
                CompressedData = compressed,
                Cycles = nCycles,
                TimeForManyCycles = stopwatch.Elapsed
            };
            return result;
        }


        internal static string ByteArrayToHexString(byte[] b)
        {
            var sb1 = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                sb1.Append(String.Format("{0:X2}", b[i]));
            }
            return sb1.ToString().ToLower();
        }


        private void Run()
        {
            Console.WriteLine("Compressing a string...");

            int lengthOriginal = GoPlacidly.Length;

            // do the compression:
            byte[] b = ZlibStream.CompressString(GoPlacidly);

            int lengthCompressed = b.Length;

            Console.WriteLine();
            Console.WriteLine("  Original Length: {0}", lengthOriginal);
            Console.WriteLine("Compressed Length: {0}", lengthCompressed);
            Console.WriteLine("    Compression %: {0:n1}%", lengthCompressed/(0.01 * lengthOriginal));
            Console.WriteLine("Compressed Data  : {0}", ByteArrayToHexString(b));
            Console.WriteLine();

            // now let's do some timed trials
            Console.WriteLine("Doing timing runs....");
            CompressionTrialResult result;
            result = DoTrial("Zlib",
                             ZlibStream.CompressString,
                             ZlibStream.UncompressString,
                             GoPlacidly,
                             10000);
            result.Show();
            result = DoTrial("GZip",
                             GZipStream.CompressString,
                             GZipStream.UncompressString,
                             GoPlacidly,
                             10000);
            result.Show();

            result = DoTrial("Deflate",
                             DeflateStream.CompressString,
                             DeflateStream.UncompressString,
                             GoPlacidly,
                             10000);
            result.Show();

            // All these classes use the same underlying algorithm - DEFLATE -
            // which means they all produce compressed forms that are roughly the
            // same size. The difference between them is only in the metadata
            // surrounding the raw compressed streams, and the level of integrity
            // checking they provide. For example, during compression, the
            // GzipStream internally calculates an Alder checksum on the data;
            // during decompression, it verifies that checksum, as an integrity
            // check. The other classes don't do this. Therefore the GZipStream
            // will always take slightly longer in compression and decompression
            // than the others, and will produce compressed streams that are
            // slightly larger.  The results will show that.
        }


        [STAThread]
        public static void Main(System.String[] args)
        {
            try
            {
                var me = new QuickCompress();
                me.Run();
            }
            catch (System.Exception e1)
            {
                Console.WriteLine("Exception: " + e1);
            }
        }
    }
}
