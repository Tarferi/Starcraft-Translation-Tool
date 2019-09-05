using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace QChkUI {

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_DOS_HEADER32 {
        public ushort e_magic;       // Magic number
        public ushort e_cblp;    // Bytes on last page of file
        public ushort e_cp;      // Pages in file
        public ushort e_crlc;    // Relocations
        public ushort e_cparhdr;     // Size of header in paragraphs
        public ushort e_minalloc;    // Minimum extra paragraphs needed
        public ushort e_maxalloc;    // Maximum extra paragraphs needed
        public ushort e_ss;      // Initial (relative) SS value
        public ushort e_sp;      // Initial SP value
        public ushort e_csum;    // Checksum
        public ushort e_ip;      // Initial IP value
        public ushort e_cs;      // Initial (relative) CS value
        public ushort e_lfarlc;      // File address of relocation table
        public ushort e_ovno;    // Overlay number
        public fixed ushort e_res1[4];    // Reserved words
        public ushort e_oemid;       // OEM identifier (for e_oeminfo)
        public ushort e_oeminfo;     // OEM information; e_oemid specific
        public fixed ushort e_res2[10];    // Reserved words
        public int e_lfanew;      // File address of new exe header
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_NT_HEADERS32 {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_FILE_HEADER {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_OPTIONAL_HEADER32 {
        public MagicType Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        // PE32 contains this additional field
        public uint BaseOfData;
        public uint ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public SubSystemType Subsystem;
        public DllCharacteristicsType DllCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public IMAGE_DATA_DIRECTORY ExportTable;
        public IMAGE_DATA_DIRECTORY ImportTable;
        public IMAGE_DATA_DIRECTORY ResourceTable;
        public IMAGE_DATA_DIRECTORY ExceptionTable;
        public IMAGE_DATA_DIRECTORY CertificateTable;
        public IMAGE_DATA_DIRECTORY BaseRelocationTable;
        public IMAGE_DATA_DIRECTORY Debug;
        public IMAGE_DATA_DIRECTORY Architecture;
        public IMAGE_DATA_DIRECTORY GlobalPtr;
        public IMAGE_DATA_DIRECTORY TLSTable;
        public IMAGE_DATA_DIRECTORY LoadConfigTable;
        public IMAGE_DATA_DIRECTORY BoundImport;
        public IMAGE_DATA_DIRECTORY IAT;
        public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
        public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
        public IMAGE_DATA_DIRECTORY Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_DATA_DIRECTORY {
        public uint VirtualAddress;
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_SECTION_HEADER {
        public fixed byte Name[8];
        public uint PhysicalAddress;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_BASE_RELOCATION {
        public uint VirtualAdress;
        public uint SizeOfBlock;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_IMPORT_DESCRIPTOR {
        public uint OriginalFirstThunk;
        public uint TimeDateStamp;
        public uint ForwarderChain;
        public uint Name;
        public uint FirstThunk;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_EXPORT_DIRECTORY {
        public uint Characteristics;
        public uint TimeDateStamp;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public uint Name;
        public uint Base;
        public uint NumberOfFunctions;
        public uint NumberOfNames;
        public uint AddressOfFunctions;     // RVA from base of image
        public uint AddressOfNames;         // RVA from base of image
        public uint AddressOfNameOrdinals;  // RVA from base of image
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_IMPORT_BY_NAME {
        public ushort Hint;
        public fixed byte Name[1];
    }

    public enum MagicType : ushort {
        IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
        IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
    }

    public enum SubSystemType : ushort {
        IMAGE_SUBSYSTEM_UNKNOWN = 0,
        IMAGE_SUBSYSTEM_NATIVE = 1,
        IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
        IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
        IMAGE_SUBSYSTEM_POSIX_CUI = 7,
        IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,
        IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,
        IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
        IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,
        IMAGE_SUBSYSTEM_EFI_ROM = 13,
        IMAGE_SUBSYSTEM_XBOX = 14

    }

    public enum DllCharacteristicsType : ushort {
        RES_0 = 0x0001,
        RES_1 = 0x0002,
        RES_2 = 0x0004,
        RES_3 = 0x0008,
        IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE = 0x0040,
        IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY = 0x0080,
        IMAGE_DLL_CHARACTERISTICS_NX_COMPAT = 0x0100,
        IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,
        IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,
        IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,
        RES_4 = 0x1000,
        IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,
        IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
    }

    enum BasedRelocationType {
        IMAGE_REL_BASED_ABSOLUTE = 0,
        IMAGE_REL_BASED_HIGH = 1,
        IMAGE_REL_BASED_LOW = 2,
        IMAGE_REL_BASED_HIGHLOW = 3,
        IMAGE_REL_BASED_HIGHADJ = 4,
        IMAGE_REL_BASED_MIPS_JMPADDR = 5,
        IMAGE_REL_BASED_MIPS_JMPADDR16 = 9,
        IMAGE_REL_BASED_IA64_IMM64 = 9,
        IMAGE_REL_BASED_DIR64 = 10
    }

    [Flags()]
    public enum AllocationType : uint {
        COMMIT = 0x1000,
        RESERVE = 0x2000,
        RESET = 0x80000,
        LARGE_PAGES = 0x20000000,
        PHYSICAL = 0x400000,
        TOP_DOWN = 0x100000,
        WRITE_WATCH = 0x200000,
        DECOMMIT = 0x4000,
        RELEASE = 0x8000
    }

    [Flags()]
    public enum MemoryProtection : uint {
        EXECUTE = 0x10,
        EXECUTE_READ = 0x20,
        EXECUTE_READWRITE = 0x40,
        EXECUTE_WRITECOPY = 0x80,
        NOACCESS = 0x01,
        READONLY = 0x02,
        READWRITE = 0x04,
        WRITECOPY = 0x08,
        GUARD_Modifierflag = 0x100,
        NOCACHE_Modifierflag = 0x200,
        WRITECOMBINE_Modifierflag = 0x400
    }

    [Flags()]
    public enum PageProtection {
        NOACCESS = 0x01,
        READONLY = 0x02,
        READWRITE = 0x04,
        WRITECOPY = 0x08,
        EXECUTE = 0x10,
        EXECUTE_READ = 0x20,
        EXECUTE_READWRITE = 0x40,
        EXECUTE_WRITECOPY = 0x80,
        GUARD = 0x100,
        NOCACHE = 0x200,
        WRITECOMBINE = 0x400,
    }

    public enum ImageSectionFlags : uint {
        IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000,  // Section contains extended relocations.
        IMAGE_SCN_MEM_DISCARDABLE = 0x02000000,  // Section can be discarded.
        IMAGE_SCN_MEM_NOT_CACHED = 0x04000000,  // Section is not cachable.
        IMAGE_SCN_MEM_NOT_PAGED = 0x08000000, // Section is not pageable.
        IMAGE_SCN_MEM_SHARED = 0x10000000,  // Section is shareable.
        IMAGE_SCN_MEM_EXECUTE = 0x20000000, // Section is executable.
        IMAGE_SCN_MEM_READ = 0x40000000, // Section is readable.
        IMAGE_SCN_MEM_WRITE = 0x80000000  // Section is writeable.
    }

    public enum ImageSectionContains : uint {
        CODE = 0x00000020,  // Section contains code.
        INITIALIZED_DATA = 0x00000040,  // Section contains initialized data.
        UNINITIALIZED_DATA = 0x00000080  // Section contains uninitialized data.
    }

    public enum DllReason : uint {
        DLL_PROCESS_ATTACH = 1,
        DLL_THREAD_ATTACH = 2,
        DLL_THREAD_DETACH = 3,
        DLL_PROCESS_DETACH = 0

    }

    unsafe class NativeDeclarations {
        public const ushort IMAGE_DOS_SIGNATURE = 0x5A4D;
        public const uint IMAGE_NT_SIGNATURE = 0x00004550;
        public const uint PAGE_NOCACHE = 0x200;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemSet(IntPtr dest, int c, IntPtr count);

        [DllImport("kernel32.dll")]
        public static extern bool IsBadReadPtr(IntPtr lp, uint ucb);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, AllocationType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// Equivalent to the IMAGE_FIRST_SECTION macro
        /// </summary>
        /// <param name="ntheader">Pointer to to ntheader</param>
        /// <returns>Pointer to the first section</returns>
        public static IMAGE_SECTION_HEADER* IMAGE_FIRST_SECTION(IMAGE_NT_HEADERS32* ntheader) {
            return ((IMAGE_SECTION_HEADER*)((int)ntheader +
                 Marshal.OffsetOf(typeof(IMAGE_NT_HEADERS32), "OptionalHeader").ToInt32() +
                 ntheader->FileHeader.SizeOfOptionalHeader));
        }

        /// <summary>
        /// Equivalent to the IMAGE_ORDINAL32 macro
        /// </summary>
        public static uint IMAGE_ORDINAL32(uint ordinal) {
            return ordinal & 0xffff;
        }

        /// <summary>
        /// Equivalent to the IMAGE_SNAP_BY_ORDINAL32 macro
        /// </summary>
        public static bool IMAGE_SNAP_BY_ORDINAL32(uint ordinal) {
            return ((ordinal & 0x80000000) != 0);
        }
    }

    public class NativeDllLoadException : Exception {
        public NativeDllLoadException()
            : base() {
        }
        public NativeDllLoadException(string message)
            : base(message) {
        }

        public NativeDllLoadException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }

    unsafe class MemoryModule : IDisposable {
            public bool Disposed { get; private set; }
            private GCHandle dataHandle;
            private IMAGE_NT_HEADERS32* headers;
            private byte* codeBase;
            private List<IntPtr> modules;
            private bool initialized;

            [UnmanagedFunctionPointer(CallingConvention.Winapi)]
            private delegate bool DllEntryDelegate(IntPtr hinstDLL, DllReason fdwReason, IntPtr lpReserved);

            private DllEntryDelegate dllEntry;

            /// <summary>
            /// Loads a unmanged (native) DLL in the memory.
            /// </summary>
            /// <param name="data">Dll as a byte array</param>
            public MemoryModule(byte[] data) {
                if (data == null)
                    throw new ArgumentNullException("data");

                this.headers = null;
                this.codeBase = null;
                this.modules = new List<IntPtr>();
                this.initialized = false;
                this.Disposed = false;

                MemoryLoadLibrary(data);
            }

            ~MemoryModule() {
                this.Dispose();
            }

            /// <summary>
            /// Returns a delegate for a function inside the DLL.
            /// </summary>
            /// <param name="funcName">The Name of the function to be searched.</param>
            /// <param name="t">The type of the delegate to be returned.</param>
            /// <returns>A delegate instance that can be cast to the appropriate delegate type.</returns>
            public Delegate GetDelegateFromFuncName(int inputOrdinal, Type t) {
                if (t == null)
                    throw new ArgumentNullException("t");

                if (this.Disposed)
                    throw new ObjectDisposedException("MemoryModule");

                if (!this.initialized)
                    throw new InvalidOperationException("Dll is not initialized.");

                int idx = -1;
                uint* nameRef;
                ushort* ordinal;
                IMAGE_EXPORT_DIRECTORY* exports;
                IntPtr funcPtr;

                IMAGE_DATA_DIRECTORY* directory = &this.headers->OptionalHeader.ExportTable;
                if (directory->Size == 0)
                    throw new NativeDllLoadException("Dll has no export table.");

                exports = (IMAGE_EXPORT_DIRECTORY*)(this.codeBase + directory->VirtualAddress);
                if (exports->NumberOfFunctions == 0 || exports->NumberOfNames == 0)
                    throw new NativeDllLoadException("Dll exports no functions.");

                nameRef = (uint*)(this.codeBase + exports->AddressOfNames);
                ordinal = (ushort*)(this.codeBase + exports->AddressOfNameOrdinals);

                ordinal += inputOrdinal;
                idx = *ordinal;
                /*
                for (int i = 0; i < exports->NumberOfNames; i++, nameRef++, ordinal++) {
                    string curFuncName = Marshal.PtrToStringAnsi(new IntPtr(this.codeBase + *nameRef));
                    if (curFuncName == funcName) {
                        idx = *ordinal;
                        break;
                    }
                }
                */
                if (idx > exports->NumberOfFunctions)
                    throw new NativeDllLoadException("IDX don't match number of funtions.");

                funcPtr = new IntPtr(this.codeBase + (*(uint*)(this.codeBase + exports->AddressOfFunctions + (idx * 4))));
                return Marshal.GetDelegateForFunctionPointer(funcPtr, t);

            }

            private void MemoryLoadLibrary(byte[] data) {
                IMAGE_DOS_HEADER32* dosHeader;
                IMAGE_NT_HEADERS32* ntHeader;
                IntPtr dllEntryPtr;
                byte* code, headers, dataPtr;
                uint locationDelta;

                this.dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                dataPtr = (byte*)this.dataHandle.AddrOfPinnedObject().ToPointer();

                dosHeader = (IMAGE_DOS_HEADER32*)dataPtr;
                if (dosHeader->e_magic != NativeDeclarations.IMAGE_DOS_SIGNATURE)
                    throw new BadImageFormatException("Not a valid executable file.");

                ntHeader = (IMAGE_NT_HEADERS32*)(dataPtr + dosHeader->e_lfanew);
                if (ntHeader->Signature != NativeDeclarations.IMAGE_NT_SIGNATURE)
                    throw new BadImageFormatException("Not a valid PE file.");

                code = (byte*)NativeDeclarations.VirtualAlloc(
                    new IntPtr(ntHeader->OptionalHeader.ImageBase),
                    ntHeader->OptionalHeader.SizeOfImage,
                    AllocationType.RESERVE,
                    MemoryProtection.READWRITE).ToPointer();

                if (code == null) {
                    code = (byte*)NativeDeclarations.VirtualAlloc(
                    IntPtr.Zero,
                    ntHeader->OptionalHeader.SizeOfImage,
                    AllocationType.RESERVE,
                    MemoryProtection.READWRITE).ToPointer();
                }

                if (code == null)
                    throw new Win32Exception();

                NativeDeclarations.VirtualAlloc(
                    new IntPtr(code),
                    ntHeader->OptionalHeader.SizeOfImage,
                    AllocationType.COMMIT,
                    MemoryProtection.READWRITE);

                this.codeBase = code;

                headers = (byte*)NativeDeclarations.VirtualAlloc(
                    new IntPtr(code),
                    ntHeader->OptionalHeader.SizeOfHeaders,
                    AllocationType.COMMIT,
                    MemoryProtection.READWRITE).ToPointer();

                if (headers == null)
                    throw new Win32Exception();

                Marshal.Copy(data, 0, new IntPtr(headers), (int)(dosHeader->e_lfanew + ntHeader->OptionalHeader.SizeOfHeaders));
                this.headers = (IMAGE_NT_HEADERS32*)&((byte*)(headers))[dosHeader->e_lfanew];

                this.headers->OptionalHeader.ImageBase = (uint)code;

                this.CopySections(data, ntHeader);

                locationDelta = (uint)(code - ntHeader->OptionalHeader.ImageBase);
                if (locationDelta != 0)
                    PerformBaseRelocation(locationDelta);

                this.BuildImportTable();
                this.FinalizeSections();

                if (this.headers->OptionalHeader.AddressOfEntryPoint == 0)
                    throw new NativeDllLoadException("DLL has no entry point");

                dllEntryPtr = new IntPtr(code + this.headers->OptionalHeader.AddressOfEntryPoint);

                this.dllEntry = (DllEntryDelegate)Marshal.GetDelegateForFunctionPointer(dllEntryPtr, typeof(DllEntryDelegate));

                if (dllEntry(new IntPtr(code), DllReason.DLL_PROCESS_ATTACH, IntPtr.Zero))
                    this.initialized = true;
                else {
                    this.initialized = false;
                    throw new NativeDllLoadException("Can't attach DLL to process.");
                }


            }

            private readonly PageProtection[,,] ProtectionFlags = new PageProtection[,,]
            {
            {
                { PageProtection.NOACCESS, PageProtection.WRITECOPY },
                { PageProtection.READONLY, PageProtection.READWRITE }
            },
            {
                { PageProtection.EXECUTE, PageProtection.WRITECOPY },
                { PageProtection.EXECUTE_READ, PageProtection.EXECUTE_READWRITE }
            }

            };

            private void FinalizeSections() {
                int imageOffset = 0;

                IMAGE_SECTION_HEADER* section = (IMAGE_SECTION_HEADER*)NativeDeclarations.IMAGE_FIRST_SECTION(this.headers);

                for (int i = 0; i < this.headers->FileHeader.NumberOfSections; i++, section++) {
                    uint protect, oldProtect, size;

                    int executable = (section->Characteristics & (uint)ImageSectionFlags.IMAGE_SCN_MEM_EXECUTE) != 0 ? 1 : 0;
                    int readable = (section->Characteristics & (uint)ImageSectionFlags.IMAGE_SCN_MEM_READ) != 0 ? 1 : 0;
                    int writeable = (section->Characteristics & (uint)ImageSectionFlags.IMAGE_SCN_MEM_WRITE) != 0 ? 1 : 0;

                    if ((section->Characteristics & (int)ImageSectionFlags.IMAGE_SCN_MEM_DISCARDABLE) > 0) {
                        NativeDeclarations.VirtualFree(new IntPtr(section->PhysicalAddress | (uint)imageOffset), section->SizeOfRawData, AllocationType.DECOMMIT);
                        continue;
                    }
                    protect = (uint)ProtectionFlags[executable, readable, writeable];
                    if ((section->Characteristics & (uint)ImageSectionFlags.IMAGE_SCN_MEM_NOT_CACHED) > 0)
                        protect |= NativeDeclarations.PAGE_NOCACHE;

                    size = section->SizeOfRawData;
                    if (size == 0) {
                        if ((section->Characteristics & (uint)ImageSectionContains.INITIALIZED_DATA) > 0)
                            size = this.headers->OptionalHeader.SizeOfInitializedData;
                        else if ((section->Characteristics & (uint)ImageSectionContains.UNINITIALIZED_DATA) > 0)
                            size = this.headers->OptionalHeader.SizeOfUninitializedData;
                    }

                    if (size > 0) {
                        if (!NativeDeclarations.VirtualProtect(new IntPtr(section->PhysicalAddress | (uint)imageOffset), size, protect, out oldProtect))
                            throw new Win32Exception("Can't change section access rights");
                    }
                }
            }

            private void BuildImportTable() {
                IMAGE_DATA_DIRECTORY* directory = &this.headers->OptionalHeader.ImportTable;
                if (directory->Size > 0) {
                    IMAGE_IMPORT_DESCRIPTOR* importDesc = (IMAGE_IMPORT_DESCRIPTOR*)(this.codeBase + directory->VirtualAddress);
                    for (; !NativeDeclarations.IsBadReadPtr(new IntPtr(importDesc), (uint)Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR))) && importDesc->Name > 0; importDesc++) {
                        uint* thunkRef;
                        int* funcRef;

                        string funcName = Marshal.PtrToStringAnsi(new IntPtr(this.codeBase + importDesc->Name));
                        IntPtr handle = NativeDeclarations.LoadLibrary(funcName);

                        if (handle == IntPtr.Zero)
                            throw new NativeDllLoadException("Can't load libary " + funcName);

                        this.modules.Add(handle);
                        if (importDesc->OriginalFirstThunk > 0) {
                            thunkRef = (uint*)(codeBase + importDesc->OriginalFirstThunk);
                            funcRef = (int*)(codeBase + importDesc->FirstThunk);
                        } else {
                            // no hint table
                            thunkRef = (uint*)(codeBase + importDesc->FirstThunk);
                            funcRef = (int*)(codeBase + importDesc->FirstThunk);
                        }
                        for (; *thunkRef > 0; thunkRef++, funcRef++) {
                            string procName;
                            if (NativeDeclarations.IMAGE_SNAP_BY_ORDINAL32(*thunkRef)) {
                                procName = Marshal.PtrToStringAnsi(new IntPtr(NativeDeclarations.IMAGE_ORDINAL32(*thunkRef)));
                                *funcRef = (int)NativeDeclarations.GetProcAddress(handle, procName);
                            } else {
                                IMAGE_IMPORT_BY_NAME* thunkData = (IMAGE_IMPORT_BY_NAME*)(codeBase + (*thunkRef));
                                procName = Marshal.PtrToStringAnsi(new IntPtr(thunkData->Name));
                                *funcRef = (int)NativeDeclarations.GetProcAddress(handle, procName);
                            }
                            // if (*funcRef == 0)
                            //   throw new NativeDllLoadException("Can't get adress for " + procName);

                        }
                    }
                }
            }

            private void PerformBaseRelocation(uint delta) {
                if (delta == 0)
                    return;

                int imageSizeOfBaseRelocation = Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));

                IMAGE_DATA_DIRECTORY* directory = &this.headers->OptionalHeader.BaseRelocationTable;
                if (directory->Size > 0) {
                    IMAGE_BASE_RELOCATION* relocation = (IMAGE_BASE_RELOCATION*)(this.codeBase + directory->VirtualAddress);
                    while (relocation->VirtualAdress > 0) {
                        byte* dest = this.codeBase + relocation->VirtualAdress;
                        ushort* relInfo = (ushort*)((byte*)relocation + imageSizeOfBaseRelocation);

                        for (int i = 0; i < ((relocation->SizeOfBlock - imageSizeOfBaseRelocation) / 2); i++, relInfo++) {
                            uint* patchAddrHL;
                            BasedRelocationType type;
                            int offset;

                            // the upper 4 bits define the type of relocation
                            type = (BasedRelocationType)(*relInfo >> 12);
                            // the lower 12 bits define the offset
                            offset = *relInfo & 0xfff;

                            switch (type) {
                                case BasedRelocationType.IMAGE_REL_BASED_ABSOLUTE:
                                    break;
                                case BasedRelocationType.IMAGE_REL_BASED_HIGHLOW:
                                    patchAddrHL = (uint*)(dest + offset);
                                    *patchAddrHL += delta;
                                    break;
                                default:
                                    break;
                            }
                        }

                        // advance to next relocation block
                        relocation = (IMAGE_BASE_RELOCATION*)(((byte*)relocation) + relocation->SizeOfBlock);
                    }
                }

            }

            void CopySections(byte[] data, IMAGE_NT_HEADERS32* ntHeader) {
                if (data == null)
                    throw new ArgumentNullException("data");

                if (ntHeader->Signature != NativeDeclarations.IMAGE_NT_SIGNATURE)
                    throw new BadImageFormatException("Inavlid PE-Header");

                uint size;
                int* dest;

                IMAGE_SECTION_HEADER* section = NativeDeclarations.IMAGE_FIRST_SECTION(this.headers);

                for (int i = 0; i < this.headers->FileHeader.NumberOfSections; i++, section++) {
                    if (section->SizeOfRawData == 0) {
                        // section doesn't contain data in the dll itself, but may define
                        // uninitialized data
                        size = ntHeader->OptionalHeader.SectionAlignment;
                        if (size > 0) {
                            dest = (int*)NativeDeclarations.VirtualAlloc(
                                new IntPtr(this.codeBase + section->VirtualAddress),
                                size,
                                AllocationType.COMMIT,
                                MemoryProtection.READWRITE).ToPointer();

                            section->PhysicalAddress = (uint)dest;
                            NativeDeclarations.MemSet(new IntPtr(dest), 0, new IntPtr(size));
                        }
                        continue;
                    }

                    dest = (int*)NativeDeclarations.VirtualAlloc(
                                new IntPtr((int)this.codeBase + section->VirtualAddress),
                                section->SizeOfRawData,
                                AllocationType.COMMIT,
                                MemoryProtection.READWRITE).ToPointer();

                    Marshal.Copy(data, (int)section->PointerToRawData, new IntPtr(dest), (int)section->SizeOfRawData);
                    section->PhysicalAddress = (uint)dest;
                }
            }

            public void Close() {
                ((IDisposable)this).Dispose();
            }

            void IDisposable.Dispose() {
                this.Dispose();
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose() {
                if (dataHandle.IsAllocated)
                    dataHandle.Free();

                if (this.initialized) {
                    this.dllEntry(new IntPtr(this.codeBase), DllReason.DLL_PROCESS_DETACH, IntPtr.Zero);
                    this.initialized = false;
                }

                if (this.modules.Count > 0) {
                    foreach (IntPtr module in this.modules) {
                        if (module != new IntPtr(-1) || module != IntPtr.Zero) // INVALID_HANDLE
                            NativeDeclarations.FreeLibrary(module);
                    }
                }

                if (this.codeBase != null)
                    NativeDeclarations.VirtualFree(new IntPtr(this.codeBase), 0, AllocationType.RELEASE);

                this.Disposed = true;

            }

        }
    
}
