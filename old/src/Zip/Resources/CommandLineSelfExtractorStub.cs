// CommandLineSelfExtractorStub.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008-2011 Dino Chiesa.
// All rights reserved.
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
// last saved (in emacs):
// Time-stamp: <2011-June-18 20:58:45>
//
// ------------------------------------------------------------------
//
// This is a the source module that implements the stub of a
// command-line self-extracting Zip archive - the code included in all
// command-line SFX files.  This code is included as a resource into the
// DotNetZip DLL, and then is compiled at runtime when a SFX is saved.
//
// ------------------------------------------------------------------


namespace Ionic.Zip
{
    // include the using statements inside the namespace declaration,
    // because source code will be concatenated together before
    // compilation.
    using System;
    using System.Reflection;
    using System.Resources;
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Ionic.Zip;

    public class CommandLineSelfExtractor
    {
        const string DllResourceName = "Ionic.Zip.dll";

        string TargetDirectory = "@@EXTRACTLOCATION";
        string PostUnpackCmdLine = "@@POST_UNPACK_CMD_LINE";
        bool ReplacedEnvVarsForTargetDirectory;
        bool ReplacedEnvVarsForCmdLine;
        bool ListOnly;
        bool Verbose;
        bool ReallyVerbose;
        bool RemoveFilesAfterExe;
        bool SkipPostUnpackCommand;
        string Password = null;

        // cannot include the following line, because of our use of
        // the AssemblyResolver event.

        //Ionic.Zip.ExtractExistingFileAction Overwrite;
        int Overwrite;

        // Attention: it isn't possible, with the design of this class as it is
        // now, to have a member variable of a type from the Ionic.Zip assembly.
        // The class design registers an assembly resolver, but apparently NOT in
        // time to allow the assembly to be used in private instance variables.

        private bool PostUnpackCmdLineIsSet()
        {
            // What is going on here?
            // The PostUnpackCmdLine is initialized to a particular value, then
            // we test to see if it begins with the first two chars of that value,
            // and ends with the last part of the value.  Why?

            // Here's the thing.  In order to insure the code is right, this module has
            // to compile as it is, as a standalone module.  But then, inside
            // DotNetZip, when generating an SFX, we do a text.Replace on the source
            // code, potentially replacing @@POST_UNPACK_CMD_LINE with an actual value.
            // The test here checks to see if it has been set.

            bool result = !(PostUnpackCmdLine.StartsWith("@@") &&
                     PostUnpackCmdLine.EndsWith("POST_UNPACK_CMD_LINE"));

            if (result && ReplacedEnvVarsForCmdLine == false)
            {
                PostUnpackCmdLine= ReplaceEnvVars(PostUnpackCmdLine);
                ReplacedEnvVarsForCmdLine = true;
            }

            return result;
        }


        private bool TargetDirectoryIsSet()
        {
            bool result = !(TargetDirectory.StartsWith("@@") &&
                     TargetDirectory.EndsWith("EXTRACTLOCATION"));

            if (result && ReplacedEnvVarsForTargetDirectory == false)
            {
                TargetDirectory= ReplaceEnvVars(TargetDirectory);
                ReplacedEnvVarsForTargetDirectory = true;
            }
            return result;
        }



        private string ReplaceEnvVars(string s)
        {
            System.Collections.IDictionary envVars = Environment.GetEnvironmentVariables();
            foreach (System.Collections.DictionaryEntry de in envVars)
            {
                string t = "%" + de.Key + "%";
                s= s.Replace(t, de.Value as String);
            }

            return s;
        }


        private bool SetRemoveFilesFlag()
        {
            bool result = false;
            Boolean.TryParse("@@REMOVE_AFTER_EXECUTE", out result);
            RemoveFilesAfterExe = result;
            return result;
        }


        private bool SetVerboseFlag()
        {
            bool result = false;
            Boolean.TryParse("@@QUIET", out result);
            Verbose = !result;
            return Verbose;
        }

        private int SetOverwriteBehavior()
        {
            Int32 result = 0;
            Int32.TryParse("@@EXTRACT_EXISTING_FILE", out result);
            Overwrite = (int) result;
            return result;
        }


        // ctor
        private CommandLineSelfExtractor()
        {
            SetRemoveFilesFlag();
            SetVerboseFlag();
            SetOverwriteBehavior();
            PostUnpackCmdLineIsSet();
            TargetDirectoryIsSet();
        }


