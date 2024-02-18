using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsnet_sharp
{
    public class SessionReadonly : ISessionReadonly
    {
        ulong id_;

        internal SessionReadonly(ulong id)
        {
            id_ = id;
        }

        public ulong GetId()
        {
            return id_;
        }
    }

    public class Session : ISession
    {
        Connection conn_;
        ulong id_;
        Dictionary<string, object>? userData_;

        internal Session(Connection conn, ulong id)
        {
            conn_ = conn;
            id_ = id;
        }

        public void Close()
        {
            conn_.Close();
        }

        public ulong GetId()
        {
            return id_;
        }

        public object? GetData(string key)
        {
            if (userData_ == null) return null;
            if (!userData_.TryGetValue(key, out var value)) return null;
            return value;
        }

        public async void Send(byte[] data, int offset, int len)
        {
            await conn_.SendAsync(data, offset, len);
        }

        public void SetData(string key, object userData)
        {
            if (userData_ == null)
            {
                userData_ = new Dictionary<string, object>();
            }
            userData_[key] = userData;
        }
    }
}
