// ZipIt.cs
//
// ----------------------------------------------------------------------
// Copyright (c) 2006-2011 Dino Chiesa.  All rights reserved.
//
// This example is released under the Microsoft Permissive License of
// October 2006.  See the license.txt file accompanying this release for
// full details.
//
// ----------------------------------------------------------------------
//
// This utility zips up a set of files and directories specified on the command line.
//
// compile with:
//     csc /debug+ /target:exe /r:Ionic.Zip.dll /out:ZipIt.exe ZipIt.cs
//
// Fri, 23 Feb 2007  11:51
//

using System;
using System.IO;
using Ionic.Zip;

namespace Ionic.Zip.Examples
{
    public class ZipIt
    {
        private static void Usage()
        {
            string UsageMessage =
            "Zipit.exe:  zip up a directory, file, or a set of them, into a zipfile.\n" +
            "            Depends on Ionic's DotNetZip library. This is version {0} of the utility.\n" +
            "usage:\n   ZipIt.exe <ZipFileToCreate> [arguments]\n" +
            "\narguments: \n" +
            "  <directory> | <file>  - a directory or file to add to the archive.\n" +
            "  -64                   - use ZIP64 extensions, for large files or large numbers of files.\n" +
            "  -aes                  - use WinZip-compatible AES 256-bit encryption for entries\n" +
            "                          subsequently added to the archive. Requires a password.\n" +
            "  -cp <codepage>        - use the specified numeric codepage to encode entry filenames \n" +
            "                          and comments, instead of the default IBM437 code page.\n" +
            "                          (cannot be used with -utf8 option)\n" +
            "  -C bzip|deflate|none  - use BZip2, Deflate, or No compression, for entries subsequently\n"+
            "                          added to the zip. The default is DEFLATE.\n"+
            "  -d <path>             - use the given directory path in the archive for\n" +
            "                          succeeding items added to the archive.\n" +
            "  -D <path>             - find files in the given directory on disk.\n" +
            "  -e[s|r|q|a]           - when there is an error reading a file to be zipped, either skip\n" +
            "                          the file, retry, quit, or ask the user what to do.\n"+
            "  -E <selector>         - a file selection expression.  Examples: \n" +
            "                            *.txt \n" +
            "                            (name = *.txt) OR (name = *.xml) \n" +
            "                            (attrs = H) OR (name != *.xml) \n" +
            "                            (ctime < 2009/02/28-10:20:00) \n" +
            "                            (size > 1g) AND (mtime < 2009-12-10) \n" +
            "                            (ctime > 2009-04-29) AND (size < 10kb) \n" +
            "                          Filenames can include full paths. You must surround a filename \n" +
            "                          that includes spaces with single quotes.\n" +
            "  -j-                   - do not traverse NTFS junctions\n" +
            "  -j+                   - traverse NTFS junctions (default)\n" +
            "  -L <level>            - compression level, 0..9 (Default is 6).\n" +
            "                          This applies only if using DEFLATE compression, the default.\n" +
            "  -p <password>         - apply the specified password for all succeeding files added.\n" +
            "                          use \"\" to reset the password to nil.\n" +
            "  -progress             - emit progress reports (good when creating large zips)\n" +
            "  -r-                   - don't recurse directories (default).\n" +
            "  -r+                   - recurse directories.\n" +
            "  -s <entry> 'string'   - insert an entry of the given name into the \n" +
            "                          archive, with the given string as its content.\n" +
            "  -sfx [w|c]            - create a self-extracting archive, either a Windows or console app." +
            "                          (cannot be used with -split)\n"+
            "  -split <maxsize>      - produce a split zip, with the specified maximum size. You can\n" +
            "                          optionally use kb or mb as a suffix to the size. \n" +
            "                          (-split cannot be used with -sfx).\n" +
            "  -Tw+                  - store Windows-format extended times (default).\n" +
            "  -Tw-                  - don't store Windows-format extended times.\n" +
            "  -Tu+                  - store Unix-format extended times (default).\n" +
            "  -Tu-                  - don't store Unix-format extended times (default).\n" +
            "  -UTnow                - use uniform date/time, NOW, for all entries. \n" +
            "  -UTnewest             - use uniform date/time, newest entry, for all entries. \n" +
            "  -UToldest             - use uniform date/time, oldest entry, for all entries. \n" +
            "  -UT <datetime>        - use uniform date/time, specified, for all entries. \n" +
            "  -utf8                 - use UTF-8 encoding for entry filenames and comments,\n" +
            "                          instead of the the default IBM437 code page.\n" +
            "                          (cannot be used with -cp option)\n" +
            "  -zc <comment>         - use the given comment for the archive.\n";

            Console.WriteLine(UsageMessage,
                               System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Environment.Exit(1);
         }


        static bool justHadByteUpdate= false;
        static bool isCanceled= false;
        static bool wantProgressReports = false;

        private static void SaveProgress(object sender, SaveProgressEventArgs e)
        {
            if (isCanceled)
            {
                e.Cancel = true;
                return;
            }
            if (!wantProgressReports) return;

            switch(e.EventType)
            {
                case ZipProgressEventType.Saving_Started:
                    Console.WriteLine("Saving: {0}", e.ArchiveName);
                    break;

                case ZipProgressEventType.Saving_Completed:
                    justHadByteUpdate= false;
                    Console.WriteLine();
                    Console.WriteLine("Done: {0}", e.ArchiveName);
                    break;

                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    if (justHadByteUpdate)
                        Console.WriteLine();
                    Console.WriteLine("  Writing: {0} ({1}/{2})",
                                      e.CurrentEntry.FileName, e.EntriesSaved+1, e.EntriesTotal);
                    justHadByteUpdate= false;
                    break;

                case ZipProgressEventType.Saving_AfterWriteEntry:
                    break;

                case ZipProgressEventType.Saving_EntryBytesRead:
                    if (justHadByteUpdate)
                        Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("     {0}/{1} ({2:N0}%)", e.BytesTransferred, e.TotalBytesToTransfer,
                                  e.BytesTransferred / (0.01 * e.TotalBytesToTransfer ));
                    justHadByteUpdate= true;
                    break;
            }
        }


        // Ask the user what he wants to do
        public static void ZipError(object sender, ZipErrorEventArgs e)
        {
            Console.WriteLine("Error reading {0}...", e.FileName);
            Console.WriteLine("   Exception: {0}...", e.Exception);
            ZipEntry entry = e.CurrentEntry;
            string response = null;
            do
            {
                Console.Write("Retry, Skip, or Quit ? (R/S/Q) ");
                response = Console.ReadLine();
                Console.WriteLine();

            } while (response != null &&
                     response[0]!='S' && response[0]!='s' &&
                     response[0]!='R' && response[0]!='r' &&
                     response[0]!='Q' && response[0]!='q');


            e.Cancel = (response[0]=='Q' || response[0]=='q');

            if (response[0]=='S' || response[0]=='s')
                entry.ZipErrorAction = ZipErrorAction.Skip;
            else if (response[0]=='R' || response[0]=='r')
                entry.ZipErrorAction = ZipErrorAction.Retry;
        }



        static void CtrlC_Handler(object sender, ConsoleCancelEventArgs args)
        {
            isCanceled = true;
            Console.WriteLine("\nCtrl-C");
            //cleanupCompleted.WaitOne();
            // prevent the process from exiting until cleanup is done:
            args.Cancel = true;
        }


        public static void Main(String[] args)
        {
            bool saveToStdout = false;
            if (args.Length < 2) Usage();

            if (args[0]=="-")
            {
                saveToStdout = true;
            }
            else if (File.Exists(args[0]))
            {
                System.Console.WriteLine("That zip file ({0}) already exists.", args[0]);
            }


            // Because the comments and filenames on zip entries may be UTF-8
            // System.Console.OutputEncoding = new System.Text.UTF8Encoding();

            Console.CancelKeyPress += CtrlC_Handler;

            try
            {
                Nullable<SelfExtractorFlavor> flavor = null;
                int codePage = 0;
                ZipEntry e = null;
                int _UseUniformTimestamp = 0;
                DateTime _fixedTimestamp= System.DateTime.Now;
                string entryDirectoryPathInArchive = "";
                string directoryOnDisk = null;
                bool recurseDirectories = false;
                bool wantRecurse = false;
                string actualItem;

                // read/update an existing zip, or create a new one.
                using (ZipFile zip = new ZipFile(args[0]))
                {
                    zip.StatusMessageTextWriter = System.Console.Out;
                    zip.SaveProgress += SaveProgress;
                    for (int i = 1; i < args.Length; i++)
                    {
                        switch (args[i])
                        {
                            case "-es":
                                zip.ZipErrorAction = ZipErrorAction.Skip;
                                break;

                            case "-er":
                                zip.ZipErrorAction = ZipErrorAction.Retry;
                                break;

                            case "-eq":
                                zip.ZipErrorAction = ZipErrorAction.Throw;
                                break;

                            case "-ea":
                                zip.ZipError += ZipError;
                                break;

                            case "-64":
                                zip.UseZip64WhenSaving = Zip64Option.Always;
                                break;

                            case "-aes":
                                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                                break;

                            case "-C":
                                i++;
                                if (args.Length <= i) Usage();
                                switch(args[i].ToLower())
                                {
                                    case "b":
                                    case "bzip":
                                    case "bzip2":
                                        zip.CompressionMethod = CompressionMethod.BZip2;
                                        break;
                                    case "d":
                                    case "deflate":
                                        zip.CompressionMethod = CompressionMethod.Deflate;
                                        break;
                                    case "n":
                                    case "none":
                                        zip.CompressionMethod = CompressionMethod.None;
                                        break;
                                    default:
                                        Usage();
                                        break;
                                }
                                break;

                            case "-cp":
                                i++;
                                if (args.Length <= i) Usage();
                                System.Int32.TryParse(args[i], out codePage);
                                if (codePage != 0)
                                {
                                    zip.AlternateEncoding = System.Text.Encoding.GetEncoding(codePage);
                                    zip.AlternateEncodingUsage = ZipOption.Always;
                                }
                                break;

                            case "-d":
                                i++;
                                if (args.Length <= i) Usage();
                                entryDirectoryPathInArchive = args[i];
                                break;

                            case "-D":
                                i++;
                                if (args.Length <= i) Usage();
                                directoryOnDisk = args[i];
                                break;

                            case "-E":
                                i++;
                                if (args.Length <= i) Usage();
                                wantRecurse = recurseDirectories || args[i].Contains("\\");
                                // Console.WriteLine("spec({0})", args[i]);
                                // Console.WriteLine("dir({0})", directoryOnDisk);
                                // Console.WriteLine("dirInArc({0})", entryDirectoryPathInArchive);
                                // Console.WriteLine("recurse({0})", recurseDirectories);
                                zip.UpdateSelectedFiles(args[i],
                                                        directoryOnDisk,
                                                        entryDirectoryPathInArchive,
                                                        wantRecurse);
                                break;

                            case "-j-":
                                zip.AddDirectoryWillTraverseReparsePoints = false;
                                break;

                            case "-j+":
                                zip.AddDirectoryWillTraverseReparsePoints = true;
                                break;

                            case "-L":
                                i++;
                                if (args.Length <= i) Usage();
                                zip.CompressionLevel = (Ionic.Zlib.CompressionLevel)
                                    System.Int32.Parse(args[i]);
                                break;

                            case "-p":
                                i++;
                                if (args.Length <= i) Usage();
                                zip.Password = (args[i] == "") ? null : args[i];
                                break;

                            case "-progress":
                                wantProgressReports = true;
                                break;

                            case "-r-":
                                recurseDirectories = false;
                                break;

                            case "-r+":
                                recurseDirectories = true;
                                break;

                            case "-s":
                                i++;
                                if (args.Length <= i) Usage();
                                string entryName = args[i];
                                i++;
                                if (args.Length <= i) Usage();
                                string content = args[i];
                                e = zip.AddEntry(Path.Combine(entryDirectoryPathInArchive, entryName), content);
                                //                                 if (entryComment != null)
                                //                                 {
                                //                                     e.Comment = entryComment;
                                //                                     entryComment = null;
                                //                                 }
                                break;

                            case "-sfx":
                                i++;
                                if (args.Length <= i) Usage();
                                if (args[i] != "w" && args[i] != "c") Usage();
                                flavor = new Nullable<SelfExtractorFlavor>
                                    ((args[i] == "w") ? SelfExtractorFlavor.WinFormsApplication : SelfExtractorFlavor.ConsoleApplication);
                                break;

                            case "-split":
                                i++;
                                if (args.Length <= i) Usage();
                                if (args[i].EndsWith("K") || args[i].EndsWith("k"))
                                    zip.MaxOutputSegmentSize = Int32.Parse(args[i].Substring(0, args[i].Length - 1)) * 1024;
                                else if (args[i].EndsWith("M") || args[i].EndsWith("m"))
                                    zip.MaxOutputSegmentSize = Int32.Parse(args[i].Substring(0, args[i].Length - 1)) * 1024 * 1024;
                                else
                                    zip.MaxOutputSegmentSize = Int32.Parse(args[i]);
                                break;

                            case "-Tw+":
                                zip.EmitTimesInWindowsFormatWhenSaving = true;
                                break;

                            case "-Tw-":
                                zip.EmitTimesInWindowsFormatWhenSaving = false;
                                break;

                            case "-Tu+":
                                zip.EmitTimesInUnixFormatWhenSaving = true;
                                break;

                            case "-Tu-":
                                zip.EmitTimesInUnixFormatWhenSaving = false;
                                break;

                            case "-UTnow":
                                _UseUniformTimestamp = 1;
                                _fixedTimestamp = System.DateTime.UtcNow;
                                break;

                            case "-UTnewest":
                                _UseUniformTimestamp = 2;
                                break;

                            case "-UToldest":
                                _UseUniformTimestamp = 3;
                                break;

                            case "-UT":
                                i++;
                                if (args.Length <= i) Usage();
                                _UseUniformTimestamp = 4;
                                try
                                {
                                    _fixedTimestamp= System.DateTime.Parse(args[i]);
                                }
                                catch
                                {
                                    throw new ArgumentException("-UT");
                                }
                                break;

                            case "-utf8":
                                zip.AlternateEncoding = System.Text.Encoding.UTF8;
                                zip.AlternateEncodingUsage = ZipOption.Always;
                                break;

#if NOT
                            case "-c":
                                i++;
                                if (args.Length <= i) Usage();
                                entryComment = args[i];  // for the next entry
                                break;
#endif

                            case "-zc":
                                i++;
                                if (args.Length <= i) Usage();
                                zip.Comment = args[i];
                                break;

                            default:
                                // UpdateItem will add Files or Dirs,
                                // recurses subdirectories
                                actualItem = Path.Combine(directoryOnDisk ?? ".", args[i]);
                                zip.UpdateItem(actualItem, entryDirectoryPathInArchive);
                                break;
                        }
                    }

                    if (_UseUniformTimestamp > 0)
                    {
                        if (_UseUniformTimestamp==2)
                        {
                            // newest
                            _fixedTimestamp = new System.DateTime(1601,1,1,0,0,0);
                            foreach(var entry in zip)
                            {
                                if (entry.LastModified > _fixedTimestamp)
                                    _fixedTimestamp = entry.LastModified;
                            }
                        }
                        else if (_UseUniformTimestamp==3)
                        {
                            // oldest
                            foreach(var entry in zip)
                            {
                                if (entry.LastModified < _fixedTimestamp)
                                    _fixedTimestamp = entry.LastModified;
                            }
                        }

                        foreach(var entry in zip)
                        {
                            entry.LastModified = _fixedTimestamp;
                        }
                    }

                    if (!flavor.HasValue)
                    {
                        if (saveToStdout)
                            zip.Save(Console.OpenStandardOutput());
                        else
                            zip.Save();
                    }
                    else
                    {
                        if (saveToStdout)
                            throw new Exception("Cannot save SFX to stdout, sorry! See http://dotnetzip.codeplex.com/WorkItem/View.aspx?WorkItemId=7246");
                        zip.SaveSelfExtractor(args[0], flavor.Value);

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