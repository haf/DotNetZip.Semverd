@echo off
goto START

-------------------------------------------------------
 MakeReleaseZips.bat

 Makes the zips, msi's, and chm for the DotNetZip release content.

 created: Thu, 19 Jun 2008  22:17

 This batch file is part of DotNetZip.
 DotNetZip is Copyright 2008-2011 Dino Chiesa.

 DotNetZip is licensed under the MS-PL.  See the accompanying
 License.txt file.

 Last Updated: <2011-August-06 01:37:07>

-------------------------------------------------------


:START

setlocal
set baseDir=%~dps0
set SNEXE=c:\winsdk\bin\sn.exe
set MSBUILD=c:\.net4.0\msbuild.exe
set POWERSHELL=c:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe
@REM set zipit=c:\users\dino\bin\zipit.exe
set zipit=%baseDir%tools\Zipit\bin\Debug\zipit.exe
set ExpectedSignedDlls=32
set stamp=%DATE% %TIME%
set stamp=%stamp:/=-%
set stamp=%stamp: =-%
set stamp=%stamp::=%


@REM get the version:
for /f "delims==" %%I in ('type SolutionInfo.cs ^| c:\bin\grep.exe AssemblyVersion ^| c:\bin\sed.exe -e "s/^.*(.\(.*\).).*/\1 /"') do set longversion=%%I

set version=%longversion:~0,3%
echo version is %version%

%MSBUILD% DotNetZip.sln /p:Configuration=Debug
%MSBUILD% DotNetZip.sln /p:Configuration=Release

call :CheckSignatures
if ERRORLEVEL 1 (
  echo exiting.
  exit /b 1
)

 set releaseDir=releases\v%version%-%stamp%
 echo making release dir %releaseDir%
 mkdir %releaseDir%

 call :MakeDocumentation

 ::REM call :MakeIntegratedHelpMsi

 call :MakeDevelopersRedist

 call :MakeRuntimeRedist

 call :MakeSilverlightRedist

 call :MakeZipUtils

 call :MakeUtilsMsi

 call :MakeRuntimeMsi

 %POWERSHELL%  .\clean.ps1

 call :MakeSrcZip


goto :END



--------------------------------------------
:CheckSignatures

  @REM check the digital signatures on the various DLLs

  SETLOCAL EnableExtensions EnableDelayedExpansion
  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Checking signatures...
  echo.

    set verbose=1
    set rcode=0
    set ccount=0
    set okcount=0
    set notsigned=0

    for /R %%D in (*.dll) do (
      @REM don't check DLLs from some directories
      set thisfile=%%D

      echo !thisfile! | findstr TestResults >nul:
      if ERRORLEVEL 1  (
        echo !thisfile! | findstr Examples >nul:
        if ERRORLEVEL 1 (
          echo !thisfile! | findstr Tests >nul:
          if ERRORLEVEL 1 (
            echo !thisfile! | findstr \obj\ >nul:
            if ERRORLEVEL 1 (
              call :BACKTICK pubkey %SNEXE% -q -T "!thisfile!"
              set /a ccount=!ccount!+1
              If "!pubkey:~-44!"=="does not represent a strongly named assembly" (
                  set /a notsigned=!notsigned!+1
                  if %verbose% GTR 0 (echo !pubkey!)
              ) else (
                  if %verbose% GTR 0 (echo %%D  !pubkey!)
                  If /i "!pubkey:~-16!"=="edbe51ad942a3f5c" (
                      set /a okcount=!okcount!+1
                  ) else (
                      set /a rcode=!rcode!+1
                  )
              )
            )
          )
        )
      )
    )

    if %verbose% GTR 0 (
      echo Checked !ccount! DLLs
      echo !notsigned! were not signed
      echo !okcount! were signed, with the correct key
      echo !rcode! were signed, with the wrong key
    )

    if !rcode! GTR 0 (
      echo.
      echo Found !rcode! assemblies signed with an unexpected key.
      exit /b 1
    )
    if !okcount! NEQ %ExpectedSignedDlls% (
      echo.
      echo There are !okcount! correctly signed assemblies.
      echo That does not agree with the configured expected value of %ExpectedSignedDlls%.
      exit /b 1
    )

  echo.
  echo.

  endlocal

goto :EOF
-------------------------------------------------------



