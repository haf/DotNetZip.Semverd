using System;
using Ionic.Zlib;

// Test deflate() with preset dictionary
class test_dict_deflate_inflate
{
    [STAThread]
    public static void Main(String[] arg)
    {
        try
        {
            var x = new test_dict_deflate_inflate();
            x.Run();
        }
        catch (System.Exception e1)
        {
            Console.WriteLine("Exception: " + e1);
        }
    }

    private void Run()
    {
        int rc;
        int bufferSize = 40000;
        byte[] compressedBytes = new byte[bufferSize];
        byte[] decompressedBytes = new byte[bufferSize];
        
        ZlibCodec compressingStream = new ZlibCodec();
        rc = compressingStream.InitializeDeflate(CompressionLevel.LEVEL9_BEST_COMPRESSION);
        CheckForError(compressingStream, rc, "InitializeDeflate");

        string dictionaryWord = "hello ";
        byte[] dictionary = System.Text.ASCIIEncoding.ASCII.GetBytes(dictionaryWord);
        string TextToCompress = "hello, hello!  How are you, Joe? ";
        byte[] BytesToCompress = System.Text.ASCIIEncoding.ASCII.GetBytes(TextToCompress);

        rc = compressingStream.SetDictionary(dictionary);
        CheckForError(compressingStream, rc, "SetDeflateDictionary");

        long dictId = compressingStream.Adler32;

        compressingStream.OutputBuffer = compressedBytes;
        compressingStream.NextOut = 0;
        compressingStream.AvailableBytesOut = bufferSize;

        compressingStream.InputBuffer = BytesToCompress;
        compressingStream.NextIn = 0;
        compressingStream.AvailableBytesIn = BytesToCompress.Length;

        rc = compressingStream.Deflate(ZlibConstants.Z_FINISH);
        if (rc != ZlibConstants.Z_STREAM_END)
        {
            System.Console.Out.WriteLine("deflate should report Z_STREAM_END");
            System.Environment.Exit(1);
        }
        rc = compressingStream.EndDeflate();
        CheckForError(compressingStream, rc, "deflateEnd");

        ZlibCodec decompressingStream = new ZlibCodec();

        decompressingStream.InputBuffer = compressedBytes;
        decompressingStream.NextIn = 0;
        decompressingStream.AvailableBytesIn = bufferSize;

        rc = decompressingStream.InitializeInflate();
        CheckForError(decompressingStream, rc, "inflateInit");
        decompressingStream.OutputBuffer = decompressedBytes;
        decompressingStream.NextOut = 0;
        decompressingStream.AvailableBytesOut = decompressedBytes.Length;

        while (true)
        {
            rc = decompressingStream.Inflate(ZlibConstants.Z_NO_FLUSH);
            if (rc == ZlibConstants.Z_STREAM_END)
            {
                break;
            }
            if (rc == ZlibConstants.Z_NEED_DICT)
            {
                if ((int)decompressingStream.Adler32 != (int)dictId)
                {
                    System.Console.Out.WriteLine("unexpected dictionary");
                    System.Environment.Exit(1);
                }
                rc = decompressingStream.SetDictionary(dictionary);
            }
            CheckForError(decompressingStream, rc, "inflate with dict");
        }

        rc = decompressingStream.EndInflate();
        CheckForError(decompressingStream, rc, "EndInflate");

        int j = 0;
        for (; j < decompressedBytes.Length; j++)
            if (decompressedBytes[j] == 0)
                break;

        var result = System.Text.ASCIIEncoding.ASCII.GetString(decompressedBytes, 0, j);

        Console.WriteLine("orig length: {0}", TextToCompress.Length);
        Console.WriteLine("compressed length: {0}", compressingStream.TotalBytesOut);
        Console.WriteLine("decompressed length: {0}", decompressingStream.TotalBytesOut);
        Console.WriteLine("result length: {0}", result.Length);
        Console.WriteLine("result of inflate:\n{0}", result);
    }

    internal static void CheckForError(ZlibCodec z, int err, System.String msg)
    {
        if (err != ZlibConstants.Z_OK)
        {
            if (z.Message != null)
                System.Console.Out.Write(z.Message + " ");
            System.Console.Out.WriteLine(msg + " error: " + err);

            System.Environment.Exit(1);
        }
    }
}