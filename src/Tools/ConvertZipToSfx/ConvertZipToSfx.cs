// ConvertZipToSfx.cs
// ------------------------------------------------------------------
//
// This is a command-line tool that creates a self-extracting Zip archive, given a
// standard zip archive.
// It requires the .NET Framework 2.0 on the target machine in order to run.
//
//
// The Visual Studio Project is a little weird.  There are code files that ARE NOT compiled
// during a normal build of the VS Solution.  They are marked as embedded resources.  These
// are the various "boilerplate" modules that are used in the self-extractor. These modules are:
//   WinFormsSelfExtractorStub.cs
//   WinFormsSelfExtractorStub.Designer.cs
//   CommandLineSelfExtractorStub.cs
//   PasswordDialog.cs
//   PasswordDialog.Designer.cs
//   ZipContentsDialog.cs
//   ZipContentsDialog.Designer.cs
//   FolderBrowserDialogEx.cs
//
// At design time, if you want to modify the way the GUI looks, you have to mark those modules
// to have a "compile" build action.  Then tweak em, test, etc.  Then again mark them as
// "Embedded resource".
//
//
// Author: Dinoch
// built on host: DINOCH-2
//
// ------------------------------------------------------------------
//
// Copyright (c) 2008 by Dino Chiesa
// All rights reserved!
//
//
// ------------------------------------------------------------------

using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

using Ionic.Zip;

namespace Ionic.Zip.Examples
{

    public class ConvertZipToSfx
    {
        private ConvertZipToSfx() { }

        public ConvertZipToSfx(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-extractdir":
                        if (i >= args.Length - 1 || ExtractDir != null)
                        {
                            Usage();
                            return;
                        }
                        ExtractDir = args[++i];
                        break;

                    case "-cmdline":
                        flavor = Ionic.Zip.SelfExtractorFlavor.ConsoleApplication;
                        break;

                case "-comment":
                    if (i >= args.Length-1 || ZipComment != null)
                    {
                        Usage();
                        return;
                    }
                    ZipComment = args[++i];
                    break;

                case "-exeonunpack":
                    if (i >= args.Length-1 || ExeOnUnpack != null)
                    {
                        Usage();
                        return;
                    }
                    ExeOnUnpack = args[++i];
                    break;

                case "-?":
                case "-help":
                    Usage();
                    return;

                default:
                        // positional args
                        if (ZipFileToConvert == null)
                            ZipFileToConvert = args[i];
                        else
                        {
                            Usage();
                            return;
                        }
                        break;
                }
            }
        }

        string ExeOnUnpack;
        string ZipComment;
        string ZipFileToConvert = null;
        string ExtractDir = null;
        bool _gaveUsage;
        SelfExtractorFlavor flavor = Ionic.Zip.SelfExtractorFlavor.WinFormsApplication;

        public void Run()
        {
            if (_gaveUsage) return;
            if (ZipFileToConvert == null)
            {
                Console.WriteLine("No zipfile specified.\n");
                Usage();
                return;
            }

            if (!System.IO.File.Exists(ZipFileToConvert))
            {
                Console.WriteLine("That zip file does not exist!\n");
                Usage();
                return;
            }

            Convert();
        }



        private void Convert()
        {
            string TargetName = ZipFileToConvert.Replace(".zip", ".exe");

            Console.WriteLine("Converting file {0} to SFX {1}", ZipFileToConvert, TargetName);

            var options = new ReadOptions { StatusMessageWriter = System.Console.Out };
            using (ZipFile zip = ZipFile.Read(ZipFileToConvert, options))
            {
                zip.Comment = ZipComment;
                SelfExtractorSaveOptions sfxOptions = new SelfExtractorSaveOptions();
                sfxOptions.Flavor = flavor;
                sfxOptions.DefaultExtractDirectory = ExtractDir;
                sfxOptions.PostExtractCommandLine = ExeOnUnpack;
                zip.SaveSelfExtractor(TargetName, sfxOptions );
            }
        }


        private void Usage()
        {
            Console.WriteLine("usage:");
            Console.WriteLine("  CreateSelfExtractor [-cmdline]  [-extractdir <xxxx>]  [-comment <xx>]");
            Console.WriteLine("                      [-exec <xx>] <Zipfile>");
            Console.WriteLine("  Creates a self-extracting archive (SFX) from an existing zip file.\n");
            Console.WriteLine("  options:");
            Console.WriteLine("     -cmdline       - the generated SFX will be a console/command-line exe.");
            Console.WriteLine("                      The default is that the SFX is a Windows (GUI) app.");
            Console.WriteLine("     -exec <xx>     - The command line to execute after the SFX runs.");
            Console.WriteLine("     -comment <xx>  - embed a comment into the self-extracting archive.");
            Console.WriteLine("                      It is displayed when the SFX is extracted.");
            Console.WriteLine();
            _gaveUsage = true;
        }



        public static void Main(string[] args)
        {
            try
            {
                new ConvertZipToSfx(args).Run();
            }
            catch (System.Exception exc1)
            {
                Console.WriteLine("Exception while creating the self extracting archive: {0}", exc1.ToString());
            }
        }


    }
}
