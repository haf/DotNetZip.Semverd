REM @echo off
goto START

-------------------------------------------------------
 GetIcon.cmd

 Copy icon from "Zip Partial DLL\Resources" to this dir. 
 This script assumes it will be run by Visual Studio, as a prebuild script, starting with the 
 current directory of C:\dinoch\dev\dotnet\zip\DotNetZip\Tools\WinFormsApp\bin\Debug

 Sat, 07 Jun 2008  10:39

-------------------------------------------------------


:START
setlocal

cd ..\..\..\..
copy /y "Zip Partial DLL\Resources\zippedFile.ico"          Tools\WinFormsApp\zippedFile.ico


endlocal
:END



