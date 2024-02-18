using System.Net.Sockets;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace gsnet_sharp
{
    internal class Connection
    {
        public Connection(Socket s)
        {
            s_ = s;
        }

        public async Task ReceiveAsync(byte[] buf, int offset, int len)
        {
            if (closed_) { return; }

            int r = 0;
            while (r < len) {
                try
                {
                    var n = await s_.ReceiveAsync(new ArraySegment<byte>(buf, offset+r, len-r));
                    if (n == 0)
                    {
                        close();
                        break;
                    }
                    r += n;
                }
                catch (ObjectDisposedException)
                {
                    
                }
                catch (SocketException)
                {

                }
            }
        }

        public async Task ReceiveAsync(byte[] buf)
        {
            await ReceiveAsync(buf, 0, 0);
        }

        public async Task<int> ReceiveSomeAsync(byte[] buf, int offset, int len)
        {
            if (closed_) { return -1; }
            return await s_.ReceiveAsync(new ArraySegment<byte>(buf, offset, len));
        }

        public async Task SendAsync(byte[] buf, int offset, int len)
        {
            if (closed_) { return; }

            int s = 0;
            while (s < len)
            {
                try
                {
                    var n = await s_.SendAsync(new ArraySegment<byte>(buf, offset + s, len - s));
                    if (n == 0)
                    {
                        close();
                        break; 
                    }
                    s += n;
                }
                catch (ObjectDisposedException)
                {

                }
                catch (SocketException)
                {

                }
            }
        }

        public async Task SendAsync(byte[] buf)
        {
            await SendAsync(buf, 0, 0);
        }

        public void Close()
        {
            close();
        }

        public void CloseWait(int secs)
        {
            if (closed_)
            {
                return;
            }

            if (secs > 0) {
                LingerOption option = new LingerOption(true, secs);
                s_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, option);
            }

            close();
        }

        public bool IsClosed()
        {
            return closed_;
        }

        void close()
        {
            if ( s_ != null)
            {
                s_.Close();
                closed_ = true;
            }
        }

        Socket s_;
        bool closed_;
    }
}
