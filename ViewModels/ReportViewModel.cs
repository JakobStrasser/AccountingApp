using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingApp.Models;

namespace AccountingApp.ViewModels
{
    public class ReportViewModel
    {

        public List<AccountingYear> Years { get; set; }
      
        public List<Account> Accounts { get; set; }
       
        //Key - accountingyearid, value - YearReport
        public Dictionary<AccountingYear, YearReport> Reports { get; set; }
       
    }

    public class YearReport
    {
        //key - account number, value  - sum      
       public Dictionary<Account, List<LedgerEntry>> LedgerEntriesPerAccount { get; set; }
    }
}
