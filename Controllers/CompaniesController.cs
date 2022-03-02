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
            AddAccountClasses(_context, company);
            _context.SaveChanges();

            AddAccountGroups(_context,  company);
            _context.SaveChanges();

            //AddDimensions(_context);
            //await _context.SaveChangesAsync();

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
                foreach (JournalEntry je in j.JournalEntries)
                {
                    foreach (LedgerEntry le in je.Rows)
                        _context.LedgerEntries.Remove(le);
                    _context.JournalEntry.Remove(je);
                }
                _context.Journals.Remove(j);
            }
            //Account classes, groups, accounts
            List<AccountClass> accountClasses = await _context.AccountClasses.Include(ac => ac.AccountGroups).Where(ac => ac.CompanyId == id).ToListAsync();
            foreach (AccountClass ac in accountClasses)
            {
                foreach(AccountGroup ag in ac.AccountGroups)
                {
                    List<Account> accounts = await _context.Accounts.Where(a => a.AccountGroupId == ag.Id && a.CompanyId == id).ToListAsync();
                    foreach (Account a in accounts)
                    {
                        _context.Accounts.Remove(a);
                    }
                    _context.AccountGroups.Remove(ag);
                }
                _context.AccountClasses.Remove(ac);
            }

            //Accounting year
            List<AccountingYear> accountingYears = await _context.AccountingYears.Where(ay => ay.CompanyId == id).ToListAsync();
            foreach (AccountingYear ay in accountingYears)
                _context.AccountingYears.Remove(ay);

            //User company link
            List<UserCompany> userCompanies = await _context.UserCompanies.Where(uc => uc.CompanyId == id).ToListAsync();
            foreach (UserCompany uc in userCompanies)
                _context.UserCompanies.Remove(uc);

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }

        private  void AddDimensions(ApplicationDbContext dbContext)
        {
            string[] accountLines = System.IO.File.ReadAllLines(rootPath + "/csv/dimension.csv");
            for (int i = 1; i < accountLines.Length; i++)
            {
                string[] split = accountLines[i].Split(";");


                int id = int.Parse(split[0]);
                string dimensionName = split[1];

                dbContext.Dimensions.Add(new Dimension { CompanyId = 1, DimensionNumber = id, Name = dimensionName, Active = false });
                Console.WriteLine("Added " + split[0] + " - " + split[1]);

            }

        }



        private  void AddAccounts(ApplicationDbContext dbContext, Company company)
        {
            string[] accountLines = System.IO.File.ReadAllLines(rootPath +"/csv/konton.csv");

            var accountGroups = dbContext.AccountGroups.ToList();


            for (int i = 1; i < accountLines.Length; i++)
            {
                string[] split = accountLines[i].Split(";");

                int accountGroupId = int.Parse(split[1]);
                int accountGroup = accountGroups.Find(ag => ag.CompanyId == company.Id && ag.AccountGroupNumber == accountGroupId).Id;

                int categoryId = int.Parse(split[2]);

                int id = int.Parse(split[4]);
                string name = split[5];
                bool standard = bool.Parse(split[6]);

                dbContext.Accounts.Add(new Account { CompanyId = company.Id, AccountNumber = id, AccountGroupId = accountGroup, Name = name, Standard = standard, Used = false });
                Console.WriteLine("Added " + split);

            }
        }

        private  void AddAccountGroups(ApplicationDbContext dbContext, Company company)
        {
            string[] accountGroupLines = System.IO.File.ReadAllLines(rootPath + "/csv/kontogrupp.csv");

            List<AccountClass> accountClasses = dbContext.AccountClasses.ToList();

            for (int i = 1; i < accountGroupLines.Length; i++)
            {
                string[] split = accountGroupLines[i].Split(";");

                var accountClassId = accountClasses.Find(ac => ac.AccountClassNumber == int.Parse(split[0])).Id;

                dbContext.AccountGroups.Add(new AccountGroup { CompanyId = company.Id, AccountGroupNumber = int.Parse(split[1]), Name = split[2], AccountClassId = accountClassId });

                Console.WriteLine("Added " + split[0] + " - " + split[1] + " - " + split[2]);

            }
        }

        private  void AddAccountClasses(ApplicationDbContext dbContext, Company company)
        {
            string[] accountClassLines = System.IO.File.ReadAllLines(rootPath + "/csv/kontoklass.csv");

            for (int i = 1; i < accountClassLines.Length; i++)
            {
                string[] split = accountClassLines[i].Split(";");

                if (int.TryParse(split[0], out int id))
                    dbContext.AccountClasses.Add(new AccountClass { CompanyId = company.Id, AccountClassNumber = id, Name = split[1] });
                dbContext.SaveChanges();
                Console.WriteLine("Added " + split[0] + " - " + split[1]);

            }
        }
    }
}
