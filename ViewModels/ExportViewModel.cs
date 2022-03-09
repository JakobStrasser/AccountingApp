using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccountingApp.ViewModels
{
    public class ExportViewModel
    {
        public int CompanyId { get; set; }
        public List<SelectListItem> AccountingYearsSelectListItems { get; set; }
        public string SelectedAccountingYear { get; set; }
       
    }
}
