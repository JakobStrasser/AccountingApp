using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class ObjectReference
    {
        public int Id { get; set; }
        public int? LedgerEntryId { get; set; }
        public int? DimensionItemId { get; set; }
 
    }
}
