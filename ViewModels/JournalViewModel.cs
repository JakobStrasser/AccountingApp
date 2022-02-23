using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.ViewModels
{
    public class JournalViewModel
    {
        public Company Company { get; set; }
        public Dictionary<int, string> Accounts { get; set; }
        public List<AccountingYear> AccountingYears { get; set; }
        public List<Dimension> Dimensions { get; set; }
       
       
    }
}