--------------------------------------------
:MakeDocumentation

  @REM This batch subroutine invokes MSBUILD using the shfbproj files
  @REM for Documentation.  Example output htmlhelp1 file name:
  @REM DotNetZipLib-v1.9.chm

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Building Documentation Files
  echo.

  echo build Help\HtmlHelp1.shfbproj
  %MSBUILD% /nologo /p:Configuration=Release   Help\HtmlHelp1.shfbproj

  echo build Help\HelpViewer.shfbproj
  %MSBUILD% /nologo /p:Configuration=Release   Help\HelpViewer.shfbproj

  set zipfile=DotNetZip-Documentation-v%version%.zip
  set rzipfile=%releaseDir%\%zipfile%
  echo zipfile is %rzipfile%

  %zipit% %rzipfile%  -s Readme.txt "This zip contains the documentation for DotNetZip in various help formats. This is for DotNetZip v%version%.  This package was packed %stamp%. "  -s You-can-Donate.txt  "FYI: DotNetZip is donationware.  Consider donating. It's for a good cause. http://cheeso.members.winisp.net/DotNetZipDonate.aspx"

  %zipit% %rzipfile%  -d .  -D Help\bin\HtmlHelp1 -E *.chm  -d "Help Viewer 1.0"  -D Help\bin\HelpViewer -r+ -E "(name != *.chm) AND (name != *.log)"

goto :EOF
--------------------------------------------




