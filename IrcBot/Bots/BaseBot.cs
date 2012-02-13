using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBot.Core.Bots;
using IrcBot.Core.Core.Helpers;

namespace IrcBot.Core.Bots
{
    public abstract class BaseBot : IIrcBot
    {
        public void IncomingMessage(IrcContext context)
        {
            if(context.IsServerMessage)
            {
                OnServerMessage(context);
            }
            else
            {
                switch(context.Command.ToUpper())
                {
                    case "PRIVMSG":
                        OnPrivateMessage(context);
                        break;
                    case "JOIN":
                        OnJoin(context);
                        break;
                    case "MODE":
                        OnMode(context);
                        break;
                    case "KICK":
                        OnKick(context);
                        break;
                    case "PART":
                        OnPart(context);
                        break;
                    case "TOPIC":
                        OnTopic(context);
                        break;
                    default:
                        OnOtherMessage(context);
                        break;
                }
            }
        }

        protected virtual void OnServerMessage(IrcContext context) { }

        protected virtual void OnPrivateMessage(IrcContext context) { }

        protected virtual void OnJoin(IrcContext context) { }

        protected virtual void OnMode(IrcContext context) { }

        protected virtual void OnKick(IrcContext context) {
            if(context.InvolvesBotClient && AutoRejoin)
            {
                context.Join(context.Parameters.First());
            }
        }

        protected virtual void OnPart(IrcContext context) { }

        protected virtual void OnTopic(IrcContext context) { }

        protected virtual void OnOtherMessage(IrcContext context) { }

        protected DateTime FromUnixTimestamp(string raw)
        {
            long secs;
            if(!long.TryParse(raw, out secs))
            {
                throw new ArgumentException(string.Format("Unable to parse value of '{0}' to long.", raw));
            }

            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return epoch.AddSeconds(secs).ToLocalTime();
        }

        public abstract string Name { get; }

        public abstract bool Enabled { get; set; }

        public bool AutoRejoin { get; set; }
    }
}
