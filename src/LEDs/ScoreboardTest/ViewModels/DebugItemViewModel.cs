using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreboardTest.ViewModels
{
    public class DebugItemViewModel
    {
        public DateTime DateTime { get; set; }
        public string Category { get; set; }
        public string Info { get; set; }

        public DebugItemViewModel(DateTime dt, string category, string info)
        {
            DateTime = dt;
            Category = category;
            Info = info;
        }
    }
}
