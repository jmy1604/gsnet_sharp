using System.Net.Sockets;

namespace gsnet_sharp
{
    // 字节切片
    public struct ByteSlice
    {
        public byte[] Data; // 原始数据字节数组
        public int Length; // 长度值
    }

    // 处理的长度结果
    public struct LengthResult
    {
        public int ProcessedLength; // 已经处理的长度
        public int NextLength; // 下次需要的长度
    }

    // 只读会话接口
    public interface ISessionReadonly
    {
        ulong GetId();
    }

    // 会话接口
    public interface ISession : ISessionReadonly
    {
        void Send(byte[] data, int offset, int len);
        void Close();
        void SetData(string key, object obj);
        object? GetData(string key);
    }

    // 会话处理器接口
    public interface ISessionHandler
    {
        public void OnInit();
        public void OnUninit();
        public void OnConnected(ISessionReadonly sess);
        public int OnData(ISession sess, in ByteSlice slice, out LengthResult result);
        public void OnTick(ISession sess, int millisecs);
        public void OnDisconnected(ISession sess);
        public void OnError(ISession sess, int error);
    }

    // 包编码器
    public interface IPacketEncoder
    {
        int EncodeHeader(in IPacket packet, byte[] data, int offset, int len);
    }

    // 包解码器
    public interface IPacketDecoder
    {
        int DecodeHeader(byte[] data, int offset, int len, out IPacket packet);
    }
}