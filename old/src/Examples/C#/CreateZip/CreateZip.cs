// CreateZip.cs
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
// This simplistic utility gets a list of all the files in the specified directory,
// and zips them into a single archive.  This utility does not recurse through
// the directory tree.
//
// compile with:
//     csc /debug+ /target:exe /R:Ionic.Utils.Zip.dll /out:CreateZip.exe CreateZip.cs 
//
//
// Wed, 29 Mar 2006  14:36
//

using System;
using Ionic.Zip;

namespace Ionic.Zip.Examples
{
    public class CreateZip
    {
        private static void Usage()
        {
            Console.WriteLine("usage:\n  CreateZip <ZipFileToCreate> <directory>");
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
                    // note: this does not recurse directories! 
                    String[] filenames = System.IO.Directory.GetFiles(DirectoryToZip);

                    // This is just a sample, provided to illustrate the DotNetZip interface.  
                    // This logic does not recurse through sub-directories.
                    // If you are zipping up a directory, you may want to see the AddDirectory() method, 
                    // which operates recursively. 
                    foreach (String filename in filenames)
                    {
                        Console.WriteLine("Adding {0}...", filename);
                        ZipEntry e= zip.AddFile(filename);
                        e.Comment = "Added by Cheeso's CreateZip utility."; 
                    }

                    zip.Comment= String.Format("This zip archive was created by the CreateZip example application on machine '{0}'",
                       System.Net.Dns.GetHostName());

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