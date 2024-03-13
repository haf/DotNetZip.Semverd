// ZipDir.cs
// 
// ----------------------------------------------------------------------
// Copyright (c) 2006, 2007, 2008 Microsoft Corporation.  All rights reserved.
//
// This example is released under the Microsoft Permissive License of
// October 2006.  See the license.txt file accompanying this release for 
// full details. 
//
// ----------------------------------------------------------------------
//
// This utility zips up a single directory specified on the command line.
// It is like a specialized ZipIt tool (See ZipIt.cs).
//
// compile with:
//     csc /debug+ /target:exe /r:Zip.dll /out:ZipDir.exe ZipDir.cs 
//
// Wed, 29 Mar 2006  14:36
//

using System;
using Ionic.Zip;

namespace Ionic.Zip.Examples
{

    public class ZipDir
    {

        private static void Usage()
        {
            Console.WriteLine("usage:\n  ZipDir <ZipFileToCreate> <directory>");
            Environment.Exit(1);
        }

        public static void Main(String[] args)
        {
            if (args.Length != 2) Usage();
            if (!System.IO.Directory.Exists(args[1]))
            {
                Console.WriteLine("The directory does not exist!\n");
                Usage();
            }
            if (System.IO.File.Exists(args[0]))
            {
                Console.WriteLine("That zipfile already exists!\n");
                Usage();
            }
            if (!args[0].EndsWith(".zip"))
            {
                Console.WriteLine("The filename must end with .zip!\n");
                Usage();
            }

            string ZipFileToCreate = args[0];
            string DirectoryToZip = args[1];
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.StatusMessageTextWriter = System.Console.Out;
                    zip.AddDirectory(DirectoryToZip); // recurses subdirectories
                    zip.Save(ZipFileToCreate);
                }
            }
            catch (System.Exception ex1)
            {
                System.Console.Error.WriteLine("exception: " + ex1);
            }

        }
    }
}