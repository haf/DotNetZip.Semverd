// ReadZip.cs
//
// ----------------------------------------------------------------------
// Copyright (c) 2006-2009 Microsoft Corporation.  All rights reserved.
//
// This example is released under the Microsoft Public License .
// See the license.txt file accompanying this release for
// full details.
//
// ----------------------------------------------------------------------
//
// This simple example utility simply reads a zip archive and extracts
// all elements in it, to the specified target directory.
//
// compile with:
//     csc /target:exe /r:Ionic.Zip.dll /out:ReadZip.exe ReadZip.cs
//
// Wed, 29 Mar 2006  14:36
//


using System;
using Ionic.Zip;

namespace Ionic.Zip.Examples
{
    public class ReadZip
    {
        private static void Usage()
        {
            Console.WriteLine("usage:\n  ReadZip2 <zipfile> <unpackdirectory>");
            Environment.Exit(1);
        }


        public static void Main(String[] args)
        {

            if (args.Length != 2) Usage();
            if (!System.IO.File.Exists(args[0]))
            {
                Console.WriteLine("That zip file does not exist!\n");
                Usage();
            }

            try
            {
                // Specifying Console.Out here causes diagnostic msgs to be sent to the Console
                // In a WinForms or WPF or Web app, you could specify nothing, or an alternate
                // TextWriter to capture diagnostic messages.

                var options = new ReadOptions { StatusMessageWriter = System.Console.Out };
                using (ZipFile zip = ZipFile.Read(args[0], options))
                {
                    // This call to ExtractAll() assumes:
                    //   - none of the entries are password-protected.
                    //   - want to extract all entries to current working directory
                    //   - none of the files in the zip already exist in the directory;
                    //     if they do, the method will throw.
                    zip.ExtractAll(args[1]);
                }
            }
            catch (System.Exception ex1)
            {
                System.Console.Error.WriteLine("exception: " + ex1);
            }

        }
    }
}