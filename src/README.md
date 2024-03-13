ProDotNetZip - NET Standard part of DotNetZip / Ionic's Zip Library
-------------------------------------------------------------------

DotNetZip is the name of an open-source project that delivers a .NET
library for handling ZIP files, and some associated tools.

ProDotNetZip is rip off the original code with limitation to NET standard subset.

 - The library allows .NET programmers to build applications
   that read, create and modify ZIP files.

 - The tools are .NET programs that rely on the library, and can be used
   by anyone on any Windows machine to build or extract ZIP files.

Original library: https://github.com/DinoChiesa/DotNetZip
SemVer updated library: https://github.com/haf/DotNetZip.Semverd

Namespace was left untouched as `Ionic.Zip`.


## How to build?

`dotnet build`


License
--------

This software is open source. It is released under the Microsoft Public
License of October 2006. See the License.txt file for details.

DotNetZip is derived in part from ZLIB, the C-language library by Mark
Adler and Jean-loup Gailly . See the License.ZLIB.txt file included in
the DotNetZip download for details.



Using the Zip Class Library: The Basics
----------------------------------------

The examples here provide just the basics.

There are many other examples available: some are included in the source
package, some in the class reference documentation in the help file, and
others on the web.  Those examples provide many illustrate how to read
and write zip files, taking advantage of all the various features of zip
files exposed by the library.  For a full set of examples, your best bet
is to see the documentation. Here's a basic primer:

The main type you will use to fiddle with zip files is the ZipFile
class. Full name: Ionic.Zip.ZipFile.  You use this to create, read, or
update zip files.  There is also a ZipOutputStream class, which offers a
Stream metaphor, for those who want it. You should choose one or the
other for your application.

The simplest way to create a ZIP file in C# looks like this:

      using (ZipFile zip = new ZipFile())
      {
        zip.AddFile(filename);
        zip.Save(NameOfZipFileTocreate);
      }



The using clause is important; don't leave it out.


The simplest way to Extract all the entries from a zipfile looks
like this:

      using (ZipFile zip = ZipFile.Read(NameOfExistingZipFile))
      {
        zip.ExtractAll(args[1]);
      }

But you could also do something like this:

      using (ZipFile zip = ZipFile.Read(NameOfExistingZipFile))
      {
        foreach (ZipEntry e in zip)
        {
          e.Extract();
        }
      }


That covers the basics.

Notice that a using clause is always employed. Don't forget this.
Don't leave it off. If you don't understand what it is, don't just skip it.
It's important.

There are a number of other options for using the class library.  For
example, you can read zip archives from streams, or you can create
(write) zip archives to streams, or you can extract into streams.  You
can apply passwords for weak encryption.  You can specify a code page
for the filenames and metadata of entries in an archive.  You can rename
entries in archives, and you can add or remove entries from archives.
You can set up save and read progress events. You can do LINQ queries on
the Entries collection.  Check the documentation for complete
information, or use Visual Studio's intellisense to explore some of the
properties and methods on the ZipFile class.

Another type you will use is ZipEntry. This represents a single entry -
either a file or a directory - within a ZipFile.  To add an entry to a
zip file, you call one of the AddEntry (or AddFile) methods on the
ZipFile class.  You never directly instantiate a ZipEntry type.  The
AddEntry/AddFile returns a ZipEntry type; you can then modify the
properties of the entry within the zip file, using that object.

For example, the following code adds a file as an entry into a ZipFile,
then renames the entry within the zip file:

      using (ZipFile zip = new ZipFile())
      {
        ZipEntry e = zip.AddFile(filename);
        e.FileName = "RenamedFile.txt";
        zip.Save(NameOfZipFileTocreate);
      }

Extracting a zip file that was created in this way will produce a file
called "RenamedFile.txt", regardless of the name of the file originally
added to the ZipFile.


As an alternative to using ZipFile type to create a zip file, you can
use the ZipOutputStream type to create zip files .  To do so, wrap it
around a stream, and write to it.

      using (var fs = File.Create(filename))
      {
        using(var s = new ZipOutputStream(fs))
        {
          s.PutNextEntry("entry1.txt");
          byte[] buffer = Encoding.ASCII.GetBytes("This is the content for entry #1.");
          s.Write(buffer, 0, buffer.Length);
        }
      }

