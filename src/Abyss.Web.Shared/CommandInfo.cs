using System;
using System.Collections.Generic;
using System.Text;

namespace Abyss.Web.Shared
{
    public class CommandInfo
    {
        public string[] Aliases { get; set; }
        public string Module { get; set; }
        public string[] Checks { get; set; }
        public string[] Parameters { get; set; }
        public bool Enabled { get; set; }
    }
}
