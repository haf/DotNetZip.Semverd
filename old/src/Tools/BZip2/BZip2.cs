// BZip2.cs
//
// ----------------------------------------------------------------------
// Copyright (c) 2011 Dino Chiesa.  All rights reserved.
//
// This example is released under the Microsoft Permissive License of
// October 2006.  See the license.txt file accompanying this release for
// full details.
//
// ----------------------------------------------------------------------
//
// This utility creates a compresses the file specified on the command line,
// using BZip2, creating a new file, with the .bz2 suffix. Or, if the
// file specified on the command-line has a .bz2 suffix, this utility
// decompresses it, restoring the original file.
//
// compile with:
//     csc /debug+ /target:exe /r:Ionic.Zip.dll /out:BZip2.exe BZip2.cs
//
// Sat, 23 Jul 2011  22:32
//

using System;
using System.IO;
using Ionic.BZip2;

namespace Ionic.Zip.Examples
{
    public class BZip2
    {
        private static void Usage()
        {
            string UsageMessage =
            "BZip2.exe:  compress a file using BZip2, or decompress a BZip2-compressed file. \n"+
            "            The original file is deleted after processing.\n" +
            "            This tool depends on Ionic's DotNetZip library. This is version {0} \n" +
            "            of the utility. See http://dotnetzip.codeplex.com for info.\n"+
            "  usage:\n   BZip2.exe <FileToProcess> [arguments]\n" +
            "\n  arguments: \n" +
            "    -v         - verbose output.\n" +
            "    -f         - force overwrite of any existing files.\n" +
            "    -keep      - don't delete the original file after compressing or \n"+
            "                 decompressing it.\n";

            Console.WriteLine(UsageMessage,
                              System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Environment.Exit(1);
         }



        static void CtrlC_Handler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nCtrl-C");
            //cleanupCompleted.WaitOne();
            // prevent the process from exiting until cleanup is done:
            args.Cancel = true;
        }

        private static void Pump(Stream src, Stream dest)
        {
            byte[] buffer = new byte[2048];
            int n;
            while ((n = src.Read(buffer, 0, buffer.Length)) > 0)
                dest.Write(buffer, 0, n);

        }


        static string Compress(string fname, bool forceOverwrite)
        {
            var outFname = fname + ".bz2";
            if (File.Exists(outFname))
            {
                if (forceOverwrite)
                    File.Delete(outFname);
                else
                    return null;
            }

            using (Stream fs = File.OpenRead(fname),
                   output = File.Create(outFname),
                   compressor = new Ionic.BZip2.ParallelBZip2OutputStream(output))
                Pump(fs, compressor);

            return outFname;
        }


        public static string Decompress(string fname, bool forceOverwrite)
        {
            var outFname = Path.GetFileNameWithoutExtension(fname);
            if (File.Exists(outFname))
            {
                if (forceOverwrite)
                    File.Delete(outFname);
                else
                    return null;
            }

            using (Stream fs = File.OpenRead(fname),
                   output = File.Create(outFname),
                   decompressor = new Ionic.BZip2.BZip2InputStream(fs))
                Pump(decompressor, output);

            return outFname;
        }


        public static void Main(String[] args)
        {
            bool keepOriginal = false;
            bool force = false;
            bool verbose = false;
            if (args.Length < 1) Usage();

            if (!File.Exists(args[0]))
            {
                System.Console.WriteLine("That file ({0}) does not exist.", args[0]);
                return;
            }

            Console.CancelKeyPress += CtrlC_Handler;

            try
            {
                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-keep":
                            keepOriginal = true;
                            break;

                        case "-f":
                            force = true;
                            break;

                        case "-v":
                            verbose = true;
                            break;

                        default:
                            throw new ArgumentException(args[i]);
                    }
                }

                string fname = args[0];
                bool decompress = (fname.ToLower().EndsWith(".bz") || fname.ToLower().EndsWith(".bz2"));
                string result = decompress
                    ? Decompress(fname, force)
                    : Compress(fname, force);

                if (result==null)
                {
                    Console.WriteLine("No action taken. The file already exists.");
                }
                else
                {
                    if (verbose)
                    {
                        var fi1 = new FileInfo(fname);
                        var fi2 = new FileInfo(result);
                        if (decompress)
                        {
                            Console.WriteLine("  Original    : {0} bytes", fi1.Length);
                            Console.WriteLine("  Decompressed: {0} bytes", fi2.Length);
                            Console.WriteLine("  Comp Ratio  : {0:N1}%", 100.0 - (fi1.Length/(0.01 * fi2.Length)));
                        }
                        else
                        {
                            Console.WriteLine("  Original  : {0} bytes", fi1.Length);
                            Console.WriteLine("  Compressed: {0} bytes", fi2.Length);
                            Console.WriteLine("  Comp Ratio: {0:N1}%", 100.0 - (fi2.Length/(0.01 * fi1.Length)));
                        }
                    }

                    if (!keepOriginal)
                    {
                        File.Delete(fname);
                    }
                }

            }
            catch (System.Exception ex1)
            {
                System.Console.WriteLine("Exception: " + ex1);
            }
        }
    }
}
