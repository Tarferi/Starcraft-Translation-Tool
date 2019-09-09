using System;
using System.IO;
using System.Windows.Controls;
using WpfApplication1;

namespace QChkUI {

    class PersistantStorage {

        public bool autoUpdate = true;
        public long lastCheckForUpdate = 0;
        public String secretKey = "";
        public bool exportColorCodes = true;
        public bool exportEscapedLineBreaks = false;
        
        public void read(ReadBuffer rb) {
            autoUpdate = rb.readBool();
            lastCheckForUpdate = rb.readLong();
            secretKey = rb.readString();
            exportColorCodes = rb.readBool();
            exportEscapedLineBreaks = rb.readBool();
        }

        public void write(WriteBuffer wb) {
            wb.writeBool(autoUpdate);
            wb.writeLong(lastCheckForUpdate);
            wb.writeString(secretKey);
            wb.writeBool(exportColorCodes);
            wb.writeBool(exportEscapedLineBreaks);
        }

        public void push() {
            History.save();
        }
    }

    class History {

        private static int historySize = 30; // 30 items per channel
        private static String historyFile = "hist.smthistory";

        public static PersistantStorage storage = new PersistantStorage();

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
                int sections = rb.readByte();
                bool foundSector = false;
                WriteBuffer wb = new WriteBuffer();
                wb.writeByte(sections);
                if (sections >= 0) {
                    for (int i = 0; i < sections; i++) {
                        String sectorName = rb.readString();
                        wb.writeString(sectorName);
                        int sectorSize = rb.readInt();
                        if (sectorName != name || history == null) { // Not our sector, copy (or not saving a sector)
                            wb.writeInt(sectorSize);
                            for (int o = 0; o < sectorSize; o++) {
                                wb.writeString(rb.readString());
                            }
                        } else { // Our sector, insert our data
                                 // Empty previous data
                            for (int o = 0; o < sectorSize; o++) {
                                rb.readString();
                            }
                            // Insert new data
                            int dataSize = history.Items.Count > historySize ? historySize : history.Items.Count;
                            wb.writeInt(dataSize);
                            for (int o = 0; o < dataSize; o++) {
                                wb.writeString(history.Items[o].ToString());
                            }
                            foundSector = true;
                        }
                    }
                } else {
                    sections = 0;
                }
                if (!foundSector && history != null) {
                    wb.writeString(name);
                    int dataSize = history.Items.Count > historySize ? historySize : history.Items.Count;
                    wb.writeInt(dataSize);
                    for (int o = 0; o < dataSize; o++) {
                        wb.writeString(history.Items[o].ToString());
                    }
                    sections++;
                }
                storage.write(wb);
                byte[] data = wb.ToArray();
                data[0] = (byte)sections;
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
                    byte sections = (byte)rb.readByte();
                    history.Items.Clear();
                    for (int i = 0; i < sections; i++) {
                        String sectorName = rb.readString();
                        int sectorSize = rb.readInt();
                        if (sectorName != name) { // Not our sector, pass
                            for (int o = 0; o < sectorSize; o++) {
                                rb.readString();
                            }
                        } else { // Our sector, get our data
                            for (int o = 0; o < sectorSize; o++) {
                                String str = rb.readString();
                                if (str != "") {
                                    history.Items.Add(str);
                                }
                            }
                            break;
                        }
                    }
                    storage.read(rb);
                }
            } catch (OutOfBoundsReadException) { // Pass read beyond
                return;
            }
        }
    }
}
