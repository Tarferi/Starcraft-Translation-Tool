using System;
using System.IO;
using System.Windows.Controls;

namespace TranslatorData {

    class PersistantStorage {

        public bool autoUpdate = true;
        public long lastCheckForUpdate = 0;
        public String secretKey = "";
        public bool exportColorCodes = true;
        public bool exportEscapedLineBreaks = false;
        public String language = "";

        public void read(ReadBuffer rb) {
            autoUpdate = rb.readBool();
            lastCheckForUpdate = rb.readLong();
            secretKey = rb.readString(Settings.defaultEncoding);
            exportColorCodes = rb.readBool();
            exportEscapedLineBreaks = rb.readBool();
            language = rb.readString(Settings.defaultEncoding);
        }

        public void write(WriteBuffer wb) {
            wb.writeBool(autoUpdate);
            wb.writeLong(lastCheckForUpdate);
            wb.writeString(secretKey, Settings.defaultEncoding);
            wb.writeBool(exportColorCodes);
            wb.writeBool(exportEscapedLineBreaks);
            wb.writeString(language, Settings.defaultEncoding);
        }

        public void push() {
            History.save();
        }
    }

    class History {

        private static int historySize = 30; // 30 items per channel
        private static String historyFile = "hist.smthistory";

        private static PersistantStorage _storage = null;

        private static PersistantStorage _storageNoLoad { get { if (_storage == null) { _storage = new PersistantStorage(); } return _storage; } }

        public static PersistantStorage storage { get { if (_storage == null) { History.open(null, null); } return _storageNoLoad; } }

        public static void save() {
            save(null, null);
        }

        public static void save(String name, ComboBox history) {
            try {
                if (!File.Exists(historyFile)) {
                    File.Create(historyFile).Close();
                }
                byte[] bytes = File.ReadAllBytes(historyFile);
                ReadBuffer rb = new ReadBuffer(bytes);
                int sections = 0;
                try {
                    sections = rb.readByte();
                } catch (OutOfBoundsReadException) { // No sections

                }
                bool foundSector = false;
                WriteBuffer wb = new WriteBuffer();
                wb.writeByte(sections);
                if (sections >= 0) {
                    for (int i = 0; i < sections; i++) {
                        String sectorName = rb.readString(Settings.defaultEncoding);
                        wb.writeString(sectorName, Settings.defaultEncoding);
                        int sectorSize = rb.readInt();
                        if (sectorName != name || history == null) { // Not our sector, copy (or not saving a sector)
                            wb.writeInt(sectorSize);
                            for (int o = 0; o < sectorSize; o++) {
                                wb.writeByteArray(rb.readByteArray());
                            }
                        } else { // Our sector, insert our data
                                 // Empty previous data
                            for (int o = 0; o < sectorSize; o++) {
                                rb.readByteArray();
                            }
                            // Insert new data
                            int dataSize = history.Items.Count > historySize ? historySize : history.Items.Count;
                            wb.writeInt(dataSize);
                            for (int o = 0; o < dataSize; o++) {
                                wb.writeString(history.Items[o].ToString(), Settings.defaultEncoding);
                            }
                            foundSector = true;
                        }
                    }
                } else {
                    sections = 0;
                }
                if (!foundSector && history != null) {
                    wb.writeString(name, Settings.defaultEncoding);
                    int dataSize = history.Items.Count > historySize ? historySize : history.Items.Count;
                    wb.writeInt(dataSize);
                    for (int o = 0; o < dataSize; o++) {
                        wb.writeString(history.Items[o].ToString(), Settings.defaultEncoding);
                    }
                    sections++;
                }
                _storageNoLoad.write(wb);
                byte[] data = wb.ToArray();
                data[0] = (byte) sections;
                File.WriteAllBytes(historyFile, data);
            } catch (OutOfBoundsReadException) { // Pass read beyond
                return;
            }
        }

        public static void open(String name, ComboBox history) {
            try {
                if (File.Exists(historyFile)) {
                    byte[] bytes = File.ReadAllBytes(historyFile);
                    ReadBuffer rb = new ReadBuffer(bytes);
                    byte sections = (byte) rb.readByte();
                    if (history != null) {
                        history.Items.Clear();
                    }
                    for (int i = 0; i < sections; i++) {
                        String sectorName = rb.readString(Settings.defaultEncoding);
                        int sectorSize = rb.readInt();
                        if (sectorName != name) { // Not our sector, pass
                            for (int o = 0; o < sectorSize; o++) {
                                rb.readByteArray();
                            }
                        } else { // Our sector, get our data
                            for (int o = 0; o < sectorSize; o++) {
                                String str = rb.readString(Settings.defaultEncoding);
                                if (str != "") {
                                    if (history != null) {
                                        history.Items.Add(str);
                                    }
                                }
                            }
                            break;
                        }
                    }
                    _storageNoLoad.read(rb);
                }
            } catch (OutOfBoundsReadException) { // Pass read beyond
                return;
            }
        }
    }
}
