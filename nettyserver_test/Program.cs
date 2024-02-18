using gsnet_sharp;
using System.Net;
using System.Net.Sockets;

static class Program
{
    class TestNettySessionHandler : INettyHandler
    {
        public void OnInit()
        {
            Console.WriteLine("session handler inited");
        }

        public void OnUninit()
        {
            Console.WriteLine("session handler uninint");
        }

        public void OnConnected(NettySessionReadonly sess)
        {
            Console.WriteLine("session {0} connected", sess.GetId());
        }

        public void OnDisconnected(NettySession sess)
        {
            Console.WriteLine("session {0} disconnected", sess.GetId());
        }

        public void OnPacket(NettySession sess, IPacket packet)
        {
            throw new NotImplementedException();
        }

        public void OnTick(NettySession sess, int millisecs)
        {
            throw new NotImplementedException();
        }

        public void OnError(NettySession sess, int error)
        {
            Console.WriteLine("session {0} occur error {1}", sess.GetId(), error);
        }
    }

    static int Main(string[] args)
    {
        var server = new NettyServer(new NettyHandlerInfo
        {
            NewHandler = (params object[] paramList) => {
                return new TestNettySessionHandler();
            }
        });
        var address = "127.0.0.1:9000";
        if (!server.Listen(address))
        {
            Console.WriteLine("server start failed");
            return -1;
        }

        server.Serve();

        Console.WriteLine("start server with {0}", address);

        string[] sendStrings = {
            "aaaaa", "bbbbb", "ccccc", "ddddd", "eeeee", "fffff", "ggggg", "hhhhh", "iiiii", "jjjjj", "kkkkk", "lllll", "mmmmm", "nnnnn"
        };

        for (int c = 0; c < 50; c++)
        {
            TcpClient client = new TcpClient();
            client.Connect(IPEndPoint.Parse(address));
            var netStream = client.GetStream();
            for (int i = 0; i < sendStrings.Length; i++)
            {
                var data = System.Text.Encoding.Default.GetBytes(sendStrings[i]);
                netStream.Write(data);
            }

            byte[] buf = new byte[128];
            for (int i = 0; i < sendStrings.Length; i++)
            {
                var n = netStream.Read(buf, 0, sendStrings[i].Length);
                Console.WriteLine("client received echo data: {0}", buf);
            }
            Thread.Sleep(1);
            client.Close();
        }
        Console.WriteLine("press any key to exit ...");
        Console.ReadKey();
        server.Close();

        return 0;
    }
}