--------------------------------------------
:MakeDevelopersRedist

  @REM example output zipfile name:  DotNetZipLib-DevKit-v1.5.zip

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the Developer's redistributable zip...
  echo.

  set zipfile=DotNetZipLib-DevKit-v%version%.zip
  @REM  for %%f in (%releaseDir%\%zipfile%) do set rzipfile=%%~ff
  set rzipfile=%releaseDir%\%zipfile%
  echo zipfile is %rzipfile%

  %zipit% %rzipfile%  -s Contents.txt "This is the Developer's Kit package for DotNetZip v%version%.  This package was packed %stamp%.  In this zip you will find Debug and Release DLLs for the various versions of the assemblies: Ionic.Zip, Ionic.Zlib, and Ionic.BZip2.  There is a separate top-level folder for each distinct version of the DLL, and within those top-level folders there are Debug and Release folders.  In the Debug folders you will find a DLL, a PDB, and an XML file for the given library, while the Release folder will have just a DLL.  The DLL is the actual library (either Debug or Release flavor), the PDB is the debug information, and the XML file is the intellisense doc for use within Visual Studio.  There are also files containing the documentation. If you have any questions, please check the forums on http://www.codeplex.com/DotNetZip"  -s PleaseDonate.txt  "DotNetZip is donationware.  Consider donating. You'll feel good about it, and it's for a good cause. http://cheeso.members.winisp.net/DotNetZipDonate.aspx"   Readme.txt License.txt License.zlib.txt License.bzip2.txt

  %zipit% %rzipfile%  -d zip-v%version%   -s Readme.txt "DotNetZip Library Developer's Kit package,  v%version% packed %stamp%.  This is the DotNetZip library.  It includes the classes in the Ionic.Zip namespace as well as the classes in the Ionic.Zlib namespace. Use this library if you want to manipulate ZIP files within .NET applications."

  %zipit% %rzipfile%  -d zip-v%version%\Debug    -D Zip\bin\Debug Ionic.Zip.dll Ionic.Zip.xml Ionic.Zip.pdb
  %zipit% %rzipfile%  -d zip-v%version%\Release  Zip\bin\Release\Ionic.Zip.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d zip-v%version%-Reduced  -s Readme.txt "DotNetZip Reduced Library, Developer's Kit package, v%version% packed %stamp%.   This is the reduced version of the DotNetZip library.  It includes the classes in the Ionic.Zip namespace as well as the classes in the Ionic.Zlib namespace.  The reduced library differs from the full library in that it lacks the ability to save self-Extracting archives (aka SFX files), and is much smaller than the full library."

  %zipit% %rzipfile%  -d zip-v%version%-Reduced\Debug   -D "Zip Reduced\bin\Debug"   Ionic.Zip.Reduced.dll Ionic.Zip.Reduced.pdb Ionic.Zip.Reduced.XML
  %zipit% %rzipfile%  -d zip-v%version%-Reduced\Release -D "Zip Reduced\bin\Release" Ionic.Zip.Reduced.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d zip-v%version%-CompactFramework  -s Readme.txt  "DotNetZip CF Library v%version% packed %stamp%. This assembly is built for the Compact Framework v2.0 or later, and includes all the classes in the Ionic.Zip namespace, as well as all the classes in the Ionic.Zlib namespace. Use this library if you want to manipulate ZIP files in smart-device applications, and if you want to use ZLIB compression directly, or if you want to use the compressing stream classes like GZipStream, DeflateStream, or ZlibStream."

  %zipit% %rzipfile%  -d zip-v%version%-CompactFramework\Debug    -D "Zip CF\bin\Debug"   Ionic.Zip.CF.dll Ionic.Zip.CF.pdb
  %zipit% %rzipfile%  -d zip-v%version%-CompactFramework\Release  -D "Zip CF\bin\Release" Ionic.Zip.CF.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d zip-v%version%-Silverlight  -s Readme.txt  "Ionic.Zip Silverlight v%version% packed %stamp%. This is the Ionic.Zip library packaged for Silverlight 3.0 or later.  Use this library if you want to manipulate ZIP files from within Silverlight applications."

  %zipit% %rzipfile%  -d zip-v%version%-Silverlight\Debug    -D "Zip SL\bin\Debug"    Ionic.Zip.dll Ionic.Zip.pdb Ionic.Zip.XML
  %zipit% %rzipfile%  -d zip-v%version%-Silverlight\Release  -D "Zip SL\bin\Release"  Ionic.Zip.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d zlib-v%version%  -s Readme.txt  "Ionic.Zlib v%version% packed %stamp%.  This is the Ionic.Zlib assembly; it includes only the classes in the Ionic.Zlib namespace. Use this library if you want to take advantage of ZLIB compression directly, or if you want to use the compressing stream classes like GZipStream, DeflateStream, or ZlibStream."
  %zipit% %rzipfile%  -d zlib-v%version%\Debug    -D Zlib\bin\Debug    Ionic.Zlib.dll Ionic.Zlib.pdb Ionic.Zlib.XML
  %zipit% %rzipfile%  -d zlib-v%version%\Release  -D Zlib\bin\Release  Ionic.Zlib.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d zlib-v%version%-CompactFramework  -s Readme.txt  "Ionic.Zlib CF v%version% packed %stamp%. This is the Ionic.Zlib library packaged for the .NET Compact Framework v2.0 or later.  Use this library if you want to take advantage of ZLIB compression directly from within Smart device applications, using the compressing stream classes like GZipStream, DeflateStream, or ZlibStream."

  %zipit% %rzipfile%  -d zlib-v%version%-CompactFramework\Debug    -D "Zlib CF\bin\Debug"    Ionic.Zlib.CF.dll Ionic.Zlib.CF.pdb Ionic.Zlib.CF.XML
  %zipit% %rzipfile%  -d zlib-v%version%-CompactFramework\Release  -D "Zlib CF\bin\Release"  Ionic.Zlib.CF.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d zlib-v%version%-Silverlight  -s Readme.txt  "Ionic.Zlib Silverlight v%version% packed %stamp%. This is the Ionic.Zlib library packaged for Silverlight 3.0 or later.  Use this library if you want to take advantage of ZLIB compression directly from within Silverlight applications, using the compressing stream classes like GZipStream, DeflateStream, or ZlibStream."

  %zipit% %rzipfile%  -d zlib-v%version%-Silverlight\Debug    -D "Zlib SL DLL\bin\Debug"    Ionic.Zlib.dll Ionic.Zlib.pdb Ionic.Zlib.XML
  %zipit% %rzipfile%  -d zlib-v%version%-Silverlight\Release  -D "Zlib SL DLL\bin\Release"  Ionic.Zlib.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d bzip2-v%version%  -s Readme.txt  "Ionic.BZip2 v%version% packed %stamp%.  This is the Ionic.BZip2 assembly; it includes only the classes in the Ionic.BZip2 namespace. Use this library if you want to take advantage of BZip2 compression directly, via the compressing stream classes like BZip2OutputStream, or BZip2InputStream."
  %zipit% %rzipfile%  -d bzip2-v%version%\Debug    -D "BZip2\bin\Debug"    Ionic.BZip2.dll Ionic.BZip2.pdb Ionic.BZip2.XML
  %zipit% %rzipfile%  -d bzip2-v%version%\Release  -D "BZip2\bin\Release"  Ionic.BZip2.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d bzip2-v%version%-CompactFramework  -s Readme.txt  "Ionic.BZip2 CF v%version% packed %stamp%. This is the Ionic.BZip2 library packaged for the .NET Compact Framework v2.0 or later.  Use this library if you want to compress or decompress using BZip2, via the stream classes  BZip2InputStream and BZip2OutputStream."

  %zipit% %rzipfile%  -d bzip2-v%version%-CompactFramework\Debug    -D "BZip2 CF\bin\Debug"    Ionic.BZip2.CF.dll Ionic.BZip2.CF.pdb Ionic.BZip2.CF.XML
  %zipit% %rzipfile%  -d bzip2-v%version%-CompactFramework\Release  -D "BZip2 CF\bin\Release"  Ionic.BZip2.CF.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d bzip2-v%version%-Silverlight  -s Readme.txt  "Ionic.BZip2 Silverlight v%version% packed %stamp%. This is the Ionic.BZip2 library packaged for Silverlight 3.0 or later.  Use this library if you want to take advantage of BZip2 compression directly from within Silverlight applications, using the stream classes like BZip2InputStream, or BZip2OutputStream."

  %zipit% %rzipfile%  -d bzip2-v%version%-Silverlight\Debug    -D "BZip2 SL DLL\bin\Debug"    Ionic.BZip2.dll Ionic.BZip2.pdb Ionic.BZip2.XML
  %zipit% %rzipfile%  -d bzip2-v%version%-Silverlight\Release  -D "BZip2 SL DLL\bin\Release"  Ionic.BZip2.dll

  @REM --------------------------------------------

  %zipit% %rzipfile% -d Tools ^
         -s Readme.txt "These are tools that may be useful as you develop applications that manipulate zip files." ^
         -D Tools\ZipIt\bin\Release            Zipit.exe Ionic.Zip.dll ^
         -D Tools\Unzip\bin\Release            Unzip.exe ^
         -D Tools\ConvertZipToSfx\bin\Release  ConvertZipToSfx.exe ^
         -D Tools\WinFormsApp\bin\Release      DotNetZip-WinFormsTool.exe ^
         -D Tools\BZip2\bin\Release            BZip2.exe Ionic.BZip2.dll ^
         -D Tools\GZip\bin\Release             GZip.exe Ionic.Zlib.dll

  @REM --------------------------------------------

  %zipit% %rzipfile%  -d VS2008-IntegratedHelp  -s Readme.txt  "This MSI installs the DotNetZip help content into the VisualStudio Integrated help system. After installing this MSI, pressing F1 within Visual Studio, with your cursor on a type defined within the DotNetZip assembly, will open the appropriate help within Visual Studio."   -D Help-VS-Integrated\HelpIntegration\Debug DotNetZip-HelpIntegration.msi

  %zipit% %rzipfile%  -d Examples\WScript -D "Zip Tests\resources"  VbsCreateZip-DotNetZip.vbs  VbsUnZip-DotNetZip.vbs  TestCheckZip.js

  %zipit% %rzipfile%  -d Examples  -D Examples  -r+  -E "name != *.cache and name != *.*~ and name != *.suo and name != *.user and name != #*.*# and name != *.vspscc and name != Examples\*\*\bin\*.* and name != Examples\*\*\obj\*.* and name != Examples\*\bin\*.* and name != Examples\*\obj\*.*"

  %zipit% %rzipfile%  -D %releaseDir%  -E "name = DotNetZip-Documentation-*.zip"

  cd %baseDir%

