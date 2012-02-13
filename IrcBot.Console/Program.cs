using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBot.Core;
using IrcBot.Bots;
using System.Net;

namespace IrcBot.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: make channel joining at the client level
            // TODO: flesh out CtcpBot

            // var s = new { Server = "irc.synirc.net", Port = 6667, SSL = false };
            var s = new { Server = "irc.choopa.net", Port = 9999, SSL = true };

            BotClient client = new BotClient(s.Server, s.Port, s.SSL);
            client.SetIdentity("geno-", "n3rd", "Robot Strider");
            AddBots(client);

            client.Connect();

            StringBuilder sb = new StringBuilder();
            ConsoleKeyInfo key;
            while((key = System.Console.ReadKey()).Key != ConsoleKey.Escape)
            {
                switch(key.Key)
                {
                    case ConsoleKey.Enter:
                        client.Write(sb.ToString());
                        sb.Clear();
                        System.Console.WriteLine();
                        break;
                    case ConsoleKey.Backspace:
                        if(sb.Length > 0)
                            sb.Length--;
                        break;
                    default:
                        sb.Append(key.KeyChar);
                        break;
                }
            }

            client.Close();
        }

        static void AddBots(BotClient host)
        {
            UrlBot urlBot = new UrlBot("#changoland");
            host.AddBot(urlBot);

            TweetBot tBot = new TweetBot();
            host.AddBot(tBot);
        }
    }
}
