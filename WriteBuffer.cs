using System;
using System.IO;
using System.Text;

namespace WpfApplication1 {
    class WriteBuffer {

        private MemoryStream me;

        public void writeByte(int value) {
            me.WriteByte((byte)(value & 0xff));
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

        public void writeString(String value) {
            value = value == null ? "" : value;
            byte[] str = Encoding.GetEncoding("EUC-KR").GetBytes(value);
            writeInt(str.Length);
            for (int i = 0; i < str.Length; i++) {
                byte chr = str[i];
                writeByte(chr);
            }
        }

        public void writeArray(String[] ba) {
            writeInt(ba.Length);
            for (int i = 0; i < ba.Length; i++) {
                writeString(ba[i]);
            }
        }

        public void writeArrayArray(String[][] ba) {
            writeInt(ba.Length);
            for (int i = 0; i < ba.Length; i++) {
                writeArray(ba[i]);
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
                writeByte((byte)ba[i]);
            }
        }

        public void writeArray(short[] ba) {
            for (int i = 0; i < ba.Length; i++) {
                writeShort((short)ba[i]);
            }
        }

        public void writeArray(int[] ba) {
            for (int i = 0; i < ba.Length; i++) {
                writeInt((int)ba[i]);
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
