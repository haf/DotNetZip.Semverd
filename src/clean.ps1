# -------------------------------------------------------
#  clean.ps1
#
#  Cleans the various project outputs.  To be used prior to zipping
#  up the source tree.
#
#  This script is part of DotNetZip.
#  DotNetZip is Copyright 2008-2011 Dino Chiesa.
#
#  DotNetZip is licensed under the MS-PL.  See the accompanying
#  License.txt file.
#
#  Last Updated: <2011-July-30 15:54:25>
#
# -------------------------------------------------------

$msbuild = "c:\.net4.0\msbuild.exe"
$dirsToClean =  @("Zip",
                  "Zip Reduced",
                  "Zip CF",
                  "Zip SL",
                  "Zip Tests",
                  "Zlib",
                  "Zlib CF",
                  "Zlib SL DLL",
                  "Zlib Tests",
                  "BZip2",
                  "BZip2 CF",
                  "BZip2 SL DLL",
                  "BZip2 Tests",
                  "Examples\C#\CreateZip",
                  "Examples\C#\ReadZip",
                  "Examples\C#\WinForms-QuickZip",
                  "Examples\C#\ZipDir",
                  "Examples\C#\ZipTreeView",
                  "Examples\VB\Quick-Unzip",
                  "Examples\VB\WinForms-DotNetZip",
                  "Examples\VB\WinForms-TreeViewZip",
                  "Tools\Unzip",
                  "Tools\BZip2",
                  "Tools\GZip",
                  "Tools\Zipit",
                  "Tools\ConvertZipToSfx",
                  "Tools\WinFormsApp"
                  )


get-childitem -filter *.csproj~ -recurse | remove-item


$saveloc = get-location

Write-Host ""
Write-Host "Running MSBuild /t:Clean ..."

foreach ($dir in $dirsToClean) {
  Write-Host ('  {0}' -f $dir)
  set-location $dir
  $expr = $msbuild + " /nologo /noconsolelogger /t:Clean /p:Configuration=Release"
  Invoke-Expression $expr
  $expr = $msbuild + " /nologo /noconsolelogger /t:Clean /p:Configuration=Debug"
  Invoke-Expression $expr
  set-location $saveloc
}

Write-Host ""
Write-Host "Removing bin and obj directories ..."

foreach ($dir in $dirsToClean) {
  Write-Host ('  {0}' -f $dir)
  set-location $dir
  foreach ($subdir in  @("bin", "obj")) {
    if (Test-Path $subdir) {
      remove-item $subdir -recurse
    }
  }
  set-location $saveloc
}


