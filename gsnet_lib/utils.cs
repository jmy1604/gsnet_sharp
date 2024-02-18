namespace gsnet_sharp
{
    internal class Utils
    {
        public static ulong Bytes2Ulong(byte[] data, int offset)
        {
            ulong n = (ulong)(data[offset] << 56) & 0xff00000000000000;
            n += (ulong)(data[offset + 1] << 48) & 0xff000000000000;
            n += (ulong)(data[offset + 2] << 40) & 0xff0000000000;
            n += (ulong)(data[offset + 3] << 32) & 0xff00000000;
            n += (ulong)(data[offset + 4] << 24) & 0xff000000;
            n += (ulong)(data[offset + 5] << 16) & 0xff0000;
            n += (ulong)(data[offset + 6] << 8) & 0xff00;
            n += (ulong)(data[offset + 7]) & 0xff;
            return n;
        }

        public static long Bytes2Long(byte[] data, int offset)
        {
            long n = data[offset] << 56 & 0x7f00000000000000;
            n += data[offset + 1] << 48 & 0xff000000000000;
            n += data[offset + 2] << 40 & 0xff0000000000;
            n += data[offset + 3] << 32 & 0xff00000000;
            n += data[offset + 4] << 24 & 0xff000000;
            n += data[offset + 5] << 16 & 0xff0000;
            n += data[offset + 6] << 8 & 0xff00;
            n += data[offset + 7] & 0xff;
            return n;
        }

        public static uint Bytes2Uint(byte[] data, int offset)
        {
            uint n = (uint)(data[offset] << 24) & 0x7f000000;
            n += (uint)(data[offset + 1] << 16) & 0xff0000;
            n += (uint)data[offset + 2] << 8 & 0xff00;
            n += (uint)data[offset + 3] & 0xff;
            return n;
        }

        public static int Bytes2Int(byte[] data, int offset)
        {
            int n = data[offset] << 24 & 0x7f000000;
            n += data[offset+1] << 16 & 0xff0000;
            n += data[offset+2] << 8 & 0xff00;
            n += data[offset+3] & 0xff;
            return n;
        }

        public static int Bytes2Ushort(byte[] data, int offset)
        {
            ushort n = (ushort)((data[offset] << 8) & 0x7f00);
            n += (ushort)(data[offset + 1] & 0xff);
            return n;
        }

        public static short Bytes2Short(byte[] data, int offset)
        {
            short n = (short)((data[offset] << 8) & 0x7f00);
            n += (short)(data[offset + 1] & 0xff);
            return n;
        }

        public static void Ulong2Bytes(ulong n, byte[] data, int offset)
        {
            data[offset] = (byte)(n >> 56 & 0xff);
            data[offset + 1] = (byte)(n >> 48 & 0xff);
            data[offset + 2] = (byte)(n >> 40 & 0xff);
            data[offset + 3] = (byte)(n >> 32 & 0xff);
            data[offset + 4] = (byte)(n >> 24 & 0xff);
            data[offset + 5] = (byte)(n >> 16 & 0xff);
            data[offset + 6] = (byte)(n >> 8 & 0xff);
            data[offset + 7] = (byte)(n & 0xff);
        }

        public static void Long2Bytes(long n, byte[] data, int offset)
        {
            data[offset] = (byte)(n >> 56 & 0xff);
            data[offset + 1] = (byte)(n >> 48 & 0xff);
            data[offset + 2] = (byte)(n >> 40 & 0xff);
            data[offset + 3] = (byte)(n >> 32 & 0xff);
            data[offset + 4] = (byte)(n >> 24 & 0xff);
            data[offset + 5] = (byte)(n >> 16 & 0xff);
            data[offset + 6] = (byte)(n >> 8 & 0xff);
            data[offset + 7] = (byte)(n & 0xff);
        }

        public static void Uint2Bytes(uint n, byte[] data, int offset)
        {
            data[offset] = (byte)(n >> 24 & 0xff);
            data[offset + 1] = (byte)(n >> 16 & 0xff);
            data[offset + 2] = (byte)(n >> 8 & 0xff);
            data[offset + 3] = (byte)(n & 0xff);
        }

        public static void Int2Bytes(int n, byte[] data, int offset)
        {
            data[offset] = (byte)(n >> 24 & 0xff);
            data[offset + 1] = (byte)(n >> 16 & 0xff);
            data[offset + 2] = (byte)(n >> 8 & 0xff);
            data[offset + 3] = (byte)(n & 0xff);
        }

        public static void Ushort2Bytes(ushort n, byte[] data, int offset)
        {
            data[offset] = (byte)(n >> 8 & 0xff);
            data[offset + 1] = (byte)(n & 0xff);
        }

        public static void Short2Bytes(short n, byte[] data, int offset)
        {
            data[offset] = (byte)(n >> 8 & 0xff);
            data[offset + 1] = (byte)(n & 0xff);
        }
    }
}