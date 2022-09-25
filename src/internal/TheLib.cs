#if !DEBUG
#endif
#define USE_MEMORY_MODULE

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TranslatorData {
    public enum SupportType {
        NonEUD = 0,
        EUDEditor = 1,
        NonCPEUD = 2,
        Unsupported = 3
    }

    public class MapString {

        public int[] unitIndexs;
        public int[] switchIndexes;
        public int[] locationIndexes;
        public int[] triggerActionIndexes;
        public int[] triggerCommentIndexes;
        public int[] briefingActionIndexes;
        public int[] briefingCommentsIndexes;
        public int[] forceNamesIndexes;

        public bool isMapName;
        public bool isMapDescription;

        public String str;
        public int mapIndex;
    }

    public unsafe struct TranslationStructure {
        public unsafe byte action;

        public unsafe byte* inputFilePath;
        public unsafe byte* outputFilePath;

        public unsafe byte* stringData;
        public unsafe int stringDataLength;

        public unsafe int useCondition;
        public unsafe byte repack;

        public unsafe byte result;
    }

    class TheLib {

        private static unsafe void run(TranslationStructure* settings) {

            unsafe {
                // Move to unmanaged memory

                //int dataSize = Marshal.SizeOf<EUDSettings>(settings);

                int dataSize = 1024; // Stupid but works
                IntPtr ptr = Marshal.AllocHGlobal(dataSize);
                byte* data = (byte*) ptr;
                UnsafeWriteBuffer wb = new UnsafeWriteBuffer(data);
                wb.writeByte(settings->action);
                wb.writeBytePtr(settings->inputFilePath);
                wb.writeBytePtr(settings->outputFilePath);

                wb.writeBytePtr(settings->stringData);
                wb.writeInt(settings->stringDataLength);

                wb.writeInt(settings->useCondition);
                wb.writeByte(settings->repack);

                wb.writeByte(settings->result);

                // Process
                IntPtr ms = (IntPtr) data;
                Process(ms);

                UnsafeReadBuffer rb = new UnsafeReadBuffer(data);
                settings->action = (byte) rb.readByte();

                settings->inputFilePath = rb.readBytePtr();
                settings->outputFilePath = rb.readBytePtr();

                settings->stringData = rb.readBytePtr();
                settings->stringDataLength = rb.readInt();

                settings->useCondition = rb.readInt();
                settings->repack = (byte) rb.readByte();

                settings->result = (byte) rb.readByte();

                Marshal.FreeHGlobal(ptr);
            }
        }

#if !USE_MEMORY_MODULE
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, IntPtr procOrdinal);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
#endif

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ProcessFunc(IntPtr settings);

        protected static unsafe void Process(IntPtr settings) {
            if (_Process == null) {
#if USE_MEMORY_MODULE
                byte[] data = TranslatorUI.Properties.Resources.TranslateLib;
                mem = new MemoryModule(data);
                _Process = (ProcessFunc) mem.GetDelegateFromFuncName(0, typeof(ProcessFunc));
#else
                IntPtr pDll = LoadLibrary("C:\\Users\\Tom\\Desktop\\Documents\\Visual Studio 2015\\Projects\\TranslateLib\\Debug\\TranslateLib.dll");
                IntPtr pAddressOfFunctionToCall0 = GetProcAddress(pDll, (IntPtr) 1);
                _Process = (ProcessFunc) Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall0, typeof(ProcessFunc));
#endif
            }
            _Process(settings);
        }

#if USE_MEMORY_MODULE
        private static MemoryModule mem = null; // Memory module (if used)
