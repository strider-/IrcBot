using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBot.Bots.Model
{
    public class Record : NoSqlEntity
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Domain { get; set; }
        public string User { get; set; }
        public string Channel { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
