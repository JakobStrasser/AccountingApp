using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class LedgerEntry
    {
        public int Id { get; set; }

        public int JournalEntryId { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public double Value { get; set; }
        public bool Active { get; set; }
        public List<ObjectReference> ObjectReferences { get; set; }
    }
}
