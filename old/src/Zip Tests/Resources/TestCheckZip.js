// TestCheckZip.js
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa.  
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
// Time-stamp: <2009-September-08 23:09:56>
//
// ------------------------------------------------------------------
//
// This is a script file that calls into the static ZipFile.CheckZip
// method via the ComHelper class.  This script is used for
// compatibility testing of the DotNetZip output.
//
// created Tue, 08 Sep 2009  22:11
//
// ------------------------------------------------------------------



function checkZip(filename)
{
    var obj = new ActiveXObject("Ionic.Zip.ComHelper");
    return obj.IsZipFile(filename);
}

function checkZipWithExtract(filename)
{
    var obj = new ActiveXObject("Ionic.Zip.ComHelper");
    return obj.IsZipFileWithExtract(filename);
}


function main()
{
    var result;
    var args = WScript.Arguments;

    if (args.Length == 1) 
    {
        result = checkZip(args(0));
    }
    else if (args.Length == 2 && args(0) == "-x") 
    {
        result = checkZipWithExtract(args(1));
    }
    else 
    {
        WScript.Echo("TestCheckZip.js - check a zipfile using Javascript.");
        WScript.Echo("  usage: TestCheckZip.js [-x]  <pathToZip>");
        WScript.Quit(1);
    }

    WScript.Echo((result==0)?"That zip is not OK":"That zip is OK");
    WScript.Quit(0);
}


main();

