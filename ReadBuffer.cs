using System;
using System.IO;
using System.Text;

namespace WpfApplication1 {
    class OutOfBoundsReadException : Exception {

    }

    class ReadBuffer {

        private MemoryStream me;

        public int readByte() {
            if(me.Length - me.Position == 0) {
                throw new OutOfBoundsReadException();
            }
            return me.ReadByte();
        }

        public bool readBool() {
            return (readByte() == 1);
        }

        public int readShort() {
            return (readByte() << 8) | (readByte() << 0);
        }

        public int readInt() {
            return (readShort() << 16) | (readShort() << 0);
        }

        public long readLong() {
            return (((long)readInt()) << 32) | (((long)readInt()) << 0);
        }

        public String[][] readStringArrayArray() {
            int length = readInt();
            String[][] strs = new String[length][];
            for (int i = 0; i < length; i++) {
                strs[i] = readStringArray();
            }
            return strs;
        }

        public String[] readStringArray() {
            int length = readInt();
            String[] strs = new String[length];
            for (int i = 0; i < length; i++) {
                strs[i] = readString();
            }
            return strs;
        }

        public String readString() {
            int length = readInt();
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++) {
                bytes[i] = (byte)readByte();
            }
            String str = Encoding.GetEncoding("EUC-KR").GetString(bytes);
            return str;
        }

        public ReadBuffer(MemoryStream me) {
            this.me = me;
        }

        public ReadBuffer(byte[] data) {
            this.me = new MemoryStream(data);
        }

        public ReadBuffer(char[] data) {
            byte[] bData = new byte[data.Length];
            for(int i = 0; i < data.Length; i++) {
                bData[i] = (byte) data[i];
            }
            this.me = new MemoryStream(bData);
        }

        public byte[] readByteArray(int length) {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++) {
                data[i] = (byte)readByte();
            }
            return data;
        }

        public short[] readShortArray(int length) {
            short[] data = new short[length];
            for (int i = 0; i < length; i++) {
                data[i] = (short)readShort();
            }
            return data;
        }

        public int[] readIntArray(int length) {
            int[] data = new int[length];
            for (int i = 0; i < length; i++) {
                data[i] = (int)readInt();
            }
            return data;
        }
    }
}
