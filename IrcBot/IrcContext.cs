using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBot.Core.Core.Helpers;

namespace IrcBot.Core
{
    /// <summary>
    /// Encapsulates information about an IRC message, as well as providing a means to respond.
    /// </summary>
    public class IrcContext
    {
        private readonly Connection _conn;
        private readonly Message _msg;
        private string _nickname, _hostname, _username;
        private ServerInfo _info;

        internal IrcContext(Message message, Connection connection, ServerInfo info)
        {
            _conn = connection;
            _info = info;
            _msg = message;
            Init();
        }

        private void Init()
        {
            bool privateMessage = _msg.Command.Equals("PRIVMSG", StringComparison.InvariantCultureIgnoreCase);
            IsServerMessage = !_msg.Prefix.Any(c => c == '!' || c == '@');
            InvolvesBotClient = _msg.Parameters.Any(p => p.Equals(_conn.Host.NickName));

            IsBangCommand =  privateMessage && _msg.Parameters.Last().StartsWith("!");

            Mentioned = privateMessage && _msg.Parameters.Last().IndexOf(_conn.Host.NickName, StringComparison.InvariantCultureIgnoreCase) >= 0;

            if(!IsServerMessage)
            {
                string[] usr = _msg.Prefix.Split(new char[] { '!', '@' });
                _nickname = usr[0];
                _username = usr[1];
                _hostname = usr[2];
            }
        }

        /// <summary>
        /// Parses the server reply code & returns an enum value.  If the command isn't from the server, returns ServerReplyCode.NotAServerReply
        /// </summary>
        /// <returns></returns>
        public ServerReplyCode GetReplyCode()
        {
            if(!IsServerMessage)
            {
                return ServerReplyCode.NotAServerReply;
            }

            int code;
            if(!int.TryParse(_msg.Command, out code))
            {
                return ServerReplyCode.Unknown;
            }

            var result = (ServerReplyCode)code;
            return result;
        }
        /// <summary>
        ///  Privmsg is used to send private messages between users, as well as to send messages to channels.
        /// </summary>
        /// <param name="recipient">The nick name of the user, or the name of the channel to send the message to</param>
        /// <param name="message">The message to send</param>
        public void Privmsg(string recipient, string message)
        {
            _conn.Write("PRIVMSG {0} :{1}", recipient, message);
        }
        /// <summary>
        /// The Join command is used by a user to request to start listening to the specific channel.
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void Join(string channel)
        {
            _conn.Write("JOIN {0}", channel);
        }
        /// <summary>
        /// The Part command causes the user sending the message to be removed from the list of active members for the channel.
        /// </summary>
        /// <param name="channel">Channel to leave</param>
        public void Part(string channel)
        {
            _conn.Write("PART {0}", channel);
        }
        /// <summary>
        /// The Mode command is provided so that users can change the characteristics of a channel.
        /// </summary>
        /// <param name="channel">Channel to affect</param>
        /// <param name="modes">The modes to set (i.e "-o" or "+v")</param>
        /// <param name="modeParams">The mode parameters (nicknames, hostmasks, ect)</param>
        public void Mode(string channel, string modes, string modeParams)
        {
            _conn.Write("MODE {0} {1} {2}", channel, modes, modeParams);
        }
        /// <summary>
        /// The Topic command is used to change the topic of a channel.
        /// </summary>
        /// <param name="channel">Channel to set the topic for</param>
        /// <param name="newTopic">The new topic to set</param>
        public void Topic(string channel, string newTopic)
        {
            _conn.Write("TOPIC {0} :{1}", channel, newTopic);
        }
        /// <summary>
        /// Names gets a list of all users currently visible in the channel
        /// </summary>
        /// <param name="channel">Channel to get a user list for</param>
        public void Names(string channel)
        {
            _conn.Write("NAMES {0}", channel);
        }
        /// <summary>
        /// Kick can be used to request the forced removal of a user from a channel.
        /// </summary>
        /// <param name="channel">Channel to remove the user from</param>
        /// <param name="nickname">Nick name of the user to remove</param>
        /// <param name="reason">An optional reason for the removal</param>
        public void Kick(string channel, string nickname, string reason = null)
        {
            if(string.IsNullOrWhiteSpace(reason))
            {
                _conn.Write("KICK {0} {1}", channel, nickname);
            }
            else
            {
                _conn.Write("KICK {0} {1} :{2}", channel, nickname, reason);
            }
        }
        /// <summary>
        /// Who is used by a client to generate a query which returns a list of information which matches the mask parameter
        /// </summary>
        /// <param name="mask">Mask to match</param>
        public void Who(string mask)
        {
            _conn.Write("WHO {0}", mask);
        }
        /// <summary>
        /// WhoIs is used to query information about particular user.
        /// </summary>
        /// <param name="nickname">Nick name to query for</param>
        public void WhoIs(string nickname)
        {
            _conn.Write("WHOIS {0}", nickname);
        }
        /// <summary>
        /// WhoWas asks for information about a nickname which no longer exists.
        /// </summary>
        /// <param name="nickname">Nick name to get a history for</param>
        /// <param name="count">Maximum history length</param>
        public void WhoWas(string nickname, int? count = null)
        {
            if(count == null)
            {
                _conn.Write("WHOWAS {0}", nickname);
            }
            else
            {
                _conn.Write("WHOWAS {0} {1}", nickname, count.Value);
            }
        }
        /// <summary>
        /// Enables the specified extended command, should the server support it
        /// </summary>
        /// <param name="command">Command to enable, i.e. HCN, NAMESX, ect</param>
        public void Protocol(string command)
        {
            _conn.Write("PROTOCTL {0}");
        }
        /// <summary>
        /// Writes a raw IRC command to the server, use at your own risk
        /// </summary>
        /// <param name="raw">The format of the command</param>
        /// <param name="args">Any arguments for the formatted command, if any</param>
        public void Write(string raw, params object[] args)
        {
            _conn.Write(raw, args);
        }
        
