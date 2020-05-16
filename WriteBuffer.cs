using System;
using System.IO;
using System.Text;

namespace WpfApplication1 {
    class WriteBuffer {

        private MemoryStream me;

        public void writeByte(int value) {
            me.WriteByte((byte) (value & 0xff));
        }

        public void writeShort(int value) {
            writeByte((value >> 8) & 0xff);
            writeByte((value >> 0) & 0xff);
        }

        public void writeInt(int value) {
            writeShort((value >> 16) & 0xffff);
            writeShort((value >> 0) & 0xffff);
        }

        public void writeLong(long value) {
            writeInt((int) ((value >> 32) & 0xffffffff));
            writeInt((int) ((value >> 0) & 0xffffffff));
        }

        public void writeBool(bool value) {
            writeByte(value ? 1 : 0);
        }

        public void writeByteArray(byte[] data) {
            writeInt(data.Length);
            for (int i = 0; i < data.Length; i++) {
                byte chr = data[i];
                writeByte(chr);
            }
        }

        public void writeString(String value, String encoding) {
            value = value == null ? "" : value;
            byte[] str = Encoding.GetEncoding(encoding).GetBytes(value);
            writeByteArray(str);
        }

        public void writeArray(String[] ba, String encoding) {
            writeInt(ba.Length);
            for (int i = 0; i < ba.Length; i++) {
                writeString(ba[i], encoding);
            }
        }

        public void writeArrayArray(String[][] ba, String[] encodings) {
            writeInt(ba.Length);
            for (int i = 0; i < ba.Length; i++) {
                writeArray(ba[i], encodings[i]);
            }
        }

        public WriteBuffer() {
            this.me = new MemoryStream();
        }

        public byte[] ToArray() {
            return me.ToArray();
        }

        public void writeArray(byte[] ba) {
            for (int i = 0; i < ba.Length; i++) {
                writeByte((byte) ba[i]);
            }
        }

        public void writeArray(short[] ba) {
            for (int i = 0; i < ba.Length; i++) {
                writeShort((short) ba[i]);
            }
        }

        public void writeArray(int[] ba) {
            for (int i = 0; i < ba.Length; i++) {
                writeInt((int) ba[i]);
            }
        }

        internal void writeIntAt(int index, int value) {
            long cp = me.Position;
            me.Position = index;
            writeInt(value);
            me.Position = cp + 4;
        }
    }
}
