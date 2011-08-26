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
// Purpose: Demonstrate compression and decompression with the ZlibStream
// class, which is part of the Ionic.Zlib namespace.
// 
// ------------------------------------------------------------------
//

using System;
using System.Reflection;
using Ionic.Zlib;


// to allow fast ngen
[assembly: AssemblyTitle("ZlibStreamExample.cs")]
[assembly: AssemblyDescription("Demonstrate compression and decompression using the ZlibStream class")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Dino Chiesa")]
[assembly: AssemblyProduct("DotNetZip Examples")]
[assembly: AssemblyCopyright("Copyright © Dino Chiesa 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.1.1.1")]


namespace Ionic.ToolsAndTests
{

    public class ZlibStreamExample
    {

        /// <summary>
        /// Converts a string to a MemoryStream.
        /// </summary>
        static System.IO.MemoryStream StringToMemoryStream(string s)
        {
            byte[] a = System.Text.Encoding.ASCII.GetBytes(s);
            return new System.IO.MemoryStream(a);
        }

        /// <summary>
        /// Converts a MemoryStream to a string. Makes some assumptions about the content of the stream. 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static String MemoryStreamToString(System.IO.MemoryStream ms)
        {
            byte[] ByteArray = ms.ToArray();
            return System.Text.Encoding.ASCII.GetString(ByteArray);
        }



        static void CopyStream(System.IO.Stream src, System.IO.Stream dest)
        {
            byte[] buffer = new byte[1024];
            int len = src.Read(buffer, 0, buffer.Length);
            while (len > 0)
            {
                dest.Write(buffer, 0, len);
                len = src.Read(buffer, 0, buffer.Length);
            }
            dest.Flush();
        }


        [STAThread]
        public static void Main(System.String[] args)
        {
            try
            {
                System.IO.MemoryStream msSinkCompressed;
                System.IO.MemoryStream msSinkDecompressed;
                ZlibStream zOut;
                String originalText = "Hello, World!  This String will be compressed... ";
                
                System.Console.Out.WriteLine("original:     {0}", originalText);

                // first, compress:
                msSinkCompressed = new System.IO.MemoryStream();
                zOut = new ZlibStream(msSinkCompressed, CompressionMode.Compress, CompressionLevel.BestCompression, true);
                CopyStream(StringToMemoryStream(originalText), zOut);
                zOut.Close();

                // at this point, msSinkCompressed contains the compressed bytes

                // now, decompress:
                msSinkCompressed.Seek(0, System.IO.SeekOrigin.Begin);
                msSinkDecompressed = new System.IO.MemoryStream();
                zOut = new ZlibStream(msSinkDecompressed, CompressionMode.Decompress, true);
                CopyStream(msSinkCompressed, zOut);

                // at this point, msSinkDecompressed contains the decompressed bytes
                string decompressed = MemoryStreamToString(msSinkDecompressed);
                System.Console.Out.WriteLine("decompressed: {0}", decompressed);
                System.Console.WriteLine();

                if (originalText == decompressed)
                    System.Console.WriteLine("A-OK. Compression followed by decompression gets the original text.");
                else
                    System.Console.WriteLine("The compression/decompression cycle failed.");
            }
            catch (System.Exception e1)
            {
                Console.WriteLine("Exception: " + e1);
            }
        }
    }
}