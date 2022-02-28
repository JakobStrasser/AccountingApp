using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OfficialId { get; set; }
        public List<AccountingYear> AccountingYears { get; set; }
        public List<AccountClass> AccountClasses { get; set; }
        public List<AccountGroup> AccountGroups { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Journal> Journals { get; set; }
        public List<JournalEntry> Transactions { get; set; }
        public List<Dimension> Dimensions { get; set; }
    }
}
