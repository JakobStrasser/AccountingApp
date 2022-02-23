using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingApp.Models
{
    public class AccountingYear
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? PreviousAccountingYearId { get; set; }
        public int? NextAccountingYearId { get; set; }

    }
}
