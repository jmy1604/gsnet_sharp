using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace gsnet_sharp
{
    public delegate ISessionHandler NewSessionHandler(params object[] paramList);

    public struct SessionHandlerInfo
    {
        public NewSessionHandler NewHandler;
        public object[] ParamList;
    }

    public class ServerConfigure
    {
        public int DefaultRecvBuffLength;
        public int MinRecvBuffLength;
        public int MaxRecvBuffLength;
    }

    public class TcpServer
    {
        public TcpServer(SessionHandlerInfo handlerInfo)
        {
            handlerInfo_ = handlerInfo;
            sessDic_ = new ConcurrentDictionary<ulong, ISession>();
        }

        public TcpServer(SessionHandlerInfo handlerInfo, ServerConfigure config)
        {
            handlerInfo_ = handlerInfo;
            config_ = config;
            sessDic_ = new ConcurrentDictionary<ulong, ISession>();
        }

        public bool Listen(string address)
        {
            if (isListening_) { return false; }

            try
            {
                listenSocket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint ep = IPEndPoint.Parse(address);
                listenSocket_.Bind(ep);
                listenSocket_.Listen();
                isListening_ = true;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }

            return true;
        }

        public async Task Serve()
        {
            if (listenSocket_ == null) { return; }

            while (isListening_)
            {
                try
                {
                    var newSocket = await listenSocket_.AcceptAsync();
                    var newConn = new Connection(newSocket);
                    handleConnAsync(newConn);
                }
                catch (InvalidOperationException e)
                {
                    if (e is ObjectDisposedException)
                    {

                    }
                }
                catch (SocketException)
                {

                }
            }
        }

        public async Task<bool> ListenAndServe(string address)
        {
            if (!Listen(address)) { return false; }
            await Serve();
            return true;
        }

        public void Close()
        {
            listenSocket_?.Close();
            listenSocket_ = null;
            isListening_ = false;
        }


        async void handleConnAsync(Connection conn)
        {
            var id = sessionIdCounter_ += 1;
            var sess = new Session(conn, id);
            if (!sessDic_.TryAdd(id, sess))
            {
                // 处理添加错误
            }

            // 创建会话处理器
            var handler = handlerInfo_.NewHandler(handlerInfo_.ParamList);

            // 初始化
            handler.OnInit();

            // 已连接事件
            handler.OnConnected(sess);

            byte[] defaultRecvBuf;
            if (config_ != null && config_.DefaultRecvBuffLength > 0)
            {
                defaultRecvBuf = new byte[config_.DefaultRecvBuffLength];
            }
            else
            {
                defaultRecvBuf = new byte[defaultRecvBufLength];
            }

            byte[] recvBuf = defaultRecvBuf;
            int maxRecvLength = config_ != null && config_.MaxRecvBuffLength > 0 ? config_.MaxRecvBuffLength : defaultMaxRecvBufLength;
            bool running = true;
            int offset = 0;
            int err;
            ByteSlice bs;
            while (running)
            {
                // TODO 接收缓冲区不是默认缓冲区时，要根据一定的策略释放掉恢复到默认，防止长时间占用更大的内存
                var r = await conn.ReceiveSomeAsync(recvBuf, offset, recvBuf.Length-offset);
                if (r <= 0) { break; }

                bs.Data = recvBuf;
                bs.Length = offset+r;
                err = handler.OnData(sess, bs, out var ls);

                // 处理数据错误
                if (err < 0) {
                    handler.OnError(sess, err);
                    break;
                }

                // 处理的长度超过数据长度
                if (ls.ProcessedLength > bs.Length)
                {
                    handler.OnError(sess, ErrorCode.ProcessedLengthError);
                    break;
                }

                // 下一次接收的长度超出最大接收长度
                if (ls.NextLength > maxRecvLength)
                {
                    handler.OnError(sess, ErrorCode.RecvLengthTooLong);
                    break;
                }

                // 正好处理完接收的数据
                if (r == ls.ProcessedLength) {
                    offset = 0;
                    continue;
                }

                offset = r - ls.ProcessedLength;

                // 未处理完的移动到缓冲区最前面
                Array.Copy(recvBuf, ls.ProcessedLength, recvBuf, 0, offset);

                // 需要的长度大于当前缓冲区长度
                if (ls.NextLength > recvBuf.Length)
                {
                    var newBuf = new byte[ls.NextLength];
                    // 移动到新缓冲区
                    Array.Copy(recvBuf, 0, newBuf, 0, offset);
                    recvBuf = newBuf;
                }
            }
            // 关闭连接
            conn.Close();
            // 断开事件
            handler.OnDisconnected(sess);
            if (!sessDic_.TryRemove(sess.GetId(), out var removedSess))
            {
                // 处理错误
            }
            if (removedSess != sess)
            {
                // 处理错误
            }
            // 反初始化事件
            handler.OnUninit();
            // TODO 回收处理器handler到对象池
        }

        SessionHandlerInfo handlerInfo_;
        ServerConfigure? config_;
        Socket? listenSocket_;
        bool isListening_;
        ulong sessionIdCounter_;
        ConcurrentDictionary<ulong, ISession> sessDic_;

        static int defaultRecvBufLength = 4096;
        static int defaultMaxRecvBufLength = 256 * 1024;
    }
}
