using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace IrcBot.Core
{
    internal class Connection : IDisposable
    {
        const int MAX_MESSAGE_LENGTH = 512;

        private static object _padLock = new object();
        TcpClient _client;
        Stream _stream;
        Action<Message> _messageReceived;
        string _partialMessage;

        internal Connection(BotClient host, Action<Message> onMessageReceived)
        {
            Host = host;
            _client = new TcpClient();
            _client.Connect(host.Server, host.Port);
            _stream = _client.GetStream();

            if(host.UsingSSL)
            {
                SslStream sslStream = new SslStream(_stream, false, ValidateCertificate);
                sslStream.AuthenticateAsClient(host.Server);
                _stream = sslStream;
            }

            _messageReceived = onMessageReceived;
            Encoding = Encoding.UTF8;
            BeginRead();
        }

        public void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }
        public void Write(string data)
        {
            if(string.IsNullOrWhiteSpace(data))
            {
                return;
            }

            if(IsConnected)
            {
                if(!data.EndsWith("\r\n"))
                    data += "\r\n";

                if(data.Length > MAX_MESSAGE_LENGTH)
                {
                    data = data.Substring(0, MAX_MESSAGE_LENGTH - 2).Trim() + "\r\n";
                }

                var raw = Encoding.GetBytes(data);
                _stream.Write(raw, 0, raw.Length);
            }
        }
        public void Disconnect(string quitMessage = "")
        {
            if(IsConnected)
            {
                Write("QUIT :" + quitMessage);
                _client.Close();
            }
        }
        public void Dispose()
        {
            Disconnect();
            _stream.Dispose();
        }

        private void BeginRead()
        {
            byte[] buffer = new byte[0x8000];
            _stream.BeginRead(buffer, 0, buffer.Length, OnData, buffer);
        }
        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        
        private void OnData(IAsyncResult result)
        {
            lock(_padLock)
            {
                if(IsConnected)
                {
                    var buffer = result.AsyncState as byte[];
                    int read = _stream.EndRead(result);
                    BeginRead();

                    var rawMessage = this.Encoding.GetString(buffer, 0, read);

                    if(_partialMessage != null)
                    {
                        rawMessage = _partialMessage + rawMessage;
                        _partialMessage = null;
                    }

                    if(!rawMessage.EndsWith("\n"))
                    {
                        int idx = rawMessage.LastIndexOf('\n');
                        _partialMessage = rawMessage.Substring(idx + 1);
                        rawMessage = rawMessage.Substring(0, rawMessage.Length - _partialMessage.Length);
                    }

                    var msgs = rawMessage.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach(var msg in msgs)
                    {
                        string finalMsg = msg.StartsWith(":") ?
                                          msg :
                                          string.Format(":{0} {1}", Host.Server, msg);

                        _messageReceived(new Message(finalMsg));
                    }
                }
            }
        }

        public BotClient Host { get; private set; }
        public bool IsConnected { get { return _client.Connected; } }
        public Encoding Encoding { get; private set; }
    }
}
