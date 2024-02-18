using gsnet_sharp;
using System.Net;
using System.Net.Sockets;

static class Program
{
    class SessionHandler : ISessionHandler
    {
        public void OnInit()
        {

        }

        public void OnUninit()
        {

        }

        public void OnConnected(ISessionReadonly sess)
        {
            Console.WriteLine("session {0} connected", sess.GetId());
        }

        public int OnData(ISession sess, in ByteSlice buffer, out LengthResult result)
        {
            if (buffer.Data == null || buffer.Length == 0)
            {
                Console.WriteLine("session {0} received empty data", sess.GetId());
                result.ProcessedLength = 0;
                result.NextLength = 0;
                return 0;
            }
            var dataSegment = new ArraySegment<byte>(buffer.Data, 0, buffer.Length);
            Console.WriteLine("session {0} received data: {1}", sess.GetId(), dataSegment.ToArray());
            sess.Send(buffer.Data, 0, buffer.Length);
            result.ProcessedLength = buffer.Length;
            result.NextLength = 0;
            return 0;
        }

        public void OnDisconnected(ISession sess)
        {
            Console.WriteLine("session {0} disconnected", sess.GetId());
        }

        public void OnError(ISession sess, int error)
        {
            Console.WriteLine("session {0} occur error {1}", sess.GetId(), error);
        }

        public void OnTick(ISession sess, int millisecs)
        {
            throw new NotImplementedException();
        }
    }

    static int Main(string[] args)
    {
        var server = new TcpServer(new SessionHandlerInfo { NewHandler = (params object[] paramList)=> {
            return new SessionHandler();
        } });
        var address = "127.0.0.1:9000";
        var ts = server.ListenAndServe(address);

        Console.WriteLine("start server with {0}", address);

        string[] sendStrings = {
            "aaaaa", "bbbbb", "ccccc", "ddddd", "eeeee", "fffff", "ggggg", "hhhhh", "iiiii", "jjjjj", "kkkkk", "lllll", "mmmmm", "nnnnn"
        };

        for (int c = 0; c < 1000; c++)
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
