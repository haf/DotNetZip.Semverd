# -------------------------------------------------------
# SetVersion.ps1
#
# Set the version in all the AssemblyInfo.cs or AssemblyInfo.vb files in
# any subdirectory.  Also set the version in the Product.wxs file, in
# any subdirectory.
#
# usage:
#  from cmd.exe:
#     powershell.exe SetVersion.ps1  2.8.3.0
#
#  from powershell.exe prompt:
#     .\SetVersion.ps1  2.8.3.0
#
#
# This script is part of DotNetZip.
# DotNetZip is Copyright 2008-2011 Dino Chiesa.
#
# DotNetZip is licensed under the MS-PL.  See the accompanying
# License.txt file.
#
# Last Updated: <2011-August-02 19:51:41>
#
# -------------------------------------------------------


function Usage
{
  echo "Usage: ";
  echo "  from cmd.exe: ";
  echo "     powershell.exe SetVersion.ps1  2.8.3.0";
  echo " ";
  echo "  from powershell.exe prompt: ";
  echo "     .\SetVersion.ps1  2.8.3.0";
  echo " ";
}


function Update-SourceVersion
{
    Param ([string]$Version)

    $NewVersion = 'AssemblyVersion("' + $Version + '")';
    $NewFileVersion = 'AssemblyFileVersion("' + $Version + '")';

    foreach ($o in $input)
    {
        $av = select-string AssemblyVersion $o
        $fv = select-string AssemblyVersion $o

        if ($av -ne $null -or $fv -ne $null)
        {
            Write-output $o.FullName
            if ($o.Attributes -band [System.IO.FileAttributes]::ReadOnly)
            {
                # checkout the file for edit, using the tf.exe tool, and
                # passing the CodePlex authn info on cmd line.
                c:\vs2010\common7\IDE\tf.exe edit $o.FullName $env:cplogin
                if (-not $?)
                {
                    Write-output " --> The TF checkout failed. "
                    # See exit code in $LASTEXITCODE
                    exit
                }
            }
            $TmpFile = $o.FullName + ".tmp"

            get-content $o.FullName |
                %{$_ -replace 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewVersion } |
                %{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $NewFileVersion }  > $TmpFile

            move-item $TmpFile $o.FullName -force
        }
    }
}




function Update-SourceWxsVersion
{
    Param ([string]$Version)

    $NewVersion = 'productVersion     = "' +  $Version + '"';

    foreach ($o in $input)
    {
        $pv = select-string productVersion $o

        if ($pv -ne $null)
        {
            Write-output $o.FullName
            if ($o.Attributes -band [System.IO.FileAttributes]::ReadOnly)
            {
                # checkout the file for edit, using the tf.exe tool, and
                # passing the CodePlex authn info on cmd line
                c:\vs2010\common7\IDE\tf  edit $o.FullName $env:cplogin
                if (-not $?)
                {
                    Write-output " --> The TF checkout failed. "
                    # See exit code in $LASTEXITCODE
                    exit
                }
            }
            $TmpFile = $o.FullName + ".tmp"
            $newGuid = 'productId          = "'+ [System.Guid]::NewGuid().ToString() + '"'

            get-content $o.FullName |
                %{$_ -replace 'productVersion *= *"[1-9]+(\.([0-9]+|\*)){1,3}"', $NewVersion } |
                %{$_ -replace 'productId *= *"([0-9A-Fa-f]){8}-([0-9A-Fa-f]){4}-([0-9A-Fa-f]){4}-([0-9A-Fa-f]){4}-([0-9A-Fa-f]){12}"', $newGuid } > $TmpFile

            move-item $TmpFile $o.FullName -force
        }
    }
}



function Update-AllAssemblyInfoFiles ( $version )
{
  foreach ($file in "SolutionInfo.cs", "AssemblyInfo.cs", "AssemblyInfo.vb" )
  {
    get-childitem -recurse |? {$_.Name -eq $file} | Update-SourceVersion $version ;
  }
}


function Update-AllProductWxsFiles ( $version )
{
  foreach ($file in "Product.wxs", "ComRegistration.wxs" )
  {
    get-childitem -recurse |? {$_.Name -eq $file} | Update-SourceWxsVersion $version ;
  }
}


# validate arguments
$r= [System.Text.RegularExpressions.Regex]::Match($args[0], "^[0-9]+(\.[0-9]+){1,3}$");

if ($r.Success)
{
  Update-AllAssemblyInfoFiles $args[0];
  Update-AllProductWxsFiles $args[0];
}
else
{
  echo " ";
  echo "Bad Input!"
  echo " ";
  Usage ;
}