Unlike the ZipFile class, the ZipOutputStream class can only create zip
files. It cannot read or update zip files.

If you want to read zip files using a streaming metaphor, you can use
ZipInputStream.  Think of ZipInputStream and ZipOutputStream as
alternatives to using ZipFile to manipulate zip files. The former is for
reading zip files; the latter is for writing them.



About Directory Paths
---------------------------------

One important note: the `ZipFile.AddXxx` methods add the file or
directory you specify, including the directory.  In other words,
logic like this:
    ZipFile zip = new ZipFile();
    zip.AddFile("c:\\a\\b\\c\\Hello.doc");
    zip.Save();

...will produce a zip archive that contains a single entry, or file, and
that file is stored with the relative directory information.  When you
extract that file from the zip, either using this Zip library or winzip
or the built-in zip support in Windows, or some other package, all those
directories will be created, and the file will be written into that
directory hierarchy.  At extraction time, if you were to extract that
file into a directory like `c:\documents`, then resulting file would be
named `c:\documents\a\b\c\Hello.doc`.

This is by design.

If you don't want that directory information in your archive,
then you need to use the overload of the `AddFile()` method that
allows you to explicitly specify the directory used for the entry
within the archive:

    zip.AddFile("c:\\a\\b\\c\\Hello.doc", "files");
    zip.Save();

This will create an archive with an entry called "files\Hello.doc",
which contains the contents of the on-disk file located at
c:\a\b\c\Hello.doc .

If you extract that file into a directory e:\documents, then the
resulting file will be called e:\documents\files\Hello.doc .

If you want no directory at all, specify "" (the empty string).
Specifying null will include all the directory hierarchy
in the filename, as in the orginal case.




Pre-requisites to run Applications that use ProDotNetZip
-----------------------------------------------------

ProDotNetZip contains only subset of original DotNetZip
library that is compilable in .NET Standard 2.0, therefore 
usable in .NET Framework, .NET Core, .NET.

Assembly should be possible to use in eg.
  * .NET Framework 4.8
  * NET 6 LTS
  * NET 8 LTS
  * NET 9


In more detail: ProDotNetZip Class Library
----------------------------------------------

## Zip Library

The Zip library allows applications to create, read, and update zip files.

This library uses the DeflateStream class to compress file data,
and extends it to support reading and writing of the metadata -
the header, CRC, and other optional data - defined or required
by the zip format spec.

The key object in the class library is the ZipFile class.  Some of the
important methods on it:

      - AddItem - adds a file or a directory to a zip archive
      - AddDirectory - adds a directory to a zip archive
      - AddFile - adds a file to a zip archive
      - AddFiles - adds a set of files to a zip archive
      - Extract - extract a single element from a zip file
      - Read - static methods to read in an existing zipfile, for
               later extraction
      - Save - save a zipfile to disk

There is also a supporting class, called ZipEntry.  Applications can
enumerate the entries in a ZipFile, via ZipEntry.  There are other
supporting classes as well.  Typically, 80% of apps will use just the
ZipFile class, and will not need to directly interact with these other
classes. But they are there if you need them.

## ZLIB Library

The ZLIB library does compression and decompression according to IETF RFC's 1950 (ZLIB),
1951 (Deflate), and 1952 (GZIP).

See http://www.ietf.org/rfc/rfc1950.txt, http://www.ietf.org/rfc/rfc1951.txt
and  http://www.ietf.org/rfc/rfc1952.txt

The key classes are:

  ZlibCodec - a class for Zlib (RFC1950/1951/1952) encoding and decoding.
        This low-level class does deflation and inflation on buffers.

  DeflateStream - patterned after the DeflateStream in
        System.IO.Compression, this class supports compression
        levels and other options.

  GZipStream - patterned after the GZipStream in
        System.IO.Compression, this class supports compression
        levels and other options.

  ZlibStream - similar to the GZipStream in
        System.IO.Compression, this class generates or consumes raw ZLIB
        streams.


## BZIP2 Library

