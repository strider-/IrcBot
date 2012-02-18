using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBot.Core.Bots;
using System.Xml.Linq;
using System.Timers;
using IrcBot.Core;
using IrcBot.Core.Helpers;
using System.Globalization;
using System.Threading;

namespace IrcBot.Bots
{
    public class GenoFeedBot : BaseBot
    {
        const string GENO_INITIAL_FEED = "https://api.twitter.com/1/statuses/user_timeline.xml?count=1&screen_name=geno";
        const string GENO_FEED = "https://api.twitter.com/1/statuses/user_timeline.xml?since_id={0}&screen_name=geno";

        string _since_id, _channel;
        System.Timers.Timer _timer;
        IrcContext _context;

        public GenoFeedBot()
        {
            Enabled = true;
            _timer = new System.Timers.Timer(1000 * 60 * 10);
            _timer.AutoReset = true;
            _timer.Elapsed += new ElapsedEventHandler(CheckDatFeed);
        }

        protected override void OnJoin(IrcContext context)
        {
            if(context.InvolvesBotClient)
            {
                _context = context;
                _channel = _context.Parameters.First();
                _timer.Start();
            }
        }

        protected override void OnPart(IrcContext context)
        {
            if(context.InvolvesBotClient)
            {
                _timer.Stop();
            }
        }

        private void CheckDatFeed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            PostDemTweets();
            _timer.Start();
        }

        private void PostDemTweets()
        {
            var feedUrl = string.IsNullOrEmpty(_since_id) ?
                          GENO_INITIAL_FEED :
                          string.Format(GENO_FEED, _since_id);

            XDocument feed = XDocument.Load(feedUrl);
            var updates = feed.Root.Elements("status").Select(e => new
            {
                Id = e.Element("id").Value,
                Text = e.Element("text").Value,
                Stamp = DateTime.ParseExact(e.Element("created_at").Value, "ddd MMM dd HH:mm:ss +ffff yyyy", CultureInfo.CurrentCulture).ToLocalTime()
            }).OrderBy(x => x.Id);

            if(updates.Any())
            {
                foreach(var tweet in updates)
                {
                    _context.Privmsg(_channel, tweet.Text);
                    Thread.Sleep(2000);
                }

                _since_id = updates.Last().Id;
            }
        }

        public override bool Enabled
        {
            get;
            set;
        }

        public override string Name
        {
            get { return "Geno Twitter Feed Bot"; }
        }
    }
}
