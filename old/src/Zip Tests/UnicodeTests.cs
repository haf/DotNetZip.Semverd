// UnicodeTests.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008-2011 Dino Chiesa .
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
// Last Saved: <2011-July-06 19:29:37>
//
// ------------------------------------------------------------------
//
// This module defines the tests for the Unicode features in DotNetZip.
//
// ------------------------------------------------------------------


using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ionic.Zip;
using Ionic.Zip.Tests.Utilities;
using System.IO;

namespace Ionic.Zip.Tests.Unicode
{
    /// <summary>
    /// Summary description for UnicodeTests
    /// </summary>
    [TestClass]
    public class UnicodeTests : IonicTestClass
    {
        public UnicodeTests() : base() { }


        [TestMethod]
        public void Create_UnicodeEntries()
        {
            int i;
            string origComment = "This is a Unicode comment. "+
                                 "Chinese: 弹 出 应 用 程 序 "+
                                 "Norwegian/Danish: æøåÆØÅ. "+
                                 "Portugese: Configurações.";
            string[] formats = {
                "弹出应用程序{0:D3}.bin",
                "n.æøåÆØÅ{0:D3}.bin",
                "Configurações-弹出-ÆØÅ-xx{0:D3}.bin"
            };

            for (int k = 0; k < formats.Length; k++)
            {
                // create the subdirectory
                string subdir = Path.Combine(TopLevelDir, "files" + k);
                Directory.CreateDirectory(subdir);

                // create a bunch of files
                int numFilesToCreate = _rnd.Next(18) + 14;
                string[] filesToZip = new string[numFilesToCreate];
                for (i = 0; i < numFilesToCreate; i++)
                {
                    filesToZip[i] = Path.Combine(subdir, String.Format(formats[k], i));
                    TestUtilities.CreateAndFillFileBinary(filesToZip[i], _rnd.Next(5000) + 2000);
                }

                // create a zipfile twice, once using Unicode, once without
                for (int j = 0; j < 2; j++)
                {
                    // select the name of the zip file
                    string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("Create_UnicodeEntries_{0}_{1}.zip", k, j));
                    Assert.IsFalse(File.Exists(zipFileToCreate), "The zip file '{0}' already exists.", zipFileToCreate);

                    TestContext.WriteLine("\n\nFormat {0}, trial {1}.  filename: {2}...", k, j, zipFileToCreate);
                    string dirInArchive = String.Format("{0}-{1}", Path.GetFileName(subdir), j);

                    using (ZipFile zip1 = new ZipFile())
                    {
#pragma warning disable 618
                        zip1.UseUnicodeAsNecessary = (j == 0);
#pragma warning restore 618
                        for (i = 0; i < filesToZip.Length; i++)
                        {
                            // use the local filename (not fully qualified)
                            ZipEntry e = zip1.AddFile(filesToZip[i], dirInArchive);
                            e.Comment = String.Format("This entry encoded with {0}", (j == 0) ? "unicode" : "the default code page.");
                        }
                        zip1.Comment = origComment;
                        zip1.Save(zipFileToCreate);
                    }

                    // Verify the number of files in the zip
                    Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Length,
                            "Incorrect number of entries in the zip file.");

                    i = 0;

                    // verify the filenames are (or are not) unicode

                    var options = new ReadOptions {
                        Encoding = (j == 0) ? System.Text.Encoding.UTF8 : ZipFile.DefaultEncoding
                    };
                    using (ZipFile zip2 = ZipFile.Read(zipFileToCreate, options))
                    {
                        foreach (ZipEntry e in zip2)
                        {
                            string fname = String.Format(formats[k], i);
                            if (j == 0)
                            {
                                Assert.AreEqual<String>(fname, Path.GetFileName(e.FileName));
                            }
                            else
                            {
                                Assert.AreNotEqual<String>(fname, Path.GetFileName(e.FileName));
                            }
                            i++;
                        }


                        // according to the spec,
                        // unicode is not supported on the zip archive comment!
                        // But this library won't enforce that.
                        // We will leave it up to the application.
                        // Assert.AreNotEqual<String>(origComment, zip2.Comment);

                    }
                }
            }
        }

        string[] miscNameFormats = {
            "file{0:D3}.bin",         // keep this at index==0
            "弹出应用程序{0:D3}.bin",   // Chinese
            "codeplexの更新RSSを見てふと書いた投稿だったけど日本語情報がないかは調{0:D3}.bin", // Japanese
            "n.æøåÆØÅ{0:D3}.bin",      // greek
            "Configurações-弹出-ÆØÅ-xx{0:D3}.bin",  // portugese + Chinese
            "Â¡¢£ ¥â° €Ãƒ †œ Ñ añoAbba{0:D3.bin}",   //??
            "А Б В Г Д Є Ж Ѕ З И І К Л М Н О П Р С Т Ф Х Ц Ч Ш Щ Ъ ЪІ Ь Ю ІА {0:D3}.b", // Russian
            "Ελληνικό αλφάβητο {0:D3}.b",
            "א ב ג ד ה ו ז ח ט י " + "{0:D3}",  // I don't know what language this is
        };


        private List<string> _CreateUnicodeFiles()
        {
            // create the subdirectory
            string subdir = Path.Combine(TopLevelDir, "files");
            Directory.CreateDirectory(subdir);
            var filesToZip = new List<String>();
            // create a bunch of files in that subdir
            int numFilesToCreate = _rnd.Next(18) + 14;
            for (int i = 0; i < numFilesToCreate; i++)
            {
                int k = i % miscNameFormats.Length;
                var f = Path.Combine(subdir, String.Format(miscNameFormats[k], i));
                filesToZip.Add(f);
                TestUtilities.CreateAndFillFileBinary(f, _rnd.Next(5000) + 2000);
            }

            return filesToZip;
        }


        [TestMethod]
        public void Create_UnicodeEntries_Mixed()
        {
            var filesToZip = _CreateUnicodeFiles();

            // Using those files create a zipfile 4 times:
            // cycle 0 - UseUnicodeAsNecessary
            // cycle 1 - Nothing
            // cycle 2 - AlternateEncoding = UTF8, AlternateEncodingUsage = Always
            // cycle 3 - AlternateEncoding = UTF8, AlternateEncodingUsage = AsNecessary
            for (int j = 0; j < 4; j++)
            {
                string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("Archive-{0}.zip", j));
                Assert.IsFalse(File.Exists(zipFileToCreate), "The file already exists ({0}).", zipFileToCreate);

                using (ZipFile zip1 = new ZipFile(zipFileToCreate))
                {
                    switch (j)
                    {
#pragma warning disable 618
                        case 0:
                            zip1.UseUnicodeAsNecessary = (j == 0);
                            break;
#pragma warning restore 618
                        case 1:
                            // do nothing
                            break;
                        case 2:
                            zip1.AlternateEncoding = System.Text.Encoding.UTF8;
                            zip1.AlternateEncodingUsage = ZipOption.Always;
                            break;
                        case 3:
                            zip1.AlternateEncoding = System.Text.Encoding.UTF8;
                            zip1.AlternateEncodingUsage = ZipOption.AsNecessary;
                            break;
                    }
                    foreach (var fileToZip in filesToZip)
                    {
                        zip1.AddFile(fileToZip, "");
                    }
                    zip1.Save();
                }

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Count,
                        "Incorrect number of entries in the zip file.");

                _CheckUnicodeZip(zipFileToCreate, j);
            }
        }




        [TestMethod]
        public void Unicode_Create_ZOS_wi12634()
        {
            TestContext.WriteLine("==Unicode_Create_ZOS_wi12634()=");
            var filesToZip = _CreateUnicodeFiles();
            byte[] buffer = new byte[2048];
            int n;

            // using those files create a zipfile twice.  First cycle uses Unicode,
            // 2nd cycle does not.
            for (int j = 0; j < 2; j++)
            {
                // select the name of the zip file
                var bpath = String.Format("wi12634-{0}.zip", j);
                string zipFileToCreate = Path.Combine(TopLevelDir, bpath);
                TestContext.WriteLine("========");
                TestContext.WriteLine("Trial {0}", j);

                Assert.IsFalse(File.Exists(zipFileToCreate),
                               "The zip file '{0}' already exists.",
                               zipFileToCreate);
                TestContext.WriteLine("file {0}", zipFileToCreate);

                int excCount = 0;

                // create using ZOS
                using (var ofs = File.Open(zipFileToCreate, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var zos = new ZipOutputStream(ofs))
                    {
#pragma warning disable 618
                        if (j == 0)
                            zos.ProvisionalAlternateEncoding = System.Text.Encoding.UTF8;
#pragma warning restore 618

                        try
                        {
                            foreach (var fileToZip in filesToZip)
                            {
                                var ename = Path.GetFileName(fileToZip);
                                TestContext.WriteLine("adding entry '{0}'", ename);
                                zos.PutNextEntry(ename); // with no path
                                using (var ifs = File.OpenRead(fileToZip))
                                {
                                    while ((n = ifs.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        zos.Write(buffer, 0, n);
                                    }
                                }
                            }
                        }
                        catch (System.Exception exc1)
                        {
                            TestContext.WriteLine("Exception #{0}", excCount);
                            TestContext.WriteLine("{0}", exc1.ToString());
                            excCount++;
                        }
                    }
                }

                Assert.IsTrue(excCount==0,
                              "Exceptions occurred during zip creation.");

                // Verify the number of files in the zip
                Assert.AreEqual<int>(TestUtilities.CountEntries(zipFileToCreate), filesToZip.Count,
                        "Incorrect number of entries in the zip file.");

                _CheckUnicodeZip(zipFileToCreate, j);
                TestContext.WriteLine("Trial {0} file checks ok", j);
            }
        }



        [TestMethod]
        public void UnicodeComment_wi10392()
        {
            const string zipFileToCreate = "UnicodeComment_wi10392.zip";
            const string cyrillicComment = "Hello, Привет";

            TestContext.WriteLine("{0}", zipFileToCreate);
            TestContext.WriteLine("==== creating zip");
            using (ZipFile zip1 = new ZipFile(zipFileToCreate, Encoding.UTF8))
            {
                zip1.Comment = cyrillicComment;
                zip1.AddEntry("entry", "this is the content of the added entry");
                zip1.Save();
            }

            string comment2 = null;
            TestContext.WriteLine("==== checking zip");
            var options = new ReadOptions {
                Encoding = Encoding.UTF8
            };
            using (ZipFile zip2 = ZipFile.Read(zipFileToCreate, options))
            {
                comment2 = zip2.Comment;
            }

            Assert.AreEqual<String>(cyrillicComment, comment2,
                                    "The comments are not equal.");
        }


        [TestMethod]
        public void UnicodeUpdate_wi12744()
        {
            const string specialEntryName = "Привет.txt";

            // two passes: one that uses the old "useUnicodeAsNecessary" property,
            // and the second that uses the newer property.
            for (int k=0; k < 2; k++)
            {

                string zipFileToCreate = String.Format("UnicodeUpdate_wi12744-{0}.zip", k);

                TestContext.WriteLine("{0}", zipFileToCreate);
                TestContext.WriteLine("==== creating zip, trial {0}", k);
                using (ZipFile zip1 = new ZipFile())
                {
                    if (k==0)
                    {
#pragma warning disable 618
                        zip1.UseUnicodeAsNecessary = true;
#pragma warning restore 618
                    }
                    else
                    {
                        zip1.AlternateEncoding = System.Text.Encoding.UTF8;
                        zip1.AlternateEncodingUsage = ZipOption.AsNecessary;
                    }

                    zip1.AddEntry(specialEntryName, "this is the content of the added entry");
                    zip1.Save(zipFileToCreate);
                }


                TestContext.WriteLine("==== create a directory with 2 addl files in it");
                string subdir = Path.Combine(TopLevelDir, "files"+k);
                Directory.CreateDirectory(subdir);
                for (int i=0; i < 2; i++)
                {
                    var filename = Path.Combine(subdir, "file" + i + ".txt");
                    TestUtilities.CreateAndFillFileText(filename, _rnd.Next(5000) + 2000);
                }

                TestContext.WriteLine("====  update the zip");
                using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                {
                    zip2.AddDirectory(subdir);
                    zip2.Save();
                }

                TestContext.WriteLine("==== check the original file in the zip");
                using (ZipFile zip3 = ZipFile.Read(zipFileToCreate))
                {
                    var e = zip3[specialEntryName];
                    Assert.IsTrue(e!=null, "Entry not found");
                    Assert.IsTrue(e.FileName == specialEntryName, "name mismatch");
                }
            }
        }




        private void _CheckUnicodeZip(string filename, int j)
        {
            int i = 0;

            // Verify that the filenames do, or do not, match the
            // names that were added.  They will match if unicode
            // was used (j!=1) or if the filename used was the first
            // in the formats list (k==0).
            using (ZipFile zip2 = ZipFile.Read(filename))
            {
                foreach (ZipEntry e in zip2)
                {
                    int k = i % miscNameFormats.Length;
                    string fname = String.Format(miscNameFormats[k], i);
                    if (j != 1 || k == 0)
                    {
                        Assert.AreEqual<String>(fname, e.FileName, "cycle ({0},{1},{2})", i, j, k);
                    }
                    else
                    {
                        Assert.AreNotEqual<String>(fname, e.FileName, "cycle ({0},{1},{2})", i, j, k);
                    }
                    i++;
                }
            }
        }


        struct CodepageTrial
        {
            public string codepage;
            public string filenameFormat;
            public bool exceptionExpected; // not all codepages will yield legal filenames for a given filenameFormat
            public CodepageTrial(string cp, string format, bool except)
            {
                codepage = cp;
                filenameFormat = format;
                exceptionExpected = except;
            }
        }

        [TestMethod]
        public void Create_WithSpecifiedCodepage()
        {
            int i;
            CodepageTrial[] trials = {
                new CodepageTrial( "big5",   "弹出应用程序{0:D3}.bin", true),
                new CodepageTrial ("big5",   "您好{0:D3}.bin",        false),
                new CodepageTrial ("gb2312", "弹出应用程序{0:D3}.bin", false),
                new CodepageTrial ("gb2312", "您好{0:D3}.bin",        false),
                // insert other trials here.??
            };

            for (int k = 0; k < trials.Length; k++)
            {
                TestContext.WriteLine("");
                TestContext.WriteLine("---------------------Trial {0}....", k);
                TestContext.WriteLine("---------------------codepage: {0}....", trials[k].codepage);
                // create the subdirectory
                string subdir = Path.Combine(TopLevelDir, String.Format("trial{0}-files", k));
                Directory.CreateDirectory(subdir);

                // create a bunch of files
                int numFiles = _rnd.Next(3) + 3;
                string[] filesToZip = new string[numFiles];
                for (i = 0; i < numFiles; i++)
                {
                    filesToZip[i] = Path.Combine(subdir, String.Format(trials[k].filenameFormat, i));
                    TestUtilities.CreateAndFillFileBinary(filesToZip[i], _rnd.Next(5000) + 2000);
                }

                Directory.SetCurrentDirectory(subdir);

                // three cases: one for old-style
                // ProvisionalAlternateEncoding, one for "AsNecessary"
                // and one for "Always"
                for (int j=0; j < 3; j++)
                {

                    // select the name of the zip file
                    string zipFileToCreate = Path.Combine(TopLevelDir, String.Format("WithSpecifiedCodepage_{0}_{1}_{2}.zip",
                                                                                     k, j, trials[k].codepage));

                    TestContext.WriteLine("");
                    TestContext.WriteLine("---------------Creating zip, trial ({0},{1})....", k, j);

                    using (ZipFile zip1 = new ZipFile(zipFileToCreate))
                    {
                        switch (j)
                        {
                            case 0:
#pragma warning disable 618
                                zip1.ProvisionalAlternateEncoding = System.Text.Encoding.GetEncoding(trials[k].codepage);
#pragma warning restore 618
                                break;
                            case 1:
                                zip1.AlternateEncoding = System.Text.Encoding.GetEncoding(trials[k].codepage);
                                zip1.AlternateEncodingUsage = ZipOption.AsNecessary;
                                break;
                            case 2:
                                zip1.AlternateEncoding = System.Text.Encoding.GetEncoding(trials[k].codepage);
                                zip1.AlternateEncodingUsage = ZipOption.Always;
                                break;
                        }

                        for (i = 0; i < filesToZip.Length; i++)
                        {
                            TestContext.WriteLine("adding entry {0}", filesToZip[i]);
                            // use the local filename (not fully qualified)
                            ZipEntry e = zip1.AddFile(filesToZip[i], "");
                            e.Comment = String.Format("This entry was encoded in the {0} codepage", trials[k].codepage);
                        }
                        zip1.Save();
                    }

                    TestContext.WriteLine("\n---------------------Extracting....");
                    Directory.SetCurrentDirectory(TopLevelDir);

                    try
                    {
                        // verify the filenames are (or are not) unicode
                        var options = new ReadOptions {
                            Encoding = System.Text.Encoding.GetEncoding(trials[k].codepage)
                        };
                        using (ZipFile zip2 = ZipFile.Read(zipFileToCreate, options))
                        {
                            foreach (ZipEntry e in zip2)
                            {
                                TestContext.WriteLine("found entry {0}", e.FileName);
                                e.Extract(String.Format("trial{0}-{1}-{2}-extract", k, j, trials[k].codepage));
                            }
                        }
                    }
                    catch (Exception e1)
                    {
                        if (trials[k].exceptionExpected)
                            TestContext.WriteLine("caught expected exception");
                        else
                            throw new System.Exception("while extracting", e1);

                    }
                }

            }
            TestContext.WriteLine("\n---------------------Done.");
        }



        [TestMethod]
        public void CodePage_UpdateZip_AlternateEncoding_wi10180()
        {
            System.Text.Encoding JIS = System.Text.Encoding.GetEncoding("shift_jis");
            TestContext.WriteLine("The CP for JIS is: {0}", JIS.CodePage);
            ReadOptions options = new ReadOptions { Encoding = JIS };
            string[] filenames = {
                "日本語.txt",
                "日本語テスト.txt"
            };

            // three trials: one for old-style
            // ProvisionalAlternateEncoding, one for "AsNecessary"
            // and one for "Always"
            for (int j=0; j < 3; j++)
            {
                string zipFileToCreate = String.Format("wi10180-{0}.zip", j);

                // pass 1 - create it
                TestContext.WriteLine("Create zip, cycle {0}...", j);
                using (var zip = new ZipFile())
                {
                    switch (j)
                    {
                        case 0:
#pragma warning disable 618
                            zip.ProvisionalAlternateEncoding = JIS;
#pragma warning restore 618
                            break;
                        case 1:
                            zip.AlternateEncoding = JIS;
                            zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                            break;
                        case 2:
                            zip.AlternateEncoding = JIS;
                            zip.AlternateEncodingUsage = ZipOption.Always;
                            break;
                    }
                    zip.AddEntry(filenames[0], "This is the content for entry (" + filenames[0] + ")");
                    TestContext.WriteLine("adding file: {0}", filenames[0]);
                    zip.Save(zipFileToCreate);
                }

                // pass 2 - read and update it
                TestContext.WriteLine("Update zip...");
                using (var zip0 = ZipFile.Read(zipFileToCreate, options))
                {
                    foreach (var e in zip0)
                    {
                        TestContext.WriteLine("existing entry name: {0}  encoding: {1}",
                                              e.FileName, e.AlternateEncoding.EncodingName );
                        Assert.AreEqual<System.Text.Encoding>
                            (options.Encoding, e.AlternateEncoding);
                    }
                    zip0.AddEntry(filenames[1], "This is more content..." + System.DateTime.UtcNow.ToString("G"));
                    TestContext.WriteLine("adding file: {0}", filenames[1]);
                    zip0.Save();
                }

                // pass 3 - verify the filenames, again
                TestContext.WriteLine("Verify zip...");
                using (var zip0 = ZipFile.Read(zipFileToCreate, options))
                {
                    foreach (string f in filenames)
                    {
                        Assert.AreEqual<string>(f, zip0[f].FileName,
                                                "The FileName was not expected, (cycle {0}) ", j);
                    }
                }
            }
        }



        [TestMethod]
        public void Unicode_AddDirectoryByName_wi8984()
        {
            string format = "弹出应用程序{0:D3}.dir"; // Chinese characters
            System.Text.Encoding UTF8 = System.Text.Encoding.GetEncoding("UTF-8");

            TestContext.WriteLine("== WorkItem 8984");
            // three trials: one for old-style
            // ProvisionalAlternateEncoding, one for "AsNecessary"
            // and one for "Always"
            for (int j=0; j < 3; j++)
            {
                TestContext.WriteLine("Trial {0}", j);
                for (int n = 1; n <= 10; n++)
                {
                TestContext.WriteLine("nEntries {0}", n);
                    var dirsAdded = new System.Collections.Generic.List<String>();
                    var zipFileToCreate = String.Format("wi8984-{0}-{1:N2}.zip", j, n);
                    using (ZipFile zip1 = new ZipFile(zipFileToCreate))
                    {
                        switch (j)
                        {
                            case 0:
#pragma warning disable 618
                                zip1.UseUnicodeAsNecessary = true;
#pragma warning restore 618
                                break;
                            case 1:
                                zip1.AlternateEncoding = UTF8;
                                zip1.AlternateEncodingUsage = ZipOption.AsNecessary;
                                break;
                            case 2:
                                zip1.AlternateEncoding = UTF8;
                                zip1.AlternateEncodingUsage = ZipOption.Always;
                                break;
                        }
                        for (int i = 0; i < n; i++)
                        {
                            // create an arbitrary directory name, add it to the zip archive
                            string dirName = String.Format(format, i);
                            zip1.AddDirectoryByName(dirName);
                            dirsAdded.Add(dirName + "/");
                        }
                        zip1.Save();
                    }


                    string extractDir = String.Format("extract-{0}-{1:D3}", j, n);
                    int dirCount = 0;
                    using (ZipFile zip2 = ZipFile.Read(zipFileToCreate))
                    {
                        foreach (var e in zip2)
                        {
                            TestContext.WriteLine("dir: {0}", e.FileName);
                            Assert.IsTrue(dirsAdded.Contains(e.FileName), "Cannot find the expected entry ({0})", e.FileName);
                            Assert.IsTrue(e.IsDirectory);
                            e.Extract(extractDir);
                            dirCount++;
                        }
                    }
                    Assert.AreEqual<int>(n, dirCount);
                    TestContext.WriteLine("");
                }
                TestContext.WriteLine("");
            }
        }




    }
}
