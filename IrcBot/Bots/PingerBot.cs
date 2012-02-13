using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace IrcBot.Core.Bots
{
    internal class PingerBot : IIrcBot
    {
        Timer _timer;
        IrcContext _context;
        string _server;
        DateTime _lastPing;

        public PingerBot(string server)
        {
            _server = server;
            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Interval = 60000;
            _timer.Elapsed += Ping;
            Latency = TimeSpan.Zero;
        }

        private void Ping(object sender, ElapsedEventArgs e)
        {
            _lastPing = DateTime.Now;
            _context.Write("PING {0}", _server);
        }

        public void IncomingMessage(IrcContext context)
        {
            if(context.Command.Equals("PONG", StringComparison.InvariantCultureIgnoreCase))
            {
                Latency = DateTime.Now.Subtract(_lastPing);
            }

            if(context.Command == "001")
            {
                _server = context.Prefix;
                _context = context;
                _timer.Start();
            }

            if(context.Command == "PING")
            {
                // the server pings the client?!  It happens!
                context.Write("PONG {0}", context.Parameters.Last());
            }
        }

        public string Name
        {
            get
            {
                return "Pinger Bot";
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public TimeSpan Latency { get; private set; }
    }
}
