using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public int JournalId { get; set; }
        public Company Company { get; set; }
        public AccountingYear AccountingYear { get; set; }
        public DateTime Date { get; set; }
        public List<LedgerEntry> Rows { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
    }
}
