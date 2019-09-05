using System;
using System.Text;
using WpfApplication1;

namespace QChkUI {
    public unsafe class UnsafeWriteBuffer {

        private unsafe byte* data;
        private int position = 0;

        public unsafe UnsafeWriteBuffer(byte* data) {
            this.data = data;
        }

        public void writeBool(bool value) {
            writeByte(value ? 1 : 0);
        }

        public void writeByte(int value) {
            data[position] = (byte) (value & 0xff);
            position++;
        }

        public void writeShort(int value) {
            writeByte((value >> 0) & 0xff);
            writeByte((value >> 8) & 0xff);
        }

        public void writeInt(int value) {
            writeShort((value >> 0) & 0xffff);
            writeShort((value >> 16) & 0xffff);
        }

        public void writeLong(long value) {
            writeShort((int)((value >> 0) & 0xffffffff));
            writeShort((int)((value >> 32) & 0xffffffff));
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

        public String dump() {
            StringBuilder sb = new StringBuilder();
            sb.Append("byte[" + this.position + "] = {");
            for(int i = 0; i <position;i++) {
                byte b = data[i];
                sb.Append("0x"+b.ToString("X2"));
                if( i != position-1) {
                    sb.Append(", ");
                }
            }
            sb.Append("};");
            return sb.ToString();
        }

        public void writeBytePtr(byte* bptr) {
            int size = IntPtr.Size;
            if (size == 1) {
                this.writeByte((int)bptr);
            } else if(size == 2) {
                this.writeShort((short)bptr);
            } else if(size == 4) {
                this.writeInt((int)bptr);
            } else if(size == 8) {
                this.writeLong((long)bptr);
            }
        }
    }
}
