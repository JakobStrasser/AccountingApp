using AccountingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.ViewModels
{
    public class CompanyDetailsViewModel
    {
        public Company Company { get; set; }
        public List<AccountingYear> AccountingYears { get; set; }
        public List<Journal> Journals { get; set; }
    }
}
