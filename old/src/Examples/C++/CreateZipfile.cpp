// CreateZipFile.cpp
//
// Example: creating a zip file from C++.
//
// This code is part of DotNetZip.
//
//
// Last saved: <2010-June-01 10:32:15>
//


using namespace System;
using namespace Ionic::Zip;

int main(array<System::String ^> ^args)
{
    Console::WriteLine(L"Hello World");
    Console::WriteLine(L"Creating a zip file from C++/CLI using DotNetZip...");

    ZipFile ^ zip;
    try
    {
        zip = gcnew ZipFile();
        zip->AddEntry("Readme.txt", "This is the content for the Readme.txt entry.");
        zip->AddFile("CreateZipFile.cpp");
        zip->Save("test.zip");
    }
    finally
    {
        //zip->~ZipFile();
        delete zip;
    }

    Console::WriteLine(L"Press <ENTER> to quit.");
    Console::ReadLine();
    return 0;
}

