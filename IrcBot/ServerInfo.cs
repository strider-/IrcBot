using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBot.Core
{
    /// <summary>
    /// Parses &amp; returns information from RPL_ISUPPORT replies
    /// </summary>
    public class ServerInfo
    {
        IDictionary<string, string> _dict;
        IDictionary<char, char> _prefixes;
        IDictionary<char, int> _chanLimit, _maxList, _idchan;
        IDictionary<string, int> _targmax;

        internal ServerInfo()
        {
            _dict = new Dictionary<string, string>();
        }

        internal void Append(IEnumerable<string> parameters)
        {
            foreach(var p in parameters)
            {
                string[] kp = p.Split('=');
                _dict[kp[0]] = kp.Length > 1 ? kp[1] : string.Empty;
            }
        }

        /// <summary>
        /// Gets all the parameter names supported by the server
        /// </summary>
        /// <returns></returns>
        public string[] GetParameterNames()
        {
            return _dict.Keys.ToArray();
        }

        private T Get<T>(string key)
        {
            if(!_dict.ContainsKey(key))
            {
                return default(T);
            }

            return (T)Convert.ChangeType(_dict[key], typeof(T));
        }

        private IDictionary<char, char> GetPrefixes()
        {
            var raw = Get<string>("PREFIX");
            if(raw == null)
            {
                return null;
            }

            int end = raw.IndexOf(')');
            var result = new Dictionary<char, char>();
            for(int i = 1; i < end; i++)
            {
                result[raw[i]] = raw[end + i];
            }

            return result;
        }

        private IEnumerable<KeyValuePair<string, int>> GetDictPairs(string raw)
        {
            return raw.Split(',').ToDictionary(
                k => k.Substring(0, k.IndexOf(':')),
                v => v.IndexOf(':') == v.Length - 1 ? int.MaxValue : int.Parse(v.Substring(v.IndexOf(':') + 1))
            );
        }

        private IDictionary<char, int> ParseDict(string raw)
        {
            if(raw == null)
            {
                return null;
            }
            var result = new Dictionary<char, int>();
            var pairs = GetDictPairs(raw);

            foreach(var pair in pairs)
            {
                string prefixes = pair.Key;
                foreach(char c in prefixes)
                {
                    result[c] = pair.Value;
                }
            }

            return result;
        }

        private IDictionary<string, int> GetTargetMax(string raw)
        {
            if(raw == null)
            {
                return null;
            }

            return (IDictionary<string, int>)GetDictPairs(raw);
        }

        private char[] GetChannelModes(int pos)
        {
            var modes = Get<string>("CHANMODES");
            if(modes == null)
            {
                return null;
            }

            return modes.Split(',')[pos].ToCharArray();
        }

        /// <summary>
        /// Gets whether or not RPL_ISUPPORT replies have been received
        /// </summary>
        public bool ReceivedInfo
        {
            get { return _dict.Any(); }
        }

        /// <summary>
        /// Gets a value from the server support list by parameter name
        /// </summary>
        /// <param name="key">Parameter name (case sensitive)</param>
        /// <returns></returns>
        public string this[string key]
        {
            get{ return Get<string>(key); }
        }

        /// <summary>
        /// Gets the IRC network name
        /// </summary>
        public string Network
        {
            get { return Get<string>("NETWORK"); }
        }

        /// <summary>
        /// Gets the supported channel prefixes
        /// </summary>
        public char[] ChannelTypes
        {
            get
            {
                var val = Get<string>("CHANTYPES");
                return val == null ? null : val.ToCharArray();
            }
        }

        /// <summary>
        /// Gets a dictionary of channel modes a person can get and the respective prefix a channel or nickname will get in case the person has it.
        /// </summary>
        /// <returns></returns>
        public IDictionary<char, char> Prefixes
        {
            get { return _prefixes ?? (_prefixes = GetPrefixes()); }
        }

        /// <summary>
        /// Gets the maximum number of channel modes with parameter allowed per MODE command.
        /// </summary>
        public int Modes
        {
            get { return Get<int>("MODES"); }
        }

        /// <summary>
        /// Gets the maximum nickname length.
        /// </summary>
        public int NicknameLength
        {
            get { return Get<int>("NICKLEN"); }
        }

        /// <summary>
        /// Gets whether or not the server support ban exceptions.
        /// </summary>
        public bool AllowsBanExceptions
        {
            get { return _dict.ContainsKey("EXCEPTS"); }
        }

        /// <summary>
        /// Gets whether or not the server support invite exceptions.
        /// </summary>
        public bool AllowsInviteExceptions
        {
            get { return _dict.ContainsKey("INVEX"); }
        }

        /// <summary>
        /// Gets the case mapping used for nick- and channel name comparing.
        /// </summary>
        public string NicknameCaseMapping
        {
            get { return Get<string>("CASEMAPPING"); }
        }

        /// <summary>
        /// Gets the maximum topic length.
        /// </summary>
        public int MaximumTopicLength
        {
            get { return Get<int>("TOPICLEN"); }
        }

        /// <summary>
        /// Gets the maximum kick comment length.
        /// </summary>
        public int MaximumKickMessageLength
        {
            get { return Get<int>("KICKLEN"); }
        }

        /// <summary>
        /// Gets the maximum channel name length
        /// </summary>
        public int MaximumChannelNameLength
        {
            get { return Get<int>("CHANNELLEN"); }
        }

        /// <summary>
        /// Gets the max length of an away message
        /// </summary>
        public int MaximumAwayMessageLength
        {
            get { return Get<int>("AWAYLEN"); }
        }

        /// <summary>
        /// Gets whether or not the	LIST command result is sent in multiple iterations so send queue won't fill and kill the client connection.
        /// </summary>
        public bool Safelist
        {
            get { return _dict.ContainsKey("SAFELIST"); }
        }

        /// <summary>
        /// Gets whether or not the KNOCK command exists.
        /// </summary>
        public bool SupportsKnock
        {
            get { return _dict.ContainsKey("KNOCK"); }
        }

        /// <summary>
        /// Gets whether or not the CPRIVMSG command exists, used for mass messaging people in specified channel
        /// </summary>
        public bool SupportsCPrivmsg
        {
            get { return _dict.ContainsKey("CPRIVMSG"); }
        }

        /// <summary>
        /// Gets whether or not the CNOTICE command exists, just like CPRIVMSG
        /// </summary>
        public bool SupportsCNotice
        {
            get { return _dict.ContainsKey("CNOTICE"); }
        }

        /// <summary>
        /// Gets whether or not the server supports sending nick!user@host mask in NAMES results
        /// </summary>
        public bool SupportsUHNames
        {
            get { return _dict.ContainsKey("UHNAMES"); }
        }

        /// <summary>
        /// Gets whether or not the server is capable of sending multiple prefixes in NAMES results
        /// </summary>
        public bool SupportsNamesX
        {
            get { return _dict.ContainsKey("NAMESX"); }
        }

        /// <summary>
        /// Gets whether or not the server supports the Hybrid Connect Notice protocol
        /// </summary>
        public bool SupportsHCN
        {
            get { return _dict.ContainsKey("HCN"); }
        }

        /// <summary>
        /// Gets whether or not the server supports server side ignores via the +g user mode.
        /// </summary>
        public bool CallerID
        {
            get { return _dict.ContainsKey("CALLERID"); }
        }

        /// <summary>
        /// Gets the maximum number entries in the list per mode.
        /// </summary>
        public IDictionary<char, int> MaxListSize
        {
            get { return _maxList ?? (_maxList = ParseDict(Get<string>("MAXLIST"))); }
        }

        /// <summary>
        /// Gets the maximum number of channels allowed to join by channel prefix.
        /// </summary>
        public IDictionary<char, int> ChannelLimit
        {
            get { return _chanLimit ?? (_chanLimit = ParseDict(Get<string>("CHANLIMIT"))); }
        }

        /// <summary>
        /// Gets the ID length for channels with an ID. The key is which channel type it is
        /// </summary>
        public IDictionary<char, int> IDChannelLength
        {
            get { return _idchan ?? (_idchan = ParseDict(Get<string>("IDCHAN"))); }
        }

        /// <summary>
        /// Gets whether or not forced nick changes are implemented: The server may change the nickname without the client sending a NICK message.
        /// </summary>
        public bool ForcedNickChanges
        {
            get { return _dict.ContainsKey("FNC"); }
        }

        /// <summary>
        /// Gets the server extentions for the LIST command. The tokens specify which extention are supported.
        /// </summary>
        /// <remarks>M = mask search, N = !mask search, U = usercount search (&gt; &lt;), C = creation time search (C&gt; C&lt;), T = topic search (T&gt; T&lt;)</remarks>
        public char[] ListExtensions
        {
            get
            {
                var val = Get<string>("ELIST");
                return val == null ? null : val.ToCharArray();
            }
        }

        /// <summary>
        /// Gets the maximum number of targets allowable for commands which accept multiple targets.
        /// </summary>
        public IDictionary<string, int> TargetMaximum
        {
            get { return _targmax ?? (_targmax = GetTargetMax(Get<string>("TARGMAX"))); }
        }

        /// <summary>
        /// Gets a list of channel modes: A = Mode that adds or removes a nick or address to a list. Always has a parameter. 
        /// </summary>
        public char[] ChanModesA
        {
            get
            {
                return GetChannelModes(0);
            }
        }

        /// <summary>
        /// Gets a list of channel modes: B = Mode that changes a setting and always has a parameter. 
        /// </summary>
        public char[] ChanModesB
        {
            get
            {
                return GetChannelModes(1);
            }
        }

        /// <summary>
        /// Gets a list of channel modes: C = Mode that changes a setting and only has a parameter when set. 
        /// </summary>
        public char[] ChanModesC
        {
            get
            {
                return GetChannelModes(2);
            }
        }

        /// <summary>
        /// Gets a list of channel modes: D = Mode that changes a setting and never has a parameter.
        /// </summary>
        public char[] ChanModesD
        {
            get
            {
                return GetChannelModes(3);
            }
        }

        /// <summary>
        /// Gets a series of commands that can be useful for the client to know exist as they may provide a more efficient means for the client to accomplish a specific task
        /// </summary>
        public string[] Commands
        {
            get
            {
                var cmds = Get<string>("CMDS");
                return cmds == null ? null : cmds.Split(',');
            }
        }
    }
}
