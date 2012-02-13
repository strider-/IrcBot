using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBot.Bots.Model
{
    public abstract class NoSqlEntity
    {
        public string Id { get; set; }
    }
}
