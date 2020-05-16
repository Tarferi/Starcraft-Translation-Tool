using System;
using System.Text;

namespace WpfApplication1 {
    unsafe class UnsafeReadBuffer {

        private unsafe byte* data;
        private int position;

        public int readByte() {
            byte value = data[position];
            position++;
            return (int) (value & 0xff);
        }

        public bool readBool() {
            return (readByte() == 1);
        }

        public int readShort() {
            return (readByte() << 0) | (readByte() << 8);
        }

        public int readInt() {
            return (readShort() << 0) | (readShort() << 16);
        }

        public String readString(String encoding) {
            int length = readInt();
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++) {
                bytes[i] = (byte) readByte();
            }
            String str = Encoding.GetEncoding(encoding).GetString(bytes);
            return str;
        }

        public UnsafeReadBuffer(byte* data) {
            this.data = data;
            this.position = 0;
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

        public byte* readBytePtr() {
            return (byte*) readInt();
        }
    }
}