The BZip2 library does compression according to the bzip2 format created by
Julian Seward. See http://en.wikipedia.org/wiki/Bzip2


## Summary
If you want to create or read zip files, the ProDotNetZip.dll assembly is
the one you want.



Namespace changes for ProDotNetZip
-----------------------------------------

ProDotNetZip has the same classes and namespaces as DotNetZip.
You can use it as clean replacement.

Detailed information about namespace in DotNetZip are as follow:
The namespace for the DotNetZip classes is Ionic.Zip.
Classes are like:
  Ionic.Zip.ZipFile
  Ionic.Zip.ZipEntry
  Ionic.Zip.ZipException
  etc

For the versions prior to v1.7, the namespace DotNetZip was Ionic.Utils.Zip.
The classes were like so:
  Ionic.Utils.Zip.ZipFile
  Ionic.Utils.Zip.ZipEntry
  etc

If you have code that depends on an older version of the library, with
classes in the Ionic.Utils.Zip namespace), a simple namespace
replacement will allow your code to compile against the new version of
the library.


In addition to the Zip capability, DotNetZip includes capability (new
for v1.7).  For Zlib, the classes are like this:
  Ionic.Zlib.DeflateStream
  Ionic.Zlib.ZlibStream
  Ionic.Zlib.ZlibCodec
  ...

For v1.9.1.6, the CRC class moved from the Ionic.Zlib namespace to the
Ionic.Crc namespace.



Dependencies
---------------------------------

ProDotNetZip package is dependent upon NET Standard 2.0 library and
two other libraries:
  * System.Security.Permissions
  * System.Text.Encoding.CodePages



The Documentation
--------------------------------------------

Use documentation for DotNetZip as relevant source of information at the moment.


The Zip Format
---------------------------------
The zip format is described by PKWare, at
  http://www.pkware.com/business_and_developers/developer/popups/appnote.txt


Self-Extracting Archive support
--------------------------------

The Self-Extracting Archive (SFX) is **not supported** in NET Standard version.



Support
--------------------------------------------

There is no official support for this library.



About Intellectual Property
---------------------------------

The specification for the zip format, which PKWARE owns, includes a
paragraph that reads:

  PKWARE is committed to the interoperability and advancement of the
  .ZIP format.  PKWARE offers a free license for certain technological
  aspects described above under certain restrictions and conditions.
  However, the use or implementation in a product of certain technological
  aspects set forth in the current APPNOTE, including those with regard to
  strong encryption or patching, requires a license from PKWARE.  Please
  contact PKWARE with regard to acquiring a license.

Contact pkware at: zipformat@pkware.com

This library does not do strong encryption as described by PKWare, nor
does it do patching.


This library uses a ZLIB implementation that is based on a conversion of
the jzlib project http://www.jcraft.com/jzlib/.  The license and
disclaimer required by the jzlib source license is referenced in the
relevant source files of DotNetZip, specifically in the sources for the
Zlib module.

This library uses a BZip2 implementation that is based on a conversion
of the bzip2 implementation in the Apache Commons compression library.
The Apache license is referenced in the relevant source files of
DotNetZip, specifically in the sources for the BZip2 module.



Limitations
---------------------------------

All issues that DotNetZip have this package also probably have, so use it at AS IS.



Examples
--------------------------------------------

Currently removed, use Examples from DotNetZip is neccessary.



Tests
--------------------------------------------

Tests are bundled to one test project, all tests must be green before releasing.




Origins
============================================

This library is mostly original code.

There is a GPL-licensed library called SharpZipLib that writes zip
files, it can be found at
http://www.sharpdevelop.net/OpenSource/SharpZipLib/Default.aspx

This library is not based on SharpZipLib.

I think there may be a Zip library shipped as part of the Mono
project.  This library is also not based on that.

Now that the Java class library is open source, there is at least one
open-source Java implementation for zip.  This implementation is not
based on a port of Sun's JDK code.

There is a zlib.net project from ComponentAce.com.  This library is not
based on that code.

This library is all new code, written by me, with these exceptions:

 -  the CRC32 class - see above for credit.
 -  the zlib library - see above for credit.
 -  the bzip2 compressor - see above for credit.


