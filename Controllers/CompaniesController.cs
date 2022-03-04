using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AccountingApp.Data;
using AccountingApp.Models;
using AccountingApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace AccountingApp.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _environment;
        private readonly string rootPath;

        public CompaniesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            rootPath = environment.WebRootPath;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            string userName = HttpContext.User.Identity.Name;
            List<int> companies = await _context.UserCompanies.Where(uc => uc.UserName == userName).Select(c=>c.CompanyId).ToListAsync();
            return View(await _context.Companies.Where(c => companies.Contains(c.Id)).ToListAsync());
        }

        public async Task<IActionResult> CompanySelector()
        {          
            string userName = HttpContext.User.Identity.Name;
            List<int> companyIds = await _context.UserCompanies.Where(uc => uc.UserName == userName).Select(c => c.CompanyId).ToListAsync();
            List<Company> companies = await _context.Companies.Where(c => companyIds.Contains(c.Id)).ToListAsync();
            CompanySelectorViewModel viewModel = new CompanySelectorViewModel
            {
                Companies = companies
            };
            return View(viewModel);
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,OfficialId")] Company company, DateTime accountingYearStart, DateTime accountingYearEnd)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                SetupCompanyData(company, accountingYearStart, accountingYearEnd);
                await _context.SaveChangesAsync();
                _context.UserCompanies.Add(new UserCompany { CompanyId = company.Id, UserName = HttpContext.User.Identity.Name });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        /// <summary>
        /// Imports default accounts with grouping and classes, adds main journal and first accounting year
        /// </summary>
        /// <param name="company"></param>
        private void SetupCompanyData(Company company, DateTime start, DateTime end)
        {          
            AddAccounts(_context, company);
            _context.SaveChanges();
    
            _context.Journals.Add(new Journal { CompanyId = company.Id, Name = "Huvudbok", Active = true, JournalEntries = new List<JournalEntry>() });
            _context.AccountingYears.Add(new AccountingYear { CompanyId = company.Id, StartDate = start, EndDate = end });
            _context.SaveChanges();
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,OfficialId")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);

            //Delete journals, journalentry,  ledgerentry
            List<Journal> journals = await _context.Journals.Include(j => j.JournalEntries).Where(j => j.CompanyId == id).ToListAsync();
            foreach (Journal j in journals)
            {
                List<JournalEntry> journalEntries = await _context.JournalEntry.Where(je => je.JournalId == j.Id).ToListAsync();
                foreach (JournalEntry je in journalEntries)
                {
                    List<LedgerEntry> ledgerEntries = await _context.LedgerEntries.Where(le => le.JournalEntryId == je.Id).ToListAsync();
                    foreach (LedgerEntry le in ledgerEntries)
                        _context.LedgerEntries.Remove(le);
                    _context.JournalEntry.Remove(je);
                }
                _context.Journals.Remove(j);
                await _context.SaveChangesAsync();
            }
            //Accounts
            List<Account> accounts = await _context.Accounts.Where(a => a.CompanyId == id).ToListAsync();
            foreach (Account a in accounts)
            {
                _context.Accounts.Remove(a);
            }
            await _context.SaveChangesAsync();

            //Accounting years
            List<AccountingYear> accountingYears = await _context.AccountingYears.Where(ay => ay.CompanyId == id).ToListAsync();
            foreach (AccountingYear ay in accountingYears)
                _context.AccountingYears.Remove(ay);
            await _context.SaveChangesAsync();

            //User company link
            List<UserCompany> userCompanies = await _context.UserCompanies.Where(uc => uc.CompanyId == id).ToListAsync();
            foreach (UserCompany uc in userCompanies)
                _context.UserCompanies.Remove(uc);
             await _context.SaveChangesAsync();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }

        private  void AddAccounts(ApplicationDbContext dbContext, Company company)
        {
            string[] accountLines = System.IO.File.ReadAllLines(rootPath +"/csv/konton.csv");
         
            //Id;Name;AccountClassNumber;AccountClassName;AccountGroupNumber;AccountGroupName;Standard

            for (int i = 1; i < accountLines.Length; i++)
            {
                string[] split = accountLines[i].Split(";");

                int accountClassNumber = int.Parse(split[2]);
                string accountClassName = split[3];
                int accountGroupNumber = int.Parse(split[4]);
                string accountGroupName = split[5];


                int id = int.Parse(split[0]);
                string name = split[1];
                bool standard = bool.Parse(split[6]);

                dbContext.Accounts.Add(new Account { CompanyId = company.Id, AccountNumber = id, Name = name,AccountClassNumber = accountClassNumber, AccountClassName = accountClassName, AccountGroupNumber = accountGroupNumber,AccountGroupName = accountGroupName, Standard = standard, Used = false });
                Console.WriteLine("Added " + split);

            }
        }

       

       
    }
}
