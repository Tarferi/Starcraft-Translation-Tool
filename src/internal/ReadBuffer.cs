using System;
using System.Collections.Generic;
using System.Text;

namespace TranslatorData {

    class OutOfBoundsReadException : Exception {

    }

    class SimpleMemoryStream {

        private byte[] data;
        private int position = 0;

        public SimpleMemoryStream(byte[] data) {
            this.data = data;
        }

        public int Length { get { return data.Length; } }
        public int Position { get { return position; } }

        public byte ReadByte() {
            if (position == data.Length) {
                throw new OutOfBoundsReadException();
            }
            position++;
            return data[position - 1];
        }
    }

    class ReadBuffer {

        private SimpleMemoryStream me;

        private int DataLeft { get { return (int) (me.Length - me.Position); } }

        public int readByte() {
            if (DataLeft == 0) {
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
            return (((long) readInt()) << 32) | (((long) readInt()) << 0);
        }

        public String[][] readStringArrayArray(Encoding[] encodings) {
            List<String[]> lst = new List<String[]>();
            int length = readInt();
            for (int i = 0; i < length; i++) {
                lst.Add(readStringArray(encodings[i]));
                if (DataLeft == 0) {
                    length = i + 1;
                }
            }
            String[][] strs = new String[length][];
            for (int i = 0; i < length; i++) {
                strs[i] = lst[i];
            }
            return strs;
        }

        public String[] readStringArray(Encoding encoding) {
            List<String> lst = new List<String>();
            int length = readInt();
            for (int i = 0; i < length; i++) {
                lst.Add(readString(encoding));
                if (DataLeft == 0) {
                    length = i + 1;
                }
            }
            String[] strs = new String[length];
            for (int i = 0; i < length; i++) {
                strs[i] = lst[i];
            }
            return strs;
        }

        public byte[] readByteArray() {
            int length = readInt();
            if (length > DataLeft) {
                length = DataLeft;
            }
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++) {
                bytes[i] = (byte) readByte();
            }
            return bytes;
        }

        public String readString(Encoding encoding) {
            String str = encoding.GetString(readByteArray());
            return str;
        }

        public ReadBuffer(byte[] data) {
            this.me = new SimpleMemoryStream(data);
        }

        public ReadBuffer(char[] data) {
            byte[] bData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) {
                bData[i] = (byte) data[i];
            }
            this.me = new SimpleMemoryStream(bData);
        }

        public byte[] readByteArray(int length) {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++) {
                data[i] = (byte) readByte();
            }
            return data;
        }

        public short[] readShortArray(int length) {
            short[] data = new short[length];
            for (int i = 0; i < length; i++) {
                data[i] = (short) readShort();
            }
            return data;
        }

        public int[] readIntArray(int length) {
            int[] data = new int[length];
            for (int i = 0; i < length; i++) {
                data[i] = (int) readInt();
            }
            return data;
        }
    }
}
