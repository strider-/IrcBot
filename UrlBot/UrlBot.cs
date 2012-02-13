using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using IrcBot.Core;
using IrcBot.Core.Bots;
using IrcBot.Core.Core.Helpers;
using System.Net;
using IrcBot.Bots.Model;
using IrcBot.Bots.Data;

namespace IrcBot.Bots
{
    public class UrlBot : BaseBot
    {
        const string REGEX_URL = @"(?<protocol>http(s)?|ftp)://(?<domain>[^/\r\n\:]+)(?(\:)(\:(?<port>\d{0,5})))(?<path>/[^\r\n\s]*)?";
        List<string> _nameBuffer;
        string[] _names;
        string _channel;
        private readonly ISession _repo;

        public UrlBot(string channel)
        {
            _repo = new RavenRepository("http://localhost:4380", "IRC_Data");
            _channel = channel;
            _nameBuffer = new List<string>();
            _names = new string[0];
            Enabled = true;
            AutoRejoin = true;
        }

        protected override void OnServerMessage(IrcContext context)
        {
            var code = context.GetReplyCode();
            
            if(code == ServerReplyCode.EndOfMessageOfTheDay)
            {
                context.Join(_channel);
            }
            else if(code == ServerReplyCode.NameReply)
            {
                _nameBuffer.AddRange(context.Parameters.Last().Split(' '));
            }
            else if(code == ServerReplyCode.EndOfNames)
            {
                _names = _nameBuffer.ToArray();
                _nameBuffer.Clear();
            }
        }

        protected override void OnPrivateMessage(IrcContext context)
        {
            if(context.IsBangCommand)
            {
                if(context.Parameters.Last().Equals("!url", StringComparison.InvariantCultureIgnoreCase) && Authorized(context))
                {
                    Enabled = !Enabled;
                    context.Privmsg(context.Parameters[0], string.Format("UrlBot: [\x02{0}\x02]", Enabled ? "Active" : "Inactive"));
                }

                if(context.Parameters.Last().Equals("!geno", StringComparison.InvariantCultureIgnoreCase))
                {
                    var rec = _repo.Random<Record>();
                    var tinyUrl = GetTinyUrl(rec.Url);
                    context.Privmsg(context.Parameters[0], string.Format("Here's your random link - {0}", tinyUrl));
                }
            }
            else if(Enabled)
            {
                var match = Regex.Match(context.Parameters.Last(), REGEX_URL);
                if(match.Success)
                {
                    var title = GetTitle(match.Value);

                    if(title != null)
                    {
                        var tinyUrl = GetTinyUrl(match.Value);
                        context.Privmsg(context.Parameters[0], string.Format("[\x02Title\x02] {0} ({1})", title, tinyUrl));
                        Log(title, match, context);
                    }
                }
            }
        }

        protected override void OnMode(IrcContext context)
        {
            context.Names(_channel);
        }

        private bool Authorized(IrcContext context)
        {
            var ops = _names.Where(n => n.StartsWith("@"));
            return ops.Any(o => o.Substring(1).Equals(context.Nickname, StringComparison.InvariantCultureIgnoreCase));
        }

        private string GetTinyUrl(string url)
        {
            return new WebClient().DownloadString("http://tinyurl.com/api-create.php?url=" + HtmlEntity.Entitize(url));
        }

        private string GetTitle(string url)
        {
            try
            {
                var req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.77 Safari/535.7";
                req.AllowAutoRedirect = true;
                var response = req.GetResponse();

                if(!response.ContentType.Contains("text/html"))
                {
                    return null;
                }

                var doc = new HtmlDocument();
                doc.Load(response.GetResponseStream());
                var titleMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='title']");
                string title = "N/A";

                if(titleMeta != null)
                {
                    title = titleMeta.Attributes["content"].Value;
                }
                else
                {
                    var node = doc.DocumentNode.SelectSingleNode("//title");
                    if(node != null)
                    {
                        title = node.InnerText.Trim();
                    }
                }

                return HtmlEntity.DeEntitize(title);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        private void Log(string title, Match match, IrcContext context)
        {
            var record = new Record
            {
                Title = title,
                Url = match.Value,
                Domain = match.Groups["domain"].Value,
                User = context.Nickname,
                Channel = context.Parameters[0],
                Timestamp = DateTime.Now
            };
            _repo.Add(record);
            _repo.CommitChanges();
        }

        public override string Name { get { return "UrlBot - Link title reporting"; } }

        public override bool Enabled { get; set; }
    }
}
