using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TranslatorUI;
using static TranslatorUI.MainWindow;

namespace TranslatorData {

    public class Settings {

        public static Encoding defaultEncoding = Encoding.UTF8;

        public String settingsPath;
        public String inpuPath;
        public String outputPath;

        public String[] langauges;
        public Encoding[] encodings;
        public String[][] strings;

        public int[] lastKnownMapping;

        public Encoding originalEncoding;

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
            ReadBuffer rb = new ReadBuffer(bytes);

            String ver = rb.readString(defaultEncoding);
            if (ver == "version") {
                int verNumber = rb.readInt();
                if (verNumber == 1) {
                    s.inpuPath = rb.readString(defaultEncoding);
                    s.outputPath = rb.readString(defaultEncoding);

                    s.langauges = rb.readStringArray(defaultEncoding);

                    string[] encodingNames = rb.readStringArray(defaultEncoding);
                    s.encodings = new Encoding[encodingNames.Length];

                    for (int i = 0; i < encodingNames.Length; i++) {
                        s.encodings[i] = NamedEncoding.GetForName(encodingNames[i]).Encoding;
                    }

                    s.strings = rb.readStringArrayArray(s.encodings);

                    s.useCondition = rb.readInt();
                    s.lastKnownMapping = rb.readIntArray(rb.readInt());

                    s.originalEncoding = NamedEncoding.GetForName(rb.readString(defaultEncoding)).Encoding;
                    s.originalStrings = rb.readStringArray(s.originalEncoding);

                    s.repack = rb.readBool();
                }

            } else { // Load vanilla settings and generate rest
                rb = new ReadBuffer(bytes);
                s.inpuPath = rb.readString(Settings.defaultEncoding);
                s.outputPath = rb.readString(Settings.defaultEncoding);

                s.langauges = rb.readStringArray(Settings.defaultEncoding);
                s.encodings = new Encoding[s.langauges.Length];
                for (int i = 0; i < s.encodings.Length; i++) {
                    s.encodings[i] = Settings.defaultEncoding;
                }

                s.strings = rb.readStringArrayArray(s.encodings);

                s.useCondition = rb.readInt();
                s.lastKnownMapping = rb.readIntArray(rb.readInt());

                s.originalEncoding = Settings.defaultEncoding;

                s.originalStrings = rb.readStringArray(s.originalEncoding);
            }

                

            // New ver
            try {
                s.repack = rb.readBool();
            } catch (Exception) {
            }
            return s;
        }

        public static Settings loadFromFile(String fileName) {
            try {
                if (File.Exists(fileName)) {
                    byte[] bytes = File.ReadAllBytes(fileName);
                    Settings s = loadVersioned(bytes, fileName);
                    return s;
                }
                return null;
            } catch (Exception) {
                throw;
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

                string[] encodingNames = new string[this.encodings.Length];
                for(int i =0; i < this.encodings.Length; i++) {
                    encodingNames[i] = this.encodings[i].EncodingName;
                }

                wb.writeArray(encodingNames, defaultEncoding);

                wb.writeArrayArray(this.strings, this.encodings);

                wb.writeInt(this.useCondition);

                wb.writeInt(this.lastKnownMapping.Length);
                wb.writeArray(this.lastKnownMapping);

                wb.writeString(this.originalEncoding.EncodingName, defaultEncoding);
                wb.writeArray(this.originalStrings, this.originalEncoding);

                wb.writeBool(this.repack);

                File.WriteAllBytes(file, wb.ToArray());
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public static Settings getBlank(String file, Encoding defaultEncoding) {
            Settings s = new Settings();
            s.settingsPath = file;

            s.inpuPath = "";
            s.outputPath = "";
            s.langauges = new String[0];
            s.strings = new String[0][];
            s.originalEncoding = defaultEncoding;
            s.useCondition = 0;
            s.lastKnownMapping = new int[0];
            s.originalStrings = new String[0];
            s.encodings = new Encoding[0];
            return s;
        }
    }
}
