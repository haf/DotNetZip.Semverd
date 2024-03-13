// ZlibDeflateInflate.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 by Dino Chiesa
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
// Purpose: Demonstrate compression and decompression with the ZlibCodec
// class, which is part of the Ionic.Zlib namespace.
// 
// ------------------------------------------------------------------
//

using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;  // HashAlgorithm
using Ionic.Zlib;

// to allow fast ngen
[assembly: AssemblyTitle("ZlibDeflateInflate.cs")]
[assembly: AssemblyDescription("Demonstrate compression and decompression with the ZlibCodec class")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Dino Chiesa")]
[assembly: AssemblyProduct("DotNetZip Examples")]
[assembly: AssemblyCopyright("Copyright © Dino Chiesa 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.1.1.1")]


namespace Ionic.ToolsAndTests
{
    public class ZlibDeflateInflate
    {
        private HashAlgorithm alg = new SHA256CryptoServiceProvider();

        public ZlibDeflateInflate () {}

        public byte[] ComputeHash(byte[] data)
        {
            return alg.ComputeHash(data);
        }

        
        public byte[] ComputeHash(string text)
        {
            return alg.ComputeHash(UTF8Encoding.UTF8.GetBytes( text ));
        }

        private static string ByteArrayToString(byte[] buffer)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (byte b in buffer)
                sb.Append(b.ToString("X2"));

            return (sb.ToString());
        }

        
        
        public void Run()
        {
            System.Console.WriteLine("\nThis program demonstrates compression of strings with the\nIonic.Zlib.ZlibCodec class.\n");
        
            string textToCompress = File.ReadAllText("ZlibDeflateInflate.cs");
            string hashOfOriginal = ByteArrayToString(ComputeHash(textToCompress));
            System.Console.WriteLine("hash of original:     {0}", hashOfOriginal);
            System.Console.WriteLine("length of original:   {0}", textToCompress.Length);
        
            byte[] compressed = ZlibCodecCompress(textToCompress);
            System.Console.WriteLine("length of compressed: {0}", compressed.Length);

            double compRatio = 100 * (1.0 - (1.0 * compressed.Length) / (1.0 * textToCompress.Length)) ;

            System.Console.WriteLine("compression rate:     {0:N1}%", compRatio);

            string decompressed = ZlibCodecDecompress(compressed);
            string hashOfDecompressed = ByteArrayToString(ComputeHash(textToCompress));
            System.Console.WriteLine("hash of decompressed: {0}", hashOfDecompressed);
            System.Console.WriteLine();
            
            if (hashOfOriginal == hashOfDecompressed)
                System.Console.WriteLine("Round trip SUCCESS: After compress and decompress, we obtained the original text.");
            else                
                System.Console.WriteLine("Round trip FAIL: After compress and decompress, we did not obtain the original text.");
        }


        private byte[] ZlibCodecCompress(string textToCompress)
        {
            int outputSize = 2048;
            byte[] output = new Byte[ outputSize ];
            byte[] uncompressed = UTF8Encoding.UTF8.GetBytes( textToCompress );
            int lengthToCompress = uncompressed.Length;

            // If you want a ZLIB stream, set this to true.  If you want
            // a bare DEFLATE stream, set this to false.
            bool wantRfc1950Header = false;

            using ( MemoryStream ms = new MemoryStream())
            {
                ZlibCodec compressor = new ZlibCodec();
                compressor.InitializeDeflate(CompressionLevel.BestCompression, wantRfc1950Header);  
            
                compressor.InputBuffer = uncompressed;
                compressor.AvailableBytesIn = lengthToCompress;
                compressor.NextIn = 0;
                compressor.OutputBuffer = output;

                foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish } )
                {
                    int bytesToWrite = 0;
                    do
                    {
                        compressor.AvailableBytesOut = outputSize;
                        compressor.NextOut = 0;
                        compressor.Deflate(f);

                        bytesToWrite = outputSize - compressor.AvailableBytesOut ;
                        if (bytesToWrite > 0)
                            ms.Write(output, 0, bytesToWrite);
                    }
                    while (( f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
                           ( f == FlushType.Finish && bytesToWrite != 0));
                }

                compressor.EndDeflate();

                ms.Flush();
                return ms.ToArray();
            }
        }

      
        private string ZlibCodecDecompress(byte[] compressed)
        {
            int outputSize = 2048;
            byte[] output = new Byte[ outputSize ];
            
            // If you have a ZLIB stream, set this to true.  If you have
            // a bare DEFLATE stream, set this to false.
            bool expectRfc1950Header = false;
            
            using ( MemoryStream ms = new MemoryStream())
            {
                ZlibCodec compressor = new ZlibCodec();
                compressor.InitializeInflate(expectRfc1950Header);
            
                compressor.InputBuffer = compressed;
                compressor.AvailableBytesIn = compressed.Length;
                compressor.NextIn = 0;
                compressor.OutputBuffer = output;

                foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish } )
                {
                    int bytesToWrite = 0;
                    do
                    {
                        compressor.AvailableBytesOut = outputSize;
                        compressor.NextOut = 0;
                        compressor.Inflate(f);

                        bytesToWrite = outputSize - compressor.AvailableBytesOut ;
                        if (bytesToWrite > 0)
                            ms.Write(output, 0, bytesToWrite);
                    }
                    while (( f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
                           ( f == FlushType.Finish && bytesToWrite != 0));
                }

                compressor.EndInflate();

                return UTF8Encoding.UTF8.GetString( ms.ToArray() );
            }
        }

      
        public static void Usage()
        {
            Console.WriteLine("\nZlibDeflateInflate: <usage statement here>.\n");
            Console.WriteLine("Usage:\n  ZlibDeflateInflate [-arg1 <value>] [-arg2]");
        }


        public static void Main(string[] args)
        {
            try 
            {
                new ZlibDeflateInflate()
                    .Run();
            }
            catch (System.Exception exc1)
            {
                Console.WriteLine("Exception: {0}", exc1.ToString());
                Usage();
            }
        }
    }
}
