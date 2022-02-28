using AccountingApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public List<SelectListItem> AccountingYearsList { get; set; }
        public string SelectedAccountingYear { get; set; }
        public DateTime SelectedAccountingYearStartDate { get; set; }
        public DateTime SelectedAccountingYearEndDate { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<SelectListItem> JournalsList { get; set; }
        public string SelectedJournal { get; set; }
        public JournalViewModelRow[] Rows { get; set; }
        public int RowsUsed { get; set; }
        public string Text { get; set; }

    }

    public class JournalViewModelRow
    {
        public string AccountNumber { get; set; }
        public double Credit { get; set; }
        public double Debit { get; set; }

    }
}
