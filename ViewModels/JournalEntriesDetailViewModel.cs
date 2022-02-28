using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.ViewModels
{
    public class JournalEntriesDetailViewModel
    {

        public JournalEntry JournalEntry { get; set; }
        public List<JournalEntriesDetailViewModelRow> Rows { get; set; }

        public double DebitSum { get; set; }
        public double CreditSum { get; set; }
    }

    public class JournalEntriesDetailViewModelRow
    {
        public int AccountNumber { get; set; }
        public string AccountName { get; set; }
        public double Credit { get; set; }
        public double Debit { get; set; }

    }
}
