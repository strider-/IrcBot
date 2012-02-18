using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBot.Core.Bots;
using IrcBot.Core.Helpers;
using System.Diagnostics;

namespace IrcBot.Core
{
    public class BotClient
    {
        private Connection _conn;
        private List<IIrcBot> _listeners;
        private ServerInfo _info;
        private PingerBot _pinger;
        private CtcpBot _ctcp;

        public BotClient(string server, int port, bool ssl)
        {
            Server = server;
            Port = port;
            UsingSSL = ssl;
            _info = new ServerInfo();
            
            _listeners = new List<IIrcBot> {
                (_pinger = new PingerBot(server)),
                (_ctcp = new CtcpBot())
            };
        }

        public void SetIdentity(string nickName, string userName, string realName)
        {
            NickName = nickName;
            UserName = userName;
            RealName = realName;
        }

        public void Connect()
        {
            _conn = new Connection(this, HandleMessage);
            _conn.Write("NICK {0}", NickName);
            _conn.Write("USER {0} {1} {2} :{3}", NickName, "*", 8, RealName);
        }

        public void AddBot(IIrcBot listener)
        {
            _listeners.Add(listener);
        }

        public void Close()
        {
            _conn.Disconnect();
            _conn.Dispose();
        }

        private void HandleMessage(Message msg)
        {
            Console.WriteLine(msg.Raw);
            var context = new IrcContext(msg, _conn, _info);

            if(context.GetReplyCode() == ServerReplyCode.ISupport)
            {
                var validParms = msg.Parameters.Skip(1).Take(msg.Parameters.Length - 2);
                _info.Append(validParms);
            }

            _listeners.ForEach(l =>
            {
                l.IncomingMessage(context);
            });
        }

#if DEBUG
        public void Write(string s)
        {
            _conn.Write(s);
        }
#endif

        public string Server { get; private set; }
        public int Port { get; private set; }
        public bool UsingSSL { get; private set; }
        public string NickName { get; private set; }
        public string UserName { get; private set; }
        public string RealName { get; private set; }
        public TimeSpan Latency { get { return _pinger.Latency; } }
    }
}
