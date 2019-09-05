using System;
using System.IO;

namespace WpfApplication1 {

    public class Settings {

        public String settingsPath;
        public String inpuPath;
        public String outputPath;

        public String[] langauges;
        public String[][] strings;

        public int[] lastKnownMapping;
        public String[] originalStrings;

        public int useCondition;

        public byte result;

        internal TranslateString[] ts = new TranslateString[0]; // Non saved data

        private Settings() {

        }
   
        public static Settings loadFromFile(String fileName) {
            Settings s = new Settings();
            try {
                byte[] bytes = File.ReadAllBytes(fileName);
                ReadBuffer rb = new ReadBuffer(bytes);
                s.settingsPath = fileName;
                s.inpuPath = rb.readString();
                s.outputPath = rb.readString();

                s.langauges = rb.readStringArray();
                s.strings = rb.readStringArrayArray();

                s.useCondition = rb.readInt();
                s.lastKnownMapping = rb.readIntArray(rb.readInt());
                s.originalStrings = rb.readStringArray();
                
                
            } catch (Exception) {
                return null;
            }
            return s;
        }

        public bool saveToFile(String file) {
            try {
                WriteBuffer wb = new WriteBuffer();
                wb.writeString(this.inpuPath);
                wb.writeString(this.outputPath);

                wb.writeArray(this.langauges);
                wb.writeArrayArray(this.strings);

                wb.writeInt(this.useCondition);

                wb.writeInt(this.lastKnownMapping.Length);
                wb.writeArray(this.lastKnownMapping);
                wb.writeArray(this.originalStrings);

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
