using System;
using Ionic.Zip;

namespace ListContents
{
    class Program
    {
        private static void Usage()
        {
            Console.WriteLine("usage:\n  ListContents <zipfile>");
            Environment.Exit(1);
        }


        public static void Main(String[] args)
        {

            if (args.Length != 1) Usage();
            if (!System.IO.File.Exists(args[0]))
            {
                Console.WriteLine("That zip file does not exist!\n");
                Usage();
            }

            try
            {
                using (ZipFile zip = ZipFile.Read(args[0]))
                {
                    // This call to ExtractAll() assumes:
                    //   - none of the entries are password-protected.
                    //   - want to extract all entries to current working directory
                    //   - none of the files in the zip already exist in the directory;
                    //     if they do, the method will throw.
                    foreach (ZipEntry item in zip.EntriesSorted)
                    {
                        if (item.IsDirectory)
                        {
                            Console.WriteLine("directory: " + item.FileName);
                        }
                        else
                        {
                            Console.WriteLine(item.FileName);
                        }
                    }
                }
            }
            catch (System.Exception ex1)
            {
                System.Console.Error.WriteLine("exception: " + ex1);
            }

        }
    }
}