        /// <summary>
        /// Gets the supported protocol extensions of the connected server
        /// </summary>
        public ServerInfo ServerSupport
        {
            get { return _info; }
        }
        /// <summary>
        /// Gets whether or not the context has a valid connection
        /// </summary>
        public bool Active
        {
            get
            {
                return _conn.IsConnected;
            }
        }
        /// <summary>
        /// Gets whether or not the message starts with a bang ('!' character)
        /// </summary>
        public bool IsBangCommand
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets whether or not the origin of the message was the server
        /// </summary>
        public bool IsServerMessage
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the nick name of the user who sent the message, if the origin of the message was a user
        /// </summary>
        public string Nickname
        {
            get { return _nickname; }
        }
        /// <summary>
        /// Gets the host name of the user who sent the message, if the origin of the message was a user
        /// </summary>
        public string Hostname
        {
            get { return _hostname; }
        }
        /// <summary>
        /// Gets the user name of the user who sent the message, if the origin of the message was a user
        /// </summary>
        public string Username
        {
            get { return _username; }
        }
        /// <summary>
        /// Gets the origin of the message; either the server or a user
        /// </summary>
        public string Prefix
        {
            get
            {
                return _msg.Prefix;
            }
        }
        /// <summary>
        /// Gets the message command
        /// </summary>
        public string Command
        {
            get
            {
                return _msg.Command;
            }
        }
        /// <summary>
        /// Gets the parameters of the message, if any
        /// </summary>
        public string[] Parameters
        {
            get { return _msg.Parameters; }
        }
        /// <summary>
        /// Gets whether or not the command effects the bot client.
        /// </summary>
        public bool InvolvesBotClient { get; private set; }
        /// <summary>
        /// Gets whether or not the nick name of the bot host was mentioned in the message.
        /// </summary>
        public bool Mentioned { get; private set; }
    }
}
