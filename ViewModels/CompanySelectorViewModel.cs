using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingApp.Models;
using AccountingApp.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccountingApp.ViewModels
{
    public class CompanySelectorViewModel
    {
        public List<Company> Companies { get; set; }

        public Company SelectedCompany { get; set; }

    }
}
