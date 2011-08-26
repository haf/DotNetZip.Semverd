@REM "C:\Program Files\EWSoftware\Sandcastle Help File Builder\SandcastleBuilderConsole.exe" DotNetZip.shfb
c:\.net3.5\msbuild.exe  /p:Configuration=Release   Help\Dotnetzip.shfbproj
if exist c:\dinoch\dev\dotnet\pronounceword.exe (c:\dinoch\dev\dotnet\pronounceword.exe Build Complete > nul)
