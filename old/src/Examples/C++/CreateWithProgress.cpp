// CreateWithProgress.cpp
//
// Example: creating a zip file, with progress events, from C++.
//
// This code is part of DotNetZip.
//
//
// Last saved: <2010-June-01 10:32:21>
//


using namespace System;
using namespace System::IO;
using namespace Ionic::Zip;


public ref class DnzHelloCppCli
{

private:
    bool justHadByteUpdate;

public:
    DnzHelloCppCli()
        {
        }

public:
    void Run()
        {
            Console::WriteLine(L"Hello World");
            Console::WriteLine("Using DotNetZip version {0}", ZipFile::LibraryVersion);
            array<String^>^ filesToAdd = System::IO::Directory::GetFiles(".", "*.cpp");

            ZipFile ^ zip;
            try
            {
                zip = gcnew ZipFile();
                zip->Password = "Harbinger";
                zip->Encryption = EncryptionAlgorithm::WinZipAes128;
                zip->SaveProgress += gcnew EventHandler<SaveProgressEventArgs^>(this, &DnzHelloCppCli::SaveProgress);
                zip->AddEntry("Readme.txt", "This is the content for the Readme.txt entry.");
                zip->AddFiles(filesToAdd, "files");
                zip->Save("MyArchive.zip");
            }
            finally
            {
                zip->~ZipFile();
            }

            Console::WriteLine(L"Press <ENTER> to quit.");
            Console::ReadLine();
            return;
        }

public:
    void SaveProgress(Object^ sender, SaveProgressEventArgs^ e)
        {
            switch (e->EventType)
            {
                case ZipProgressEventType::Saving_Started:
                {
                    Console::WriteLine("Saving: {0}", e->ArchiveName);
                    break;
                }
                case ZipProgressEventType::Saving_BeforeWriteEntry:
                {
                    if (this->justHadByteUpdate)
                    {
                        Console::WriteLine();
                    }
                    Console::WriteLine("  Writing: {0} ({1}/{2})",
                                       e->CurrentEntry->FileName,
                                       (e->EntriesSaved + 1),
                                       e->EntriesTotal);
                    this->justHadByteUpdate = false;
                    break;
                }
                case ZipProgressEventType::Saving_AfterWriteEntry:
                {
                    if (e->CurrentEntry->InputStreamWasJitProvided)
                    {
                        e->CurrentEntry->InputStream->Close();
                        e->CurrentEntry->InputStream = nullptr;
                    }
                    break;
                }
                case ZipProgressEventType::Saving_Completed:
                {
                    this->justHadByteUpdate = false;
                    Console::WriteLine();
                    Console::WriteLine("Done: {0}", e->ArchiveName);
                    break;
                }
                case ZipProgressEventType::Saving_EntryBytesRead:
                {
                    if (this->justHadByteUpdate)
                    {
                        Console::SetCursorPosition(0, Console::CursorTop);
                    }
                    Console::Write("     {0}/{1} ({2:N0}%)",
                                   e->BytesTransferred,
                                   e->TotalBytesToTransfer,
                                   (((double) e->BytesTransferred) / (0.01 * e->TotalBytesToTransfer)));
                    this->justHadByteUpdate = true;
                    break;
                }
            }
        }

};


int main(array<System::String ^> ^args)
{
    try
    {
        DnzHelloCppCli^ me = gcnew DnzHelloCppCli();
        me->Run();
    }
    catch (Exception^ ex1)
    {
        Console::Error->WriteLine(String::Concat("exception: ", ex1));
    }
    return 0;
}

