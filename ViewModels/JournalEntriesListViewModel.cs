using AccountingApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.ViewModels
{
    public class JournalEntriesListViewModel
    {

        public JournalEntry JournalEntry { get; set; }
        public List<JournalEntriesListViewModelRow> Rows { get; set; }
        List<AccountingYear> accountingYears { get; set; }
        List<SelectListItem> accountingYearsSelectList { get; set; }
        AccountingYear selectedAccountingYear { get;set; }

        public double DebitSum { get; set; }
        public double CreditSum { get; set; }


    }

    public class JournalEntriesListViewModelRow
    {
        public int AccountNumber { get; set; }
        public string AccountName { get; set; }
        public double Credit { get; set; }
        public double Debit { get; set; }

    }
}
