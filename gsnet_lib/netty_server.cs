using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace gsnet_sharp
{
    // 包头接口
    public interface IPacketHeader
    {
        int Length();
        int ContentLength();
    }

    // 包接口
    public interface IPacket
    {
        byte[]? Data();
        int Offset();
        int Length();
    }

    // netty处理器接口
    public interface INettyHandler
    {
        public void OnInit();
        public void OnUninit();
        public void OnConnected(NettySessionReadonly sess);
        public void OnDisconnected(NettySession sess);
        public void OnPacket(NettySession sess, IPacket packet);
        public void OnTick(NettySession sess, int millisecs);
        public void OnError(NettySession sess, int error);
    }

    public delegate INettyHandler NewNettyHandler(params object[] paramList);

    public struct NettyHandlerInfo
    {
        public NewNettyHandler NewHandler;
        public object[] ParamList;
        public IPacketEncoder Encoder;
        public IPacketDecoder Decoder;

    }

    public class NettyServerConfigure : ServerConfigure
    {
    }

    public class NettyServer
    {
        public NettyServer(NettyHandlerInfo handlerInfo)
        {
            SessionHandlerInfo sessHandlerInfo;
            sessHandlerInfo.NewHandler = prepareHandler(handlerInfo.NewHandler, handlerInfo.Encoder, handlerInfo.Decoder);
            sessHandlerInfo.ParamList = handlerInfo.ParamList;
            s_ = new TcpServer(sessHandlerInfo);
        }

        public NettyServer(NettyHandlerInfo handlerInfo, NettyServerConfigure config)
        {
            SessionHandlerInfo sessHandlerInfo;
            sessHandlerInfo.NewHandler = prepareHandler(handlerInfo.NewHandler, handlerInfo.Encoder, handlerInfo.Decoder);
            sessHandlerInfo.ParamList = handlerInfo.ParamList;
            s_ = new TcpServer(sessHandlerInfo, config);
            config_ = config;
        }

        public bool Listen(string address)
        {
            return s_.Listen(address);
        }

        public async void Serve()
        {
            await s_.Serve();
        }

        public Task<bool> ListenAndServe(string address)
        {
            return s_.ListenAndServe(address);
        }

        public void Close()
        {
            s_.Close();
        }

        NewSessionHandler prepareHandler(NewNettyHandler newNettyHandler, IPacketEncoder encoder, IPacketDecoder decoder)
        {
            return (params object[] paramList) =>
            {
                var nettyHandler = newNettyHandler(paramList);
                NettySessionHandler handler = new NettySessionHandler(nettyHandler, encoder, decoder); 
                return handler;
            };
        }

        readonly NettyServerConfigure? config_;
        readonly TcpServer s_;
    }
}
