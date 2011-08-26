.SUFFIXES: .tlb  .il .dll .cpp .cs .vb .rc .ocx .jsl .java

#_NETHOME=$(WINDIR)\Microsoft.NET\Framework\v1.1.4322
#_NETHOME=c:\.net2.0
_NETHOME=c:\.net3.5
#_NETSDK=c:\Progra~1\Microsoft.NET\SDK\v1.1\Bin
#_NETSDK=c:\netsdk2.0\bin
_NETSDK=c:\netsdk3.5\bin

_CS_DBG_FLAGS=/debug+
_CS_DLL_FLAGS=/t:library $(_CS_DBG_FLAGS)
_CS_EXE_FLAGS=/t:exe $(_CS_DBG_FLAGS)

_CSC=$(_NETHOME)\csc.exe

_BASE_IMPORTS=/R:System.dll /R:Ionic.Zlib.dll


default: Ionic.Zlib.dll ZlibDeflateInflate.exe test_dict_deflate_inflate.exe test_flush_sync.exe test_large_deflate_inflate.exe  ZlibStreamExample.exe


Ionic.Zlib.dll: ..\..\..\Zlib\obj\Debug\Ionic.Zlib.dll ..\..\..\Zlib\obj\Debug\Ionic.Zlib.pdb
        copy ..\..\..\Zlib\obj\Debug\Ionic.Zlib.* .

.cs.exe:
        if not exist  Ionic.Zlib.dll ( copy ..\..\..\Zlib\obj\Debug\Ionic.Zlib.* . )
        $(_CSC) $(_CS_EXE_FLAGS) $(_BASE_IMPORTS) /out:$@ $**


clean:
        -del ZlibDeflateInflate.exe test_dict_deflate_inflate.exe test_flush_sync.exe test_large_deflate_inflate.exe  ZlibStreamExample.exe
        -del Ionic.Zlib.dll 
        -del *.pdb