goto :EOF
--------------------------------------------



--------------------------------------------
:MakeRuntimeRedist

  @REM example output zipfile name:  DotNetZipLib-Runtime-v1.5.zip

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the user's redistributable zip...
  echo.

  set zipfile=DotNetZipLib-Runtime-v%version%.zip
  set rzipfile=%releaseDir%\%zipfile%

  echo zipfile is %rzipfile%

  %zipit% %rzipfile%    -s Contents.txt "This is the redistributable package for DotNetZip v%version%.  Packed %stamp%. In this zip you will find a separate folder for each separate version of the DLL. In each folder there is a RELEASE build DLL, suitable for redistribution with your app. If you have any questions, please check the forums on http://www.codeplex.com/DotNetZip "   -s DotNetZip-is-DonationWare.txt  "DotNetZip is donationware. Consider donating. It's for a good cause. http://cheeso.members.winisp.net/DotNetZipDonate.aspx"   Readme.txt License.txt License.zlib.txt License.bzip2.txt

  %zipit% %rzipfile% -d zip-v%version% -s Readme.txt  "DotNetZip Redistributable Library v%version% packed %stamp%"  Zip\bin\Release\Ionic.Zip.dll

  %zipit% %rzipfile% -d zip-v%version%-Reduced  -s Readme.txt  "DotNetZip Reduced Redistributable Library v%version% packed %stamp%"   "Zip Reduced\bin\Release\Ionic.Zip.Reduced.dll"

  %zipit% %rzipfile% -d zip-v%version%-CompactFramework -s Readme.txt "DotNetZip Library for .NET Compact Framework v%version% packed %stamp%"   "Zip CF\bin\Release\Ionic.Zip.CF.dll"

  %zipit% %rzipfile% -d zlib-v%version% -s Readme.txt  "Ionic.Zlib Redistributable Library v%version% packed %stamp%"  Zlib\bin\Release\Ionic.Zlib.dll

  %zipit% %rzipfile% -d zlib-v%version%-CompactFramework -s Readme.txt  "Ionic.Zlib Library for .NET Compact Framework v%version% packed %stamp%"  "Zlib CF\bin\Release\Ionic.Zlib.CF.dll"

  %zipit% %rzipfile% -d bzip2-v%version% -s Readme.txt  "Ionic.BZip2 Redistributable Library v%version% packed %stamp%"  BZip2\bin\Release\Ionic.BZip2.dll

  %zipit% %rzipfile% -d bzip2-v%version%-CompactFramework -s Readme.txt "Ionic.BZip2 Library for .NET Compact Framework v%version% packed %stamp%" "BZip2 CF\bin\Release\Ionic.BZip2.CF.dll"

goto :EOF
--------------------------------------------



--------------------------------------------
:MakeSilverlightRedist

  @REM example output zipfile name:  DotNetZipLib-Silverlight-v1.9.zip

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the silverlight redistributable zip...
  echo.

  set zipfile=DotNetZipLib-Silverlight-v%version%.zip
  set rzipfile=%releaseDir%\%zipfile%

  echo zipfile is %rzipfile%

  %zipit% %rzipfile%    -s Contents.txt "This is the Silverlight redistributable package for DotNetZip v%version%.  Packed %stamp%.  There is an assembly for BZip2, one for ZLIB/Deflate/GZIP, and one for Zip, in the Release folder, and debug builds of the same in the Debug folder, for a total of 6 DLLs.  You need to reference exactly one of those DLLs; which one depends on your application requirements. If you have any questions, please check the forums on http://dotnetzip.codeplex.com/discussions "   -s DotNetZip-is-DonationWare.txt  "DotNetZip is donationware. Consider donating. It's for a good cause. http://cheeso.members.winisp.net/DotNetZipDonate.aspx"   License.txt License.zlib.txt License.bzip2.txt

  %zipit% %rzipfile%  -d Release   "Zip SL\bin\Release\Ionic.Zip.dll"  "Zlib SL DLL\bin\Release\Ionic.Zlib.dll"  "BZip2 SL DLL\bin\Release\Ionic.BZip2.dll"

  %zipit% %rzipfile%  -d Debug  -D "Zip SL\bin\Debug"  Ionic.Zip.dll  Ionic.Zip.xml Ionic.Zip.pdb -D "Zlib SL DLL\bin\Debug"  Ionic.Zlib.dll Ionic.Zlib.xml Ionic.Zlib.pdb -D "BZip2 SL DLL\bin\Debug"  Ionic.BZip2.dll  Ionic.BZip2.xml  Ionic.BZip2.pdb

goto :EOF
--------------------------------------------




--------------------------------------------
:MakeZipUtils

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the Zip Utils zip...
  echo.

  set zipfile=DotNetZipUtils-v%version%.zip

  @REM    for %%f in (%releaseDir%\%zipfile%) do set rzipfile=%%~ff

  set rzipfile=%releaseDir%\%zipfile%
  echo zipfile is %rzipfile%

  %zipit% %rzipfile%  -zc "Zip utilities v%version% packed %stamp%"    -s Contents.txt "These are the DotNetZip utilities and tools, for DotNetZip v%version%.  Packed %stamp%."   -s I-Welcome-Donations.txt  "DotNetZip is donationware.  I welcome donations. It's for a good cause. http://cheeso.members.winisp.net/DotNetZipDonate.aspx"   License.txt License.zlib.txt License.bzip2.txt

  %zipit% %rzipfile% ^
         -D Tools\ZipIt\bin\Release            Zipit.exe Ionic.Zip.dll ^
         -D Tools\Unzip\bin\Release            Unzip.exe ^
         -D Tools\ConvertZipToSfx\bin\Release  ConvertZipToSfx.exe ^
         -D Tools\WinFormsApp\bin\Release      DotNetZip-WinFormsTool.exe ^
         -D Tools\BZip2\bin\Release            BZip2.exe Ionic.BZip2.dll ^
         -D Tools\GZip\bin\Release             GZip.exe Ionic.Zlib.dll

goto :EOF
--------------------------------------------



--------------------------------------------
:MakeIntegratedHelpMsi

  @REM example output zipfile name:  DotNetZip-HelpIntegration.msi

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the Integrated help MSI...
  echo.

  c:\vs2008\Common7\ide\devenv.exe Help-VS-Integrated\HelpIntegration.sln  /build Debug  /project HelpIntegration
  echo waiting for Help-VS-Integrated\HelpIntegration\Debug\DotNetZip-HelpIntegration.msi
  c:\dinoch\dev\dotnet\AwaitFile Help-VS-Integrated\HelpIntegration\Debug\DotNetZip-HelpIntegration.msi
  @REM move  Help-VS-Integrated\HelpIntegration\Debug\DotNetZip-HelpIntegration.msi  %releaseDir%\DotNetZip-HelpIntegration.msi

goto :EOF
--------------------------------------------




--------------------------------------------
:MakeUtilsMsi

  @REM example output zipfile name:   DotNetZipUtils-v1.8.msi

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the Utils MSI...
  echo.

  cd "Setup Utils"
  %MSBUILD%  /p:Configuration=Release
  cd ..
  move "Setup Utils\bin\Release\en-us\DotNetZipUtils.msi"  %releaseDir%\DotNetZipUtils-v%version%.msi

goto :EOF
--------------------------------------------




--------------------------------------------
:MakeRuntimeMsi

  @REM example output zipfile name:   DotNetZip-Runtime-v1.8.msi

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the Runtime MSI...
  echo.

  cd "Setup Runtime"
  %MSBUILD%  /p:Configuration=Release
  cd ..
  move "Setup Runtime\bin\Release\en-us\DotNetZip-Runtime.msi"  %releaseDir%\DotNetZip-Runtime-v%version%.msi

goto :EOF
--------------------------------------------




--------------------------------------------
:MakeSrcZip

  echo.
  echo +++++++++++++++++++++++++++++++++++++++++++++++++++++++
  echo.
  echo Making the Source Zip...
  echo.

  @REM set zipfile=DotNetZip-src-v%version%.zip

  cd..
  @REM Delete any existing src zips
  for %%f in (DotNetZip-src-v*.zip) do del %%f

  %POWERSHELL% DotNetZip\ZipSrc.ps1

  @REM edit in place to remove Ionic.pfx and Ionic.snk from the csproj files

  @REM glob the filename:
  for %%f in (DotNetZip-src-v*.zip) do set actualFilename=%%f

  DotNetZip\EditCsproj.exe -z %actualFileName%

  cd %baseDir%
  move ..\%actualFileName%  %releaseDir%

goto :EOF
--------------------------------------------


--------------------------------------------
:BACKTICK
    call :GET_CMDLINE %*
    set varspec=%1
    setlocal EnableDelayedExpansion
    for /f "usebackq delims==" %%I in (`%CMDLINE%`) do set output=%%I
    endlocal & set %varspec%=%output%
    goto :EOF
--------------------------------------------


--------------------------------------------
:GET_CMDLINE
    @REM given a set of params [0..n], sets CMDLINE to
    @REM the join of params[1..n]
    setlocal enableextensions EnableDelayedExpansion
    set PRIOR=
    set PARAMS=
    shift
    :GET_PARAMs_LOOP
    if [%1]==[] goto GET_PARAMS_DONE
    set PARAMS=%PARAMS% %1
    shift
    goto GET_PARAMS_LOOP
    :GET_PARAMS_DONE
    REM strip the first space
    set PARAMS=%PARAMS:~1%
    endlocal & set CMDLINE=%PARAMS%
    goto :EOF
--------------------------------------------



:END
@if exist c:\users\dino\dev\dotnet\pronounceword.exe (c:\users\dino\dev\dotnet\pronounceword.exe All Done > nul:)
echo.
echo release zips are in %releaseDir%
echo.

endlocal

