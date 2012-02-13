using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBot.Core.Bots
{
    public class CtcpBot : IIrcBot
    {
        const char DELIM = '\x001';

        public void IncomingMessage(IrcContext context)
        {
            if(context.Parameters.Any())
            {
                var msg = context.Parameters.Last();
                if(msg.First() == DELIM && msg.Last() == DELIM)
                {
                    HandleCtcpCommand(context);
                }
            }
        }

        private void HandleCtcpCommand(IrcContext context)
        {
            var msg = context.Parameters.Last().Trim(DELIM);
            var cmd = msg.Split(' ')[0];

            switch(cmd.ToUpper())
            {
                case "VERSION":
                    context.Write("{1} {2}: {0}VERSION {3}:{4}:{5}{0}", DELIM, context.Command, context.Nickname, "Nothing Special", "0.1b", "Windows 3.11");
                    break;
            }
        }

        public string Name
        {
            get { return "CTCP Bot"; }
        }

        public bool Enabled
        {
            get;
            set;
        }
    }
}