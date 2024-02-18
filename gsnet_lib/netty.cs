using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gsnet_sharp
{
    public class NettySessionReadonly : ISessionReadonly
    {
        public ISessionReadonly? sess_;

        public ulong GetId()
        {
            if (sess_ == null) return 0;
            return sess_.GetId();
        }
    }

    public class NettySession : ISession
    {
        public ISession sess_;
        public IPacketEncoder encoder_;

        public NettySession(ISession sess, IPacketEncoder encoder)
        {
            sess_ = sess;
            encoder_ = encoder;
        }

        public void Close()
        {
            sess_.Close();
        }

        public void SetData(string key, object obj)
        {
            sess_.SetData(key, obj);
        }

        public object? GetData(string key)
        {
            return sess_.GetData(key);
        }

        public ulong GetId()
        {
            return sess_.GetId();
        }

        public void Send(byte[] data, int offset, int len)
        {
            sess_.Send(data, offset, len);
        }
    }

    /*
     +----------------------------------------------------------------------+
     |                          MagicNumber 4bytes                          | 
     +----------------------------------------------------------------------+ 
     | 协议版本号 1byte | 序列化算法 1byte |    指令 1byte   |    状态 1byte   |
     +----------------------------------------------------------------------+
     |                          保留字段  4bytes                             |
     +----------------------------------------------------------------------+
     |                          数据长度  4bytes                             |
     +----------------------------------------------------------------------+
     |                                                                      |
     */
    public class LengthFieldPacketHeader
    {
        public byte version_;
        public byte serializationAlgorithm_;
        public byte command_;
        public byte status_;
        public int reserved_;
        public int dataLength_;
    }

    // 基于长度域格式的包
    public class LengthFieldPacket : LengthFieldPacketHeader, IPacket
    {
        internal byte[]? data_;
        internal int offset_;

        public byte Version() { return version_; }
        public byte[]? Data() { return data_; }
        public int Offset() { return offset_; }
        public int Length() { return dataLength_; }
    }

    public class LengthFieldPacketEncoder : IPacketEncoder
    {
        public int EncodeHeader(in IPacket packet, byte[] data, int offset, int len)
        {
            throw new NotImplementedException();
        }
    }

    public class LengthFieldPacketDecoder : IPacketDecoder
    {
        public int DecodeHeader(byte[] data, int offset, int len, out IPacket packet)            
        {
            packet = packet_;

            // 数据长度不够包头长度
            if (len < LengthFieldBasedFrameDecoder.HeaderLength)
            {
                return 0;
            }

            int processedLength = 0;
            // MagicNumber
            int magicNumber = Utils.Bytes2Int(data, offset);
            if (magicNumber != LengthFieldBasedFrameDecoder.MagicNumber)
            {
                return ErrorCode.NettyPacketFormatInvalid;
            }
            processedLength += 4;
            // Version
            if (data[processedLength] != LengthFieldBasedFrameDecoder.Version)
            {
                return ErrorCode.NettyPacketFormatInvalid;
            }
            packet_.version_ = data[processedLength];
            processedLength += 1;
            // Serialization Algorithm
            packet_.serializationAlgorithm_ = data[processedLength];
            processedLength += 1;
            // Command
            packet_.command_ = data[processedLength];
            processedLength += 1;
            // Status
            packet_.status_ = data[processedLength];
            processedLength += 1;
            // Reserved
            packet_.reserved_ = Utils.Bytes2Int(data, offset + processedLength);
            processedLength += 4;
            // Data Length
            int dataLength = Utils.Bytes2Int(data, offset + processedLength);
            packet_.dataLength_ = dataLength;
            processedLength += 4;
            // 数据长度足够
            if (len - processedLength >= dataLength)
            {
                packet_.data_ = data;
                packet_.offset_ = offset;
                processedLength += dataLength;
            }
            else
            {
                packet_.data_ = null;
            }
            return processedLength;
        }

        LengthFieldPacket packet_ = new LengthFieldPacket();
    }

    public class NettySessionHandler : ISessionHandler
    {
        public NettySessionHandler(INettyHandler nettyHandler, IPacketEncoder encoder, IPacketDecoder decoder)
        {
            nettyHandler_ = nettyHandler;
            packetEncoder_ = encoder;
            packetDecoder_ = decoder;
        }

        public void OnInit()
        {
            nettyHandler_.OnInit();
        }

        public void OnUninit()
        {
            nettyHandler_.OnUninit();
        }

        public void OnConnected(ISessionReadonly sess)
        {
            if (sessReadonly_ == null)
            {
                sessReadonly_ = new NettySessionReadonly();
                sessReadonly_.sess_ = sess;
            }
            nettyHandler_.OnConnected(sessReadonly_);
        }

        public void OnDisconnected(ISession sess)
        {
            var ns = prepareNettySession(sess);
            nettyHandler_.OnDisconnected(ns);
        }

        public int OnData(ISession sess, in ByteSlice slice, out LengthResult result)
        {
            result.ProcessedLength = 0;
            result.NextLength = 0;

            int processed = 0;
            while (processed < slice.Length)
            {
                IPacket recvPacket;
                var p = packetDecoder_.DecodeHeader(slice.Data, processed, slice.Length, out recvPacket);
                if (p < 0) { return p; } // 有错误
                else if (p == 0) { break; } // 未处理数据跳出
                processed += p;
                if (recvPacket.Data() == null) {
                    result.NextLength = recvPacket.Length();
                    break;
                }
                var ns = prepareNettySession(sess);
                nettyHandler_.OnPacket(ns, recvPacket);
            }

            result.ProcessedLength = processed;
            return 0;
        }

        public void OnError(ISession sess, int error)
        {
            var ns = prepareNettySession(sess);
            nettyHandler_.OnError(ns, error);
        }

        public void OnTick(ISession sess, int millisecs)
        {
            var ns = prepareNettySession(sess);
            nettyHandler_.OnTick(ns, millisecs);
        }

        NettySession prepareNettySession(ISession sess)
        {
            if (sess_ == null)
            {
                sess_ = new NettySession(sess, packetEncoder_);
            }
            return sess_;
        }

        INettyHandler nettyHandler_;
        NettySessionReadonly? sessReadonly_;
        NettySession? sess_;
        IPacketEncoder packetEncoder_;
        IPacketDecoder packetDecoder_;
    }
}
