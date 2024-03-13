setlocal

if not exist Ionic.Zip.dll (
   fsutil hardlink create "Ionic.Zip.dll" "c:\dinoch\dev\dotnet\zip\DotNetZip\Zip Full DLL\bin\Debug\Ionic.Zip.dll"
)

set CLCMD=\vc9\bin\cl.exe
set LINKCMD=\vc9\bin\link.exe
set MTCMD=c:\netsdk2.0\Bin\mt.exe

for %%F in (CreateWithProgress CreateZipFile) do call :COMPILE_ONE_APP %%F

goto ALL_DONE



--------------------------------------------
:COMPILE_ONE_APP
  @REM Arg:  basename
  set fileToBuild=%1
  %CLCMD% /Od /D "WIN32" /D "_DEBUG" /D "_UNICODE" /D "UNICODE" /FD /EHa /MDd  /Fo".\\"  -I\vc9\include -I\winsdk\include /W3 /c /Zi /clr /TP /FU "c:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll" /FU Ionic.Zip.dll %fileToBuild%.cpp

  %LINKCMD% /OUT:%fileToBuild%.exe /MANIFEST /MANIFESTFILE:"%fileToBuild%.exe.intermediate.manifest" /MANIFESTUAC:"level='asInvoker' uiAccess='false'" /DEBUG /ASSEMBLYDEBUG /PDB:%fileToBuild%.pdb /DYNAMICBASE /FIXED:No /NXCOMPAT /MACHINE:X86 /LIBPATH:\vc9\lib /LIBPATH:\winsdk\lib %fileToBuild%.obj

  %MTCMD% /outputresource:"%fileToBuild%.exe;#1" /manifest %fileToBuild%.exe.intermediate.manifest
  del %fileToBuild%.exe.intermediate.manifest
  @GOTO:EOF



--------------------------------------------
:ALL_DONE


endlocal