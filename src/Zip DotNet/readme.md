# Zip DotNet description

This project contains multiple monikers (**net6.0**, **net48** and **netstandard2.1**) and links files and references projects the same way as `Zip NetStandard` project.  
It was made to simplify nuget package generation and to remove strict version dependency on *System.Security.Permissions 5.0* and *System.Text.Encoding.CodePages 5.0*.  

Metadata from nuspec is copied to property group in .csproj file.

As addition, SourceLink is enabled to allow to debug the library.

This project uses dotnet 6 sdk, so it should be installed prior to the build.

In order to generate the nuget package, just run `GenerateNuGetPackage.cmd` with parameters from within this project folder

```
    GenerateNuGetPackage.cmd 1.16.0 'true'
```
First parameter is version of the package and the assembly.  
Second is a flag indicating if this package was created from CI.

Newly created nuget packages will appear in `.nugets` folder.