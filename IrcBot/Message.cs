using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBot.Core
{
    internal class Message
    {
        internal Message(string raw)
        {
            Raw = raw;
            string trail = null;
            int prefixIndex = Raw.IndexOf(" ");
            int trailingIndex = Raw.IndexOf(" :");            

            Prefix = Raw.Substring(1, prefixIndex - 1);

            if(trailingIndex > -1)
            {
                trail = Raw.Substring(trailingIndex + 2);
            }
            else
            {
                trailingIndex = Raw.Length;
            }

            var cmdAndParms = Raw.Substring(prefixIndex, trailingIndex - prefixIndex)
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Command = cmdAndParms.First();

            var parms = cmdAndParms.Skip(1).ToList();
            if(!string.IsNullOrWhiteSpace(trail))
                parms.Add(trail);

            Parameters = parms.ToArray();
        }

        public string Prefix { get; private set; }

        public string Command { get; private set; }

        public string[] Parameters { get; private set; }

        public string Raw { get; private set; }
    }
}
