using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    /// <summary>
    /// Tracks each Item belonging to a Dimension and if it's active or not.
    /// </summary>
    public class DimensionItem
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int DimensionId { get; set; }
        public Dimension Dimension { get; set; }

        public List<ObjectReference> TransactionRowDimensions { get; set; }
    }
}