        // ctor
        public CommandLineSelfExtractor(string[] args) : this()
        {
            string specifiedDirectory = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-d":
                        i++;
                        if (args.Length <= i)
                        {
                            Console.WriteLine("please supply a directory.\n");
                            GiveUsageAndExit();
                        }
                        if (specifiedDirectory != null)
                        {
                            Console.WriteLine("You already provided a directory.\n");
                            GiveUsageAndExit();
                        }
                        specifiedDirectory = args[i];
                        break;
                    case "-p":
                        i++;
                        if (args.Length <= i)
                        {
                            Console.WriteLine("please supply a password.\n");
                            GiveUsageAndExit();
                        }
                        if (Password != null)
                        {
                            Console.WriteLine("You already provided a password.\n");
                            GiveUsageAndExit();
                        }
                        Password = args[i];
                        break;
                    case "-o":
                        Overwrite = 1;
                        //WantOverwrite = ExtractExistingFileAction.OverwriteSilently;
                        break;
                    case "-n":
                        Overwrite= 2;
                        //WantOverwrite = ExtractExistingFileAction.DoNotOverwrite;
                        break;
                    case "-l":
                        ListOnly = true;
                        break;
                    case "-r+":
                        RemoveFilesAfterExe = true;
                        break;
                    case "-r-":
                        RemoveFilesAfterExe = false;
                        break;
                    case "-x":
                        SkipPostUnpackCommand = true;
                        break;
                    case "-?":
                        GiveUsageAndExit();
                        break;
                    case "-v-":
                        Verbose = false;
                        break;
                    case "-v+":
                        if (Verbose)
                            ReallyVerbose = true;
                        else
                            Verbose = true;
                        break;
                    default:
                        Console.WriteLine("unrecognized argument: '{0}'\n", args[i]);
                        GiveUsageAndExit();
                        break;
                }
            }


            if (!ListOnly)
            {
                if (specifiedDirectory!=null)
                    TargetDirectory = specifiedDirectory;
                else if (!TargetDirectoryIsSet())
                    TargetDirectory = ".";  // cwd
            }

