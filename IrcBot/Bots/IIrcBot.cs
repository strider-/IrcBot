using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBot.Core.Bots
{
    public interface IIrcBot
    {
        void IncomingMessage(IrcContext context);
        string Name { get; }
        bool Enabled { get; }
    }
}