#endif
        private static ProcessFunc _Process = null; // DLL Method

        private static unsafe byte* toByteArray(String strStr, String encoding) {
            return toByteArray(strStr, null, encoding);
        }

        private static unsafe byte* toByteArray(String strStr, int* outLength, String encoding) {
            byte[] str = Encoding.GetEncoding(encoding).GetBytes(strStr);
            if (str == null) {
                return (byte*) 0;
            }
            if (str.Length == 0) {
                return (byte*) 0;
            }
            IntPtr ptr = Marshal.AllocHGlobal(str.Length + 1);
            byte* bytes = (byte*) ptr;
            for (int i = 0; i < str.Length; i++) {
                bytes[i] = (byte) str[i];
            }
            bytes[str.Length] = (byte) 0;
            if (outLength != null) {
                *outLength = str.Length + 1;
            }
            return bytes;
        }

        private static unsafe byte* toByteArray(byte[] data) {
            if (data == null) {
                return (byte*) 0;
            }
            IntPtr ptr = Marshal.AllocHGlobal(data.Length);
            byte* bytes = (byte*) ptr;
            for (int i = 0; i < data.Length; i++) {
                bytes[i] = (byte) data[i];
            }
            return bytes;
        }

        private static unsafe byte* toByteArray(int[] data, String[] strArr, int* outLength, String encoding) {
            int dataSize = 4; // Size of array
            byte*[] strPtrs = new byte*[strArr.Length];
            int[] strPtrsLengths = new int[strArr.Length];

            for (int i = 0; i < strArr.Length; i++) {
                int len = 0; // Total length of encoded data, without null delimiter
                byte* strR = toByteArray(strArr[i], &len, encoding);
                strPtrs[i] = strR;
                strPtrsLengths[i] = len;
                dataSize += len + 4 + 4; // Size of string, ID of string and length of length
            }

            IntPtr ptr = Marshal.AllocHGlobal(dataSize);
            if (outLength != null) {
                *outLength = dataSize;
            }

            byte* bytes = (byte*) ptr;
            UnsafeWriteBuffer wb = new UnsafeWriteBuffer(bytes);

            wb.writeInt(strArr.Length);
            for (int i = 0; i < strArr.Length; i++) {
                byte* str = strPtrs[i];
                int len = strPtrsLengths[i];
                wb.writeInt(data[i]);
                wb.writeInt(len);
                for (int o = 0; o < len; o++) {
                    wb.writeByte(str[o]);
                }
                killByteArray(str);
            }

            return bytes;
        }

        private static unsafe void killByteArray(byte* str) {
            if (str == (byte*) 0) {
                return;
            }
            IntPtr ptr = (IntPtr) str;
            Marshal.FreeHGlobal(ptr);
        }

        public unsafe static SupportType getSupportType(Settings settings) {
            TranslationStructure es = new TranslationStructure();
            es.action = (byte) 4;

            es.inputFilePath = toByteArray(settings.inpuPath, Settings.defaultEncoding);

            es.useCondition = settings.useCondition;

            run(&es);

            killByteArray(es.inputFilePath);

            settings.result = es.result;
            if (es.result != 0) { // Error
                return SupportType.Unsupported;
            }
            return (SupportType) es.useCondition;
        }

        public unsafe static bool checkUsedCondition(Settings settings) {
            TranslationStructure es = new TranslationStructure();
            es.action = (byte) 5;

            es.inputFilePath = toByteArray(settings.inpuPath, Settings.defaultEncoding);

            es.useCondition = settings.useCondition;

            run(&es);

            killByteArray(es.inputFilePath);

            settings.result = es.result;
            if (es.result != 0) { // Error
                return false;
            }
            return es.useCondition == 0;
        }

        public unsafe static bool process(Settings settings, int languageIndex) {
            TranslationStructure es = new TranslationStructure();
            es.action = (byte) 1;

            es.inputFilePath = toByteArray(settings.inpuPath, Settings.defaultEncoding);
            es.outputFilePath = toByteArray(settings.outputPath, Settings.defaultEncoding);

            es.useCondition = settings.useCondition;
            es.repack = (byte) (settings.repack ? 1 : 0);

            int len = 0;
            es.stringData = toByteArray(settings.lastKnownMapping, settings.strings[languageIndex], &len, settings.encodings[languageIndex]);
            es.stringDataLength = len;

            run(&es);

            killByteArray(es.inputFilePath);
            killByteArray(es.outputFilePath);
            killByteArray(es.stringData);

            settings.result = es.result;
            settings.repack = es.repack == 1;

            return es.result == 0;
        }

        private static MapString readMapString(UnsafeReadBuffer rb, String encoding) {
            MapString ms = new MapString();

            ms.mapIndex = rb.readInt();

            ms.str = rb.readString(encoding);
            ms.isMapDescription = rb.readByte() == 1 ? true : false;
            ms.isMapName = rb.readByte() == 1 ? true : false;

            ms.unitIndexs = rb.readIntArray(rb.readInt());
            ms.switchIndexes = rb.readIntArray(rb.readInt());
            ms.locationIndexes = rb.readIntArray(rb.readInt());
            ms.triggerActionIndexes = rb.readIntArray(rb.readInt());
            ms.triggerCommentIndexes = rb.readIntArray(rb.readInt());
            ms.briefingActionIndexes = rb.readIntArray(rb.readInt());
            ms.briefingCommentsIndexes = rb.readIntArray(rb.readInt());
            ms.forceNamesIndexes = rb.readIntArray(rb.readInt());

            return ms;
        }

        public static unsafe MapString[] getStrings(String filePath, String encoding) {
            TranslationStructure es = new TranslationStructure();
            es.action = (byte) 2;
            es.outputFilePath = (byte*) 0;
            es.inputFilePath = toByteArray(filePath, Settings.defaultEncoding);
            try {
                run(&es); // Run extraction

                killByteArray(es.inputFilePath);
                UnsafeReadBuffer rb = new UnsafeReadBuffer(es.stringData);

                int totalStrings = rb.readInt();
                MapString[] result = new MapString[totalStrings];
                for (int i = 0; i < totalStrings; i++) {
                    result[i] = readMapString(rb, encoding);
                }

                // Sort by index
                bool changed = true;
                while (changed) {
                    changed = false;
                    for (int i = 1; i < result.Length; i++) {
                        MapString p0 = result[i - 1];
                        MapString p1 = result[i];
                        if (p0.mapIndex > p1.mapIndex) {
                            changed = true;
                            result[i - 1] = p1;
                            result[i] = p0;
                        }
                    }
                }

                for (int i = 0; i < result.Length; i++) {
                    //result[i].storageIndex = i;
                }
                es.action = (byte) 3;
                run(&es); // Free the array data string

                return result;
            } catch (Exception) {

            }
            return null;
        }
    }
}
