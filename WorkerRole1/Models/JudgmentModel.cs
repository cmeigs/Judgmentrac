using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerRole1.Models
{
    class Judgment
    {
        public string JudgmentName { get; set; }
        public int Principal { get; set; }
        public int Rate { get; set; }
        public DateTime EndDate { get; set; }
        public string UserName { get; set; }
    }
}