            if (ListOnly && ((Overwrite!= 0) || (specifiedDirectory != null)))
            {
                Console.WriteLine("Inconsistent options.\n");
                GiveUsageAndExit();
            }
        }


        // workitem 8988
        private string[] SplitCommandLine(string cmdline)
        {
            // if the first char is NOT a double-quote, then just split the line
            if (cmdline[0]!='"')
                return cmdline.Split( new char[] {' '}, 2);

            // the first char is double-quote.  Need to verify that there's another one.
            int ix = cmdline.IndexOf('"', 1);
            if (ix == -1) return null;  // no double-quote - FAIL

            // if the double-quote is the last char, then just return an array of ONE string
            if (ix+1 == cmdline.Length) return new string[] { cmdline.Substring(1,ix-1) };

            if (cmdline[ix+1]!= ' ') return null; // no space following the double-quote - FAIL

            // there's definitely another double quote, followed by a space
            string[] args = new string[2];
            args[0] = cmdline.Substring(1,ix-1);
            while (cmdline[ix+1]==' ') ix++;  // go to next non-space char
            args[1] = cmdline.Substring(ix+1);
            return args;
        }


        static CommandLineSelfExtractor()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Resolver);
        }


        static System.Reflection.Assembly Resolver(object sender, ResolveEventArgs args)
        {
            // super defensive
            Assembly a1 = Assembly.GetExecutingAssembly();
            if (a1==null)
                throw new Exception("GetExecutingAssembly returns null.");

            string[] tokens = args.Name.Split(',');
            String[] names = a1.GetManifestResourceNames();

            if (names==null)
                throw new Exception("GetManifestResourceNames returns null.");

            // workitem 7978
            Stream s = null;
            foreach (string n in names)
            {
                string root = n.Substring(0,n.Length-4);
                string ext = n.Substring(n.Length-3);
                if (root.Equals(tokens[0])  && ext.ToLower().Equals("dll"))
                {
                    s= a1.GetManifestResourceStream(n);
                    if (s!=null) break;
                }
            }

            if (s==null)
                throw new Exception(String.Format("GetManifestResourceStream returns null. Available resources: [{0}]",
                                                  String.Join("|", names)));

            byte[] block = new byte[s.Length];

            if (block==null)
                throw new Exception(String.Format("Cannot allocated buffer of length({0}).", s.Length));

            s.Read(block, 0, block.Length);
            Assembly a2 = Assembly.Load(block);
            if (a2==null)
                throw new Exception("Assembly.Load(block) returns null");

            return a2;
        }



        public int Run()
        {
            //System.Diagnostics.Debugger.Break();

            List<String> itemsExtracted= new List<String>();

            global::Ionic.Zip.ExtractExistingFileAction WantOverwrite =
                (Ionic.Zip.ExtractExistingFileAction) Overwrite;

            // There way this works:  the EXE is a ZIP file.  So
            // read from the location of the assembly, in other words the path to the exe.
            Assembly a = Assembly.GetExecutingAssembly();

            int rc = 0;
            try
            {
                // workitem 7067
                using (global::Ionic.Zip.ZipFile zip = global::Ionic.Zip.ZipFile.Read(a.Location))
                {
                    if (Verbose)
                        Console.WriteLine("Command-Line Self Extractor generated by DotNetZip.");

                    if (!ListOnly)
                    {
                        if (Verbose)
                        {
                            Console.Write("Extracting to {0}", TargetDirectory);
                            System.Console.WriteLine(" (Existing file action: {0})", WantOverwrite.ToString());
                        }
                    }

                    bool header = true;
                    foreach (global::Ionic.Zip.ZipEntry entry in zip)
                    {
                        if (ListOnly || ReallyVerbose)
                        {
                            if (header)
                            {
                                System.Console.WriteLine("Extracting Zip file: {0}", zip.Name);
                                if ((zip.Comment != null) && (zip.Comment != ""))
                                    System.Console.WriteLine("Comment: {0}", zip.Comment);

                                System.Console.WriteLine("\n{1,-22} {2,9}  {3,5}   {4,9}  {5,3} {6,8} {0}",
                                             "Filename", "Modified", "Size", "Ratio", "Packed", "pw?", "CRC");
                                System.Console.WriteLine(new System.String('-', 80));
                                header = false;
                            }

                            System.Console.WriteLine("{1,-22} {2,9} {3,5:F0}%   {4,9}  {5,3} {6:X8} {0}",
                                         entry.FileName,
                                         entry.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                                         entry.UncompressedSize,
                                         entry.CompressionRatio,
                                         entry.CompressedSize,
                                         (entry.UsesEncryption) ? "Y" : "N",
                                         entry.Crc);

                        }

                        if (!ListOnly)
                        {
                            if (Verbose && !ReallyVerbose)
                                System.Console.WriteLine("  {0}", entry.FileName);

                            if (entry.Encryption == global::Ionic.Zip.EncryptionAlgorithm.None)
                            {
                                try
                                {
                                    entry.Extract(TargetDirectory, WantOverwrite);
                                    itemsExtracted.Add(entry.FileName);
                                }
                                catch (Exception ex1)
                                {
                                    Console.WriteLine("  Error -- {0}", ex1.Message);
                                    rc++;
                                }
                            }
                            else
                            {
                                if (Password == null)
                                {
                                    Console.WriteLine("Cannot extract entry {0} without a password.", entry.FileName);
                                    rc++;
                                }
                                else
                                {
                                    try
                                    {
                                        entry.ExtractWithPassword(TargetDirectory, WantOverwrite, Password);
                                        itemsExtracted.Add(entry.FileName);
                                    }
                                    catch (Exception ex2)
                                    {
                                        Console.WriteLine("  Error -- {0}", ex2.Message);
                                        rc++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("The self-extracting zip file is corrupted.");
                return 4;
            }

            if (rc != 0) return rc;


            // potentially execute the embedded command
            if (PostUnpackCmdLineIsSet() && !SkipPostUnpackCommand)
            {
                if (ListOnly)
                {
                    Console.WriteLine("\nExecute on unpack: {0}", PostUnpackCmdLine);
                }
                else
                {
                    try
                    {
                        string[] args = SplitCommandLine(PostUnpackCmdLine);

                        if (args!= null && args.Length > 0)
                        {
                            if (Verbose)
                                System.Console.WriteLine("Running command:  {0}", PostUnpackCmdLine);

                            ProcessStartInfo startInfo = new ProcessStartInfo(args[0]);
                            startInfo.WorkingDirectory = TargetDirectory;
                            startInfo.CreateNoWindow = true;
                            if (args.Length > 1) startInfo.Arguments = args[1];

                            using (Process p = Process.Start(startInfo))
                            {
                                if (p!=null)
                                {
                                    p.WaitForExit();
                                    rc = p.ExitCode;
                                    // workitem 8925
                                    if (p.ExitCode == 0)
                                    {
                                        if (RemoveFilesAfterExe)
                                        {
                                            foreach (string s in itemsExtracted)
                                            {
                                                string fullPath = Path.Combine(TargetDirectory,s);
                                                try
                                                {
                                                    if (File.Exists(fullPath))
                                                        File.Delete(fullPath);
                                                    else if (Directory.Exists(fullPath))
                                                        Directory.Delete(fullPath, true);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                            }


                        }
                    }
                    catch (Exception exc1)
                    {
                        System.Console.WriteLine("{0}", exc1);
                        rc = 5;
                    }

                }
            }

            return rc;
        }



        private void GiveUsageAndExit()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string s = Path.GetFileName(a.Location);
            Console.WriteLine("DotNetZip Command-Line Self Extractor, see http://DotNetZip.codeplex.com/");
            Console.WriteLine("Copyright (c) 2008-2011 Dino Chiesa.");

            Console.WriteLine("usage:\n  {0} [-p <password>] [-d <directory>]", s);

            string more = "    Extracts entries from the archive. If any files to be extracted already\n" +
                          "    exist, the program will stop.\n\n    Additional Options:\n" +
                          "{0}" +
                          "{1}" +
                          "{2}" +
                          "{3}";

            string overwriteString =
                          String.Format("      -o    overwrite any existing files upon extraction{0}.\n" +
                                        "      -n    do not overwrite any existing files upon extraction{1}.\n",
                                        (Overwrite == 1) ? " (default)" : "",
                                        (Overwrite == 2) ? " (default)" : "");

            string removeString = PostUnpackCmdLineIsSet()
                ? String.Format("      -r+   remove files after the optional post-unpack exe completes{0}.\n" +
                                "      -r-   don't remove files after the optional post-unpack exe completes{1}.\n",
                                RemoveFilesAfterExe ? " (default)" : "",
                                RemoveFilesAfterExe ?  "" : " (default)")
                : "";

            string verbString = String.Format("      -v-   turn OFF verbose messages{0}.\n"+
                                              "      -v+   turn ON verbose messages{1}.\n",
                                              Verbose ?  "" : " (default)",
                                              Verbose ? " (default)" : "");

            string cmdString = PostUnpackCmdLineIsSet()
                ? String.Format("      -x    don't run the post-unpack exe.\n            [cmd is: {0}]\n",
                              PostUnpackCmdLine)
                : "" ;

            Console.WriteLine(more, overwriteString, removeString, cmdString, verbString);


            if (TargetDirectoryIsSet())
                Console.WriteLine("    default extract dir: [{0}]\n", TargetDirectory);


            Console.WriteLine("  {0} -l", s);
            Console.WriteLine("    Lists entries in the archive.");
            FreeConsole();
            Environment.Exit(1);
        }


        [STAThread]
        public static int Main(string[] args)
        {
            int left = 0;
            int top = 0;
            try
            {
                left = Console.CursorLeft;
                top = Console.CursorTop;
            }
            catch { } // suppress

            bool wantPause = (left==0 && top==0);
            int rc = 0;
            try
            {
                CommandLineSelfExtractor me = new CommandLineSelfExtractor(args);

                // Hide my own console window if there is no parent console
                // (which means, it was launched rom explorer).
                if (!me.Verbose)
                {
                    IntPtr myHandle = Process.GetCurrentProcess().MainWindowHandle;
                    ShowWindow(myHandle, SW_HIDE);
                }

                rc = me.Run();

                // If there was an error, and this is a new console, and
                // we're still displaying the console, then do a
                // ReadLine.  This gives the user a chance to read the
                // window error messages before dismissing.
                if (rc != 0 && wantPause && me.Verbose)
                {
                    //Console.WriteLine("rc({0})  wantPause({1}) verbose({2})", rc, wantPause, me.Verbose);
                    Console.Write("<ENTER> to continue...");
                    Console.ReadLine();
                }

            }
            catch (System.Exception exc1)
            {
                Console.WriteLine("Exception while extracting: {0}", exc1.ToString());
                rc = 255;
            }

            FreeConsole();
            return rc;
        }

        private static readonly int SW_HIDE= 0;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

    }
}
