using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.Models
{

    /// <summary>
    /// Dimension tracks the dimension reports are broken down by
    /// 1 Kostnadsställe/resultatenhet
    /// 2 Kostnadsbärare
    /// 3 Filial/butik
    /// 4 Verksamhetsgren
    /// 5 Reserverad för framtida utökning av standarden
    /// 6 Projekt
    /// 7 Anställd
    /// 18-19 Reserverade för framtida utökning av standarden
    /// 20- Fritt disponibla
    /// 
    /// </summary>
    public class Dimension
    {
        public int Id { get; set; }
        public int DimensionNumber { get; set; }
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public List<DimensionItem> DimensionItems { get; set; }
    }
}
