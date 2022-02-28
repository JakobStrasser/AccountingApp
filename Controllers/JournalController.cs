using AccountingApp.Data;
using AccountingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountingApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
           
            //Build selectlist for accounting year
            List<AccountingYear> years = await _context.AccountingYears.Where(ay => ay.CompanyId == company.Id).OrderBy(ay => ay.StartDate).ToListAsync();
            List<SelectListItem> yearList = new List<SelectListItem>();
            foreach(AccountingYear y in years)
            {
                yearList.Add(new SelectListItem { Value = y.Id.ToString(), Text = y.StartDate.Year.ToString() });
            }

            //Build dictionary for accounts
            List<Account> accounts = await _context.Accounts.Where(a => a.CompanyId == company.Id).OrderBy(a => a.AccountNumber).ToListAsync();            
            Dictionary<int, string> accountDict = new Dictionary<int, string>();
            foreach(Account a in accounts)
            {
                accountDict.Add(a.AccountNumber, a.Name);
            }
            //Build selectlist for journals
            List<Journal> Journals = await _context.Journals.Where(j => j.CompanyId == CompanyId).ToListAsync();
            List<SelectListItem> journals = new List<SelectListItem>();
            foreach (Journal j in Journals)
            {
                journals.Add(new SelectListItem { Value = j.Id.ToString(), Text = j.Name });
            }

          

            return View(new JournalViewModel
            {
                Company = company,
                Accounts = accountDict,
                SelectedAccountingYear = years.First().Id.ToString(),
                SelectedAccountingYearStartDate = years.First().StartDate,
                SelectedAccountingYearEndDate = years.First().EndDate,
                AccountingYears = years,
                AccountingYearsList = yearList,
                JournalsList = journals,
                Rows = new JournalViewModelRow[100]  //Prepare a list to hold rows
            });
           
        }
        [HttpPost]
        [Route("Journal/{CompanyId}")]
        public async Task<IActionResult> Submit(int CompanyId, JournalViewModel model)
        {

            try
            {
                //Extra validation
               
                //Load company, accountingyear, journal
                Company company = await _context.Companies.FindAsync(CompanyId);
                AccountingYear accountingYear = await _context.AccountingYears.FindAsync(int.Parse(model.SelectedAccountingYear));
                Journal journal = await _context.Journals.FindAsync(int.Parse(model.SelectedJournal));
                await _context.Entry(journal).Collection(j => j.JournalEntries).LoadAsync();

                //Create journalentry & ledgerentries
            
                JournalEntry journalEntry = new JournalEntry { Company = company, AccountingYear = accountingYear, Date = model.SelectedDate, Active = true, Rows = new List<LedgerEntry>(),Name = model.Text };         
                //var result =+ await _context.SaveChangesAsync();
 
                journal.JournalEntries.Add(journalEntry);
                //result = +await _context.SaveChangesAsync();

                for (int i = 0; i <= model.RowsUsed; i++)
                {
                    int accountNumber = int.Parse(model.Rows[i].AccountNumber);
                    Account account = await _context.Accounts.Where(a => a.CompanyId == company.Id && a.AccountNumber == accountNumber).FirstAsync();
                    journalEntry.Rows.Add(new LedgerEntry { JournalEntryId = journal.Id, AccountId = account.Id, Value = (model.Rows[i].Debit - model.Rows[i].Credit), Active = true });
                    //result =+ await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                SubmitViewModel submitViewModel = new SubmitViewModel { JournalEntryId = journalEntry.Id, Status = "Success!" };
                return View(submitViewModel);
            }
            catch (Exception ex)
            {

                SubmitViewModel submitViewModel = new SubmitViewModel { JournalEntryId = null, Status = "<p>" + ex.Message + "</p><p>" + ex.StackTrace + "</p>"  };
                return View(submitViewModel);
            }
           

        }
    }
}
