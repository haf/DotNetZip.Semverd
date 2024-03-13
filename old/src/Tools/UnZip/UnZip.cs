// UnZip.cs
//
// ----------------------------------------------------------------------
// Copyright (c) 2006, 2007, 2008 Microsoft Corporation.  All rights reserved.
//
// This example is released under the Microsoft Public License .
// See the license.txt file accompanying this release for
// full details.
//
// ----------------------------------------------------------------------
//
// This command-line utility unzips a zipfile into the specified directory,
// or lists the entries in a zipfile without unzipping.
//
// compile with:
//     csc /target:exe /r:Ionic.Zip.dll /out:UnZip.exe UnZip.cs
//
// created
// Wed, 29 Mar 2006  14:36
//


using System;
using System.Collections.Generic;
using Ionic.Zip;

namespace Ionic.Zip.Examples
{
    public class UnZip
    {

        private static void Usage()
        {
            Console.WriteLine("UnZip.exe:  extract or list or test the entries in a zip file.");
            Console.WriteLine("            Depends on Ionic's DotNetZip library. This is version {0} of the utility.",
                  System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("usage:\n" +
                  "  unzip [options] <zipfile> [<entryname>...]  \n" +
                  "     unzips all files in the archive.\n" +
                  "     options:\n" +
                  "       -                 emit extracted content to stdout.\n" +
                  "       -o                overwrite existing files if necessary.\n" +
                  "       -f                flatten directory structure when extracting.\n" +
                  "       -p <password>     specify password for extraction.\n" +
                  "       -t                test the file for consistency. \n" +
                  "       -q                operate quietly (no verbose messages). \n" +
                  "       -cp <codepage>    extract with the specified numeric codepage.  Only do this if you\n" +
                  "                         know the codepage, and it is neither IBM437 nor UTF-8. If the \n" +
                  "                         codepage you specify here is different than the codepage of \n" +
                  "                         the cmd.exe, then the verbose messages will look odd, but the \n" +
                  "                         files will be extracted properly.\n" +
                  "       -d <directory>    unpack to the specified directory. If none provided, it will\n" +
                  "                         unzip to the current directory.\n" +
                  "       <entryname>       unzip only the specified filename.\n\n" +
                  "  unzip -l <zipfile>\n" +
                  "     lists the entries in the zip archive.\n" +
                  "  unzip -i <zipfile>\n" +
                  "     displays full information about all the entries in the zip archive.\n" +
                  "  unzip -t <zipfile> [-p <password>] [-cp <codepage>]\n" +
                  "     tests the zip archive.\n" +
                  "  unzip -r <zipfile>\n" +
                  "     repairs the zip archive - rewriting the directory.\n" +
                  "  unzip -?\n" +
                  "     displays this message.\n"
                  );
            Environment.Exit(1);
        }

        enum ActionDesired
        {
            Extract,
            List,
            Info,
            Test,
            Repair
        }

        public static void Main(String[] args)
        {
            int startArgs = 0;
            int i;
            int codePage = 0;
            string zipfile = null;
            string targdir = null;
            string password = null;
            List<string> entriesToExtract = new List<String>();
            bool extractToConsole = false;
            ActionDesired action = ActionDesired.Extract;
            ExtractExistingFileAction behaviorForExistingFile = ExtractExistingFileAction.DoNotOverwrite;
            bool wantQuiet = false;
            bool wantFlatten = false;
            System.IO.Stream bitbucket = System.IO.Stream.Null;
            System.IO.Stream outstream = null;

            // because the comments and filenames on zip entries may be UTF-8
            //System.Console.OutputEncoding = new System.Text.UTF8Encoding();

            if (args.Length == 0) Usage();
            if (args[0] == "-")
            {
                extractToConsole = true;
                outstream = Console.OpenStandardOutput();
                startArgs = 1;
            }

            for (i = startArgs; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-cp":
                        i++;
                        if (args.Length <= i) Usage();
                        if (codePage != 0) Usage();
                        System.Int32.TryParse(args[i], out codePage);
                        break;

                    case "-d":
                        i++;
                        if (args.Length <= i) Usage();
                        if (targdir != null) Usage();
                        if (extractToConsole) Usage();
                        if (action != ActionDesired.Extract) Usage();
                        targdir = args[i];
                        break;

                    case "-f":
                        wantFlatten = true;
                        if (action != ActionDesired.Extract) Usage();
                        break;

                    case "-i":
                        if (password != null) Usage();
                        if (targdir != null) Usage();
                        if (wantQuiet) Usage();
                        if (entriesToExtract.Count > 0) Usage();
                        action = ActionDesired.Info;
                        break;

                    case "-l":
                        if (password != null) Usage();
                        if (targdir != null) Usage();
                        if (wantQuiet) Usage();
                        if (entriesToExtract.Count > 0) Usage();
                        if (behaviorForExistingFile == ExtractExistingFileAction.OverwriteSilently) Usage();
                        action = ActionDesired.List;
                        break;

                    case "-o":
                        behaviorForExistingFile = ExtractExistingFileAction.OverwriteSilently;
                        if (action != ActionDesired.Extract) Usage();
                        break;

                    case "-r":
                        if (wantFlatten == true) Usage();
                        if (targdir != null) Usage();
                        if (action == ActionDesired.Test) Usage();
                        action = ActionDesired.Repair;
                        break;

                    case "-p":
                        i++;
                        if (args.Length <= i) Usage();
                        if (password != null) Usage();
                        password = args[i];
                        break;

                    case "-q":
                        if (action == ActionDesired.List) Usage();
                        wantQuiet = true;
                        break;

                    case "-t":
                        action = ActionDesired.Test;
                        if (targdir != null) Usage();
                        //if (wantQuiet) Usage();
                        if (entriesToExtract.Count > 0) Usage();
                        break;

                    case "-?":
                        Usage();
                        break;

                    default:
                        // positional args
                        if (zipfile == null)
                            zipfile = args[i];
                        else if (action != ActionDesired.Extract) Usage();
                        else entriesToExtract.Add(args[i]);
                        break;
                }

            }
            if (zipfile == null)
            {
                Console.WriteLine("unzip: No zipfile specified.\n");
                Usage();
            }

            if (!System.IO.File.Exists(zipfile))
            {
                Console.WriteLine("unzip: That zip file does not exist!\n");
                Usage();
            }

            if (targdir == null) targdir = ".";

            try
            {
                if (action == ActionDesired.Repair)
                {
                    ZipFile.FixZipDirectory(zipfile);
                }
                else
                {
                    var options = new ReadOptions {
                            Encoding = (codePage != 0)
                                ? System.Text.Encoding.GetEncoding(codePage)
                                : null
                    };
                    using (ZipFile zip =  ZipFile.Read(zipfile, options))
                    {

                        if (entriesToExtract.Count > 0)
                        {
                            // extract specified entries
                            foreach (var entryToExtract in entriesToExtract)
                            {
                                // find the entry
                                ZipEntry e= zip[entryToExtract];
                                if (e == null)
                                {
                                    System.Console.WriteLine("  entry ({0}) does not exist in the zip archive.", entryToExtract);
                                }
                                else
                                {
                                    if (wantFlatten) e.FileName = System.IO.Path.GetFileName(e.FileName);

                                    if (password == null)
                                    {
                                        if (e.UsesEncryption)
                                            System.Console.WriteLine("  That entry ({0}) requires a password to extract.", entryToExtract);
                                        else if (extractToConsole)
                                            e.Extract(outstream);
                                        else
                                            e.Extract(targdir, behaviorForExistingFile);
                                    }
                                    else
                                    {
                                        if (extractToConsole)
                                            e.ExtractWithPassword(outstream, password);
                                        else
                                            e.ExtractWithPassword(targdir, behaviorForExistingFile, password);
                                    }
                                }
                            }
                        }
                        else if (action == ActionDesired.Info)
                        {
                            System.Console.WriteLine("{0}", zip.Info);
                        }
                        else
                        {
                            // extract all, or list, or test

                            // The logic here does almost the same thing as the ExtractAll() method
                            // on the ZipFile class.  But in this case we *could* have control over
                            // it, for example only extract files of a certain type, or whose names
                            // matched a certain pattern, or whose lastmodified times fit a certain
                            // condition, or use a different password for each entry, etc.  We can
                            // also display status for each entry, as here.

                            Int64 totalUncompressedSize = 0;
                            bool header = true;
                            foreach (ZipEntry e in zip.EntriesSorted)
                            {
                                if (!wantQuiet)
                                {
                                    if (header)
                                    {
                                        System.Console.WriteLine("Zipfile: {0}", zip.Name);
                                        if ((zip.Comment != null) && (zip.Comment != ""))
                                            System.Console.WriteLine("Comment: {0}", zip.Comment);

                                        System.Console.WriteLine("\n{1,-22} {2,10}  {3,5}   {4,10}  {5,3} {6,8} {0}",
                                                                 "Filename", "Modified", "Size", "Ratio", "Packed", "pw?", "CRC");
                                        System.Console.WriteLine(new System.String('-', 80));
                                        header = false;
                                    }
                                    totalUncompressedSize += e.UncompressedSize;
                                    System.Console.WriteLine("{1,-22} {2,10} {3,5:F0}%   {4,10}  {5,3} {6:X8} {0}",
                                                             e.FileName,
                                                             e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                                                             e.UncompressedSize,
                                                             e.CompressionRatio,
                                                             e.CompressedSize,
                                                             (e.UsesEncryption) ? "Y" : "N",
                                                             e.Crc);

                                    if ((e.Comment != null) && (e.Comment != ""))
                                        System.Console.WriteLine("  Comment: {0}", e.Comment);
                                }

                                if (action == ActionDesired.Extract)
                                {
                                    if (e.UsesEncryption)
                                    {
                                        if (password == null)
                                            System.Console.WriteLine("unzip: {0}: Cannot extract this entry without a password.", e.FileName);
                                        else
                                        {
                                            if (wantFlatten) e.FileName = System.IO.Path.GetFileName(e.FileName);
                                            if (extractToConsole)
                                                e.ExtractWithPassword(outstream, password);
                                            else
                                                e.ExtractWithPassword(targdir, behaviorForExistingFile, password);
                                        }
                                    }
                                    else
                                    {
                                        if (wantFlatten) e.FileName = System.IO.Path.GetFileName(e.FileName);
                                        if (extractToConsole)
                                            e.Extract(outstream);
                                        else
                                            e.Extract(targdir, behaviorForExistingFile);

                                    }
                                }
                                else if (action == ActionDesired.Test)
                                {
                                    e.ExtractWithPassword(bitbucket, password);
                                }

                            } // foreach

                            if (!wantQuiet)
                            {
                                System.Console.WriteLine(new System.String('-', 80));
                                System.Console.WriteLine("{1,-22} {2,10}  {3,5}   {4,10}  {5,3} {6,8} {0}",
                                                         zip.Entries.Count.ToString() + " files", "", totalUncompressedSize, "", "", "", "");
                            }
                        } // else (extract all)
                    } // end using(), the underlying file is closed.
                }
            }
            catch (System.Exception ex1)
            {
                System.Console.Error.WriteLine("exception: " + ex1);
            }

            Console.WriteLine();
        }
    }
}
