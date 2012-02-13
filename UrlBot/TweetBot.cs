using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBot.Core.Bots;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.XPath;
using System.Xml.Linq;

namespace IrcBot.Bots
{
    public class TweetBot : BaseBot
    {
        const string REGEX_URL = @"(?<protocol>http(s)?)://twitter.com/(?<path>[^\r\n]*/status)/(?<id>\d*)";
        const string API_URL = @"https://api.twitter.com/1/statuses/show.xml?id={0}";

        public TweetBot()
        {
            Enabled = true;
        }

        protected override void OnPrivateMessage(Core.IrcContext context)
        {
            if(Enabled)
            {
                var match = Regex.Match(context.Parameters.Last(), REGEX_URL);
                if(match.Success)
                {
                    try
                    {
                        string id = match.Groups["id"].Value;
                        var doc = XDocument.Load(string.Format(API_URL, id));
                        var screenName = doc.XPathSelectElement("//user/screen_name").Value;
                        var realName = doc.XPathSelectElement("//user/name").Value;
                        var tweet = doc.XPathSelectElement("//text").Value;

                        context.Privmsg(context.Parameters.First(), string.Format("[\x02Tweet\x02] @{0} ({1}) - {2}", screenName, realName, tweet));
                    }
                    catch
                    {
                        context.Privmsg(context.Parameters.First(), "couldn't load that tweet, sorry brah");
                    }
                }
            }
        }
        public override string Name
        {
            get { return "Tweet Bot"; }
        }

        public override bool Enabled
        {
            get;
            set;
        }
    }
}
