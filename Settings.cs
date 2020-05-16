using System;
using System.IO;

namespace WpfApplication1 {

    public class Settings {

        public static readonly String defaultEncoding = "UTF-8";

        public String settingsPath;
        public String inpuPath;
        public String outputPath;

        public String[] langauges;
        public String[] encodings;
        public String[][] strings;

        public int[] lastKnownMapping;

        public String originalEncoding;

        public String[] originalStrings;

        public int useCondition;
        public bool repack = true;

        public byte result;

        internal TranslateString[] ts = new TranslateString[0]; // Non saved data

        private Settings() {

        }

        private static Settings loadVersioned(byte[] bytes, String fileName) {
            Settings s = new Settings();
            s.settingsPath = fileName;
            try {
                ReadBuffer rb = new ReadBuffer(bytes);

                String ver = rb.readString(defaultEncoding);
                if (ver == "version") {
                    int verNumber = rb.readInt();
                    if (verNumber == 1) {
                        s.inpuPath = rb.readString(defaultEncoding);
                        s.outputPath = rb.readString(defaultEncoding);

                        s.langauges = rb.readStringArray(defaultEncoding);
                        s.encodings = rb.readStringArray(defaultEncoding);

                        s.strings = rb.readStringArrayArray(s.encodings);

                        s.useCondition = rb.readInt();
                        s.lastKnownMapping = rb.readIntArray(rb.readInt());

                        s.originalEncoding = rb.readString(defaultEncoding);
                        s.originalStrings = rb.readStringArray(s.originalEncoding);

                        s.repack = rb.readBool();
                    }

                } else { // Load vanilla settings and generate rest
                    rb = new ReadBuffer(bytes);
                    String encoding = "EUC-KR";

                    s.inpuPath = rb.readString(encoding);
                    s.outputPath = rb.readString(encoding);

                    s.langauges = rb.readStringArray(encoding);
                    s.encodings = new string[s.langauges.Length];
                    for (int i = 0; i < s.encodings.Length; i++) {
                        s.encodings[i] = "EUC-KR";
                    }

                    s.strings = rb.readStringArrayArray(s.encodings);

                    s.useCondition = rb.readInt();
                    s.lastKnownMapping = rb.readIntArray(rb.readInt());

                    s.originalEncoding = "EUC-KR";

                    s.originalStrings = rb.readStringArray(s.originalEncoding);
                }



                // New ver
                try {
                    s.repack = rb.readBool();
                } catch (Exception) {
                }
                return s;
            } catch (Exception) {
                return null;
            }
        }

        public static Settings loadFromFile(String fileName) {
            try {
                byte[] bytes = File.ReadAllBytes(fileName);
                Settings s = loadVersioned(bytes, fileName);
                for (int i = 0; i < s.encodings.Length; i++) {
                    s.encodings[i] = Settings.defaultEncoding;
                }
                s.originalEncoding = Settings.defaultEncoding;
                return s;
            } catch (Exception) {
                return null;
            }
        }

        public bool saveToFile(String file) {
            try {
                WriteBuffer wb = new WriteBuffer();
                wb.writeString("version", defaultEncoding);
                wb.writeInt(1);

                wb.writeString(this.inpuPath, defaultEncoding);
                wb.writeString(this.outputPath, defaultEncoding);

                wb.writeArray(this.langauges, defaultEncoding);
                wb.writeArray(this.encodings, defaultEncoding);

                wb.writeArrayArray(this.strings, this.encodings);

                wb.writeInt(this.useCondition);

                wb.writeInt(this.lastKnownMapping.Length);
                wb.writeArray(this.lastKnownMapping);

                wb.writeString(this.originalEncoding, defaultEncoding);
                wb.writeArray(this.originalStrings, this.originalEncoding);

                wb.writeBool(this.repack);

                File.WriteAllBytes(file, wb.ToArray());
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public static Settings getBlank(String file) {
            Settings s = new Settings();
            s.settingsPath = file;

            s.inpuPath = "";
            s.outputPath = "";
            s.langauges = new String[0];
            s.strings = new String[0][];
            s.useCondition = 0;
            s.lastKnownMapping = new int[0];
            s.originalStrings = new String[0];
            return s;
        }
    }
}
