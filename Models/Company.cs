using System.Collections.Generic;


namespace AccountingApp.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OfficialId { get; set; }
        public List<AccountingYear> AccountingYears { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Journal> Journals { get; set; }
        public List<JournalEntry> Transactions { get; set; }
  
    }
}
