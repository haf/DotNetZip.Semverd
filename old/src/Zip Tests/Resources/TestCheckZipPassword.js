// TestCheckZip.js
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa.
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
// last saved: <2011-June-13 17:04:58>
//
// ------------------------------------------------------------------
//
// This is a script file that calls into the static ZipFile.CheckZipPassword
// method via the ComHelper class.  This script is used for
// compatibility testing of the DotNetZip output.
//
// created Mon, 13 Jun 2011  16:50
//
// ------------------------------------------------------------------



function checkZipPassword(filename, passwd) {
    var obj = new ActiveXObject("Ionic.Zip.ComHelper");
    return obj.CheckZipPassword(filename, passwd);
}


function main() {
    var result;
    var args = WScript.Arguments;

    if (args.Length == 2) {
        result = checkZipPassword(args(0), args(1));
    }
    else {
        WScript.Echo("TestCheckZipPassword.js - check a zipfile using Javascript.");
        WScript.Echo("  usage: TestCheckZipPassword.js <pathToZip> <password>");
        WScript.Quit(1);
    }

    WScript.Echo((result==0)?"That zip is not OK":"That zip is OK");
    WScript.Quit(0);
}


main();

