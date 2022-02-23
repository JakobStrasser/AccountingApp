using AccountingApp.Data;
using AccountingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingApp.Models;

namespace AccountingApp.Controllers
{
    public class JournalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JournalController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("Journal/{CompanyId}")]
        public async Task<IActionResult> Index(int CompanyId)
        {
            Company company = await _context.Companies.FindAsync(CompanyId);
            List<AccountingYear> years = await _context.AccountingYears.Where(ay => ay.CompanyId == company.Id).OrderBy(ay => ay.StartDate).ToListAsync();
            List<Account> accounts = await _context.Accounts.Where(a => a.CompanyId == company.Id).OrderBy(a => a.AccountNumber).ToListAsync();
            Dictionary<int, string> accountDict = new Dictionary<int, string>();
            foreach(Account a in accounts)
            {
                accountDict.Add(a.AccountNumber, a.Name);
            }

            List<Dimension> dimensions = await _context.Dimensions.Where(d => d.CompanyId == company.Id && d.Active).OrderBy(d => d.DimensionNumber).ToListAsync();

            //TODO: Calculate sum per account and date

            return View(new JournalViewModel { Company = company, Accounts = accountDict, AccountingYears = years, Dimensions = dimensions });
           
        }
    }
}
