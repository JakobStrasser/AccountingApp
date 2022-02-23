using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class Journal
    {
        public int JournalId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}
