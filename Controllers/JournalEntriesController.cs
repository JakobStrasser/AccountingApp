using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.AspNetCore.Authorization;
using AccountingApp.ViewModels;
using Microsoft.AspNetCore.StaticFiles;
using System.Text;

namespace AccountingApp.Controllers
{
    [Authorize]
    public class JournalEntriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JournalEntriesController(ApplicationDbContext context)
        {
            _context = context;
        }
        //TODO: Limit to accounting year
        //TODO: Move companyId to session
        [Route("JournalEntries/Index/{companyId}")]
        // GET: JournalEntries
        public async Task<IActionResult> Index(int companyId)
        {
            ViewBag.companyId = companyId;
            List<JournalEntry> journalEntries = await _context.JournalEntry.Where(je => je.Company.Id == companyId).OrderBy(je => je.Date).ToListAsync();
            List<JournalEntriesListViewModel> journalEntriesListViewModels = new List<JournalEntriesListViewModel>();
            foreach (JournalEntry je in journalEntries)
            {
                JournalEntriesListViewModel currentJournalEntry = new JournalEntriesListViewModel { JournalEntry = je, CreditSum = 0, DebitSum = 0, Rows = new List<JournalEntriesListViewModelRow>() };
                List<LedgerEntry> currentRows = await _context.LedgerEntries.Where(le => le.JournalEntryId == je.Id).ToListAsync();
                foreach (LedgerEntry le in currentRows)
                {
                    double debit = (le.Value >= 0) ? Math.Abs(le.Value) : 0;
                    double credit = (le.Value >= 0) ? 0 : Math.Abs(le.Value);

                    currentJournalEntry.DebitSum += debit;
                    currentJournalEntry.CreditSum += credit;

                    Account account = await _context.Accounts.FindAsync(le.AccountId);


                    currentJournalEntry.Rows.Add(new JournalEntriesListViewModelRow { AccountName = account.Name, AccountNumber = account.AccountNumber, Debit = debit, Credit = credit });
                }
                journalEntriesListViewModels.Add(currentJournalEntry);
            }
            return View(journalEntriesListViewModels);
        }

        // GET: JournalEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var journalEntry = await _context.JournalEntry
                .FirstOrDefaultAsync(m => m.Id == id);
            if (journalEntry == null)
            {
                return NotFound();
            }

            JournalEntriesDetailViewModel journalEntriesDetailViewModel = new JournalEntriesDetailViewModel();
            journalEntriesDetailViewModel.JournalEntry = journalEntry;
            journalEntriesDetailViewModel.Rows = new List<JournalEntriesDetailViewModelRow>();
            await _context.Entry(journalEntry).Collection(je => je.Rows).LoadAsync();

            foreach (LedgerEntry le in journalEntry.Rows)
            {
                double debit = (le.Value >= 0) ? Math.Abs(le.Value) : 0;
                double credit = (le.Value >= 0) ? 0 : Math.Abs(le.Value);

                journalEntriesDetailViewModel.DebitSum += debit;
                journalEntriesDetailViewModel.CreditSum += credit;

                Account account = await _context.Accounts.FindAsync(le.AccountId);


                journalEntriesDetailViewModel.Rows.Add(new JournalEntriesDetailViewModelRow { AccountName = account.Name, AccountNumber = account.AccountNumber, Debit = debit, Credit = credit });
            }

            return View(journalEntriesDetailViewModel);
        }
        [Route("JournalEntries/Create/{CompanyId}")]
        public async Task<IActionResult> Create(int CompanyId)
        {
            Company company = await _context.Companies.FindAsync(CompanyId);

            //Build selectlist for accounting year
            List<AccountingYear> years = await _context.AccountingYears.Where(ay => ay.CompanyId == company.Id).OrderByDescending(ay => ay.StartDate).ToListAsync();
            List<SelectListItem> yearList = GetAccountingYearSelectList(company.Id);


            //Build dictionary for accounts
            List<Account> accounts = await _context.Accounts.Where(a => a.CompanyId == company.Id).OrderBy(a => a.AccountNumber).ToListAsync();
            Dictionary<int, string> accountDict = new Dictionary<int, string>();
            foreach (Account a in accounts)
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

            //Return view with model
            return View(new JournalEntryViewModel
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
        [Route("JournalEntries/Create/{CompanyId}")]
        public async Task<IActionResult> Create(int CompanyId, JournalEntryViewModel model)
        {
            //Load company, accountingyear, journal
            Company company = await _context.Companies.FindAsync(CompanyId);
            AccountingYear accountingYear = await _context.AccountingYears.FindAsync(int.Parse(model.SelectedAccountingYear));
            Journal journal = await _context.Journals.FindAsync(int.Parse(model.SelectedJournal));
            await _context.Entry(journal).Collection(j => j.JournalEntries).LoadAsync();

            //Create journalentry & ledgerentries            
            JournalEntry journalEntry = new JournalEntry { Company = company, AccountingYear = accountingYear, Date = model.SelectedDate, Active = true, Rows = new List<LedgerEntry>(), Name = model.Text };
            journal.JournalEntries.Add(journalEntry);

            //Loop over each row and add to JournalEntry
            for (int i = 0; i <= model.RowsUsed; i++)
            {
                int accountNumber = int.Parse(model.Rows[i].AccountNumber);
                Account account = await _context.Accounts.Where(a => a.CompanyId == company.Id && a.AccountNumber == accountNumber).FirstAsync();
                //Set account to used if not
                if (!account.Used)
                    account.Used = true;

                journalEntry.Rows.Add(new LedgerEntry { JournalEntryId = journal.Id, AccountId = account.Id, Value = (model.Rows[i].Debit - model.Rows[i].Credit), Active = true });
            }
            await _context.SaveChangesAsync();
            SubmitViewModel submitViewModel = new SubmitViewModel { JournalEntryId = journalEntry.Id, CompanyId = company.Id, Status = "Success!" };
            return RedirectToAction("Submit", submitViewModel);
        }

        [Route("JournalEntries/Submit")]
        public async Task<IActionResult> Submit(SubmitViewModel model)
        {
            return View(model);
        }

        //TODO: Fix loading LedgerEntry into rows
        [Route("JournalEntries/Edit/{JournalEntryId}")]
        public async Task<IActionResult> Edit(int JournalEntryId)
        {

            JournalEntry journalEntry = await _context.JournalEntry.Include(je => je.Company).FirstAsync(je => je.Id == JournalEntryId);
            Company company = journalEntry.Company;

            //Build selectlist for accounting year
            List<SelectListItem> yearList = GetAccountingYearSelectList(company.Id);
            AccountingYear startYear = await _context.AccountingYears.Where(ay => ay.Id.ToString() == yearList.First().Value).FirstAsync();
            List<AccountingYear> years = await _context.AccountingYears.Where(ay => ay.CompanyId == company.Id).OrderByDescending(ay => ay.StartDate).ToListAsync();

            //Build dictionary for accounts
            List<Account> accounts = await _context.Accounts.Where(a => a.CompanyId == company.Id).OrderBy(a => a.AccountNumber).ToListAsync();
            Dictionary<int, string> accountDict = new Dictionary<int, string>();
            foreach (Account a in accounts)
            {
                accountDict.Add(a.AccountNumber, a.Name);
            }

            //Build selectlist for journals
            List<Journal> Journals = await _context.Journals.Where(j => j.CompanyId == company.Id).ToListAsync();
            List<SelectListItem> journals = new List<SelectListItem>();
            foreach (Journal j in Journals)
            {
                journals.Add(new SelectListItem { Value = j.Id.ToString(), Text = j.Name });
            }

            JournalViewModelRow[] rows = new JournalViewModelRow[100];
            List<LedgerEntry> journalEntryRows = await _context.LedgerEntries.Where(le => le.JournalEntryId == JournalEntryId).ToListAsync();
            int rowIndex = 0;
            foreach (LedgerEntry le in journalEntryRows)
            {
                rows[rowIndex] = new JournalViewModelRow();
                rows[rowIndex].AccountNumber = le.Account.AccountNumber.ToString();
                rows[rowIndex].Debit = (le.Value >= 0) ? Math.Abs(le.Value) : 0;
                rows[rowIndex].Credit = (le.Value >= 0) ? 0 : Math.Abs(le.Value);
                rowIndex++;
            }

            int rowsUsed = rowIndex;
            for (; rowIndex < 100; rowIndex++)
            {
                rows[rowIndex] = new JournalViewModelRow { AccountNumber = "", Credit = 0, Debit = 0 };
            }

            //Return view with model
            return View(new JournalEntryViewModel
            {
                Company = company,
                Accounts = accountDict,
                SelectedAccountingYear = journalEntry.AccountingYear.Id.ToString(),
                SelectedAccountingYearStartDate = startYear.StartDate,
                SelectedAccountingYearEndDate = startYear.EndDate,
                SelectedDate = journalEntry.Date,
                AccountingYears = years,
                AccountingYearsList = yearList,
                JournalsList = journals,
                SelectedJournal = journalEntry.JournalId.ToString(),
                Rows = rows,
                Text = journalEntry.Name,
                RowsUsed = rowsUsed
            });

        }

        [HttpPost]
        [Route("JournalEntries/Edit/{JournalEntryId}")]
        public async Task<IActionResult> Edit(int JournalEntryId, JournalEntryViewModel model)
        {

            try
            {
                //Extra validation

                //Load company, accountingyear, journal
                JournalEntry journalEntry = await _context.JournalEntry.FindAsync(JournalEntryId);
                Company company = await _context.Companies.FindAsync(journalEntry.Company);
                AccountingYear accountingYear = await _context.AccountingYears.FindAsync(int.Parse(model.SelectedAccountingYear));
                journalEntry.AccountingYear = accountingYear;
                Journal journal = await _context.Journals.FindAsync(int.Parse(model.SelectedJournal));
                journalEntry.JournalId = journal.Id;
                journalEntry.Date = model.SelectedDate;


                for (int i = 0; i <= model.RowsUsed; i++)
                {
                    int accountNumber = int.Parse(model.Rows[i].AccountNumber);
                    Account account = await _context.Accounts.Where(a => a.CompanyId == company.Id && a.AccountNumber == accountNumber).FirstAsync();
                    journalEntry.Rows[i] = new LedgerEntry { JournalEntryId = journal.Id, AccountId = account.Id, Value = (model.Rows[i].Debit - model.Rows[i].Credit), Active = true };
                    //result =+ await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                SubmitViewModel submitViewModel = new SubmitViewModel { JournalEntryId = journalEntry.Id, Status = "Success!" };
                return View(submitViewModel);
            }
            catch (Exception ex)
            {

                SubmitViewModel submitViewModel = new SubmitViewModel { JournalEntryId = null, CompanyId = null, Status = "<p>" + ex.Message + "</p><p>" + ex.StackTrace + "</p>" };
                return View(submitViewModel);
            }
        }

        /// <summary>
        /// Returns view for Balance &  P&L reports per accountingyear
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [Route("JournalEntries/Report/{companyId}")]
        public async Task<IActionResult> Report(int companyId)
        {
            List<Account> accounts = await _context.Accounts.Where(a => a.CompanyId == companyId).OrderBy(a => a.AccountNumber).ToListAsync();
            List<AccountingYear> years = await _context.AccountingYears.Where(ay => ay.CompanyId == companyId).ToListAsync();
            ReportViewModel reportViewModel = new ReportViewModel { Years = years, Accounts = accounts, Reports = new Dictionary<AccountingYear, YearReport>() };

            foreach (AccountingYear year in years)
            {
                Dictionary<Account, List<LedgerEntry>> ledgerEntriesPerAccount = new Dictionary<Account, List<LedgerEntry>>();
                List<JournalEntry> journalEntriesInYear = await _context.JournalEntry.Where(je => je.AccountingYear == year).ToListAsync();
                foreach (JournalEntry journalEntry in journalEntriesInYear)
                {
                    List<LedgerEntry> ledgerEntriesToAdd = _context.LedgerEntries.Where(le => le.JournalEntryId == journalEntry.Id).ToList();
                    foreach (LedgerEntry ledgerEntry in ledgerEntriesToAdd)
                    {
                        if (ledgerEntriesPerAccount.ContainsKey(ledgerEntry.Account))
                            ledgerEntriesPerAccount[ledgerEntry.Account].Add(ledgerEntry);
                        else
                            ledgerEntriesPerAccount.Add(ledgerEntry.Account, new List<LedgerEntry> { ledgerEntry });
                    }
                }
                reportViewModel.Reports.Add(year, new YearReport { LedgerEntriesPerAccount = ledgerEntriesPerAccount });
            }


            return View(reportViewModel);
        }

        [Route("JournalEntries/Export/{companyId}")]
        public async Task<IActionResult> Export(int companyId)
        {

            List<SelectListItem> accountingYearsSelectListItems = GetAccountingYearSelectList(companyId);
          
            ExportViewModel exportViewModel = new ExportViewModel { CompanyId = companyId, AccountingYearsSelectListItems = accountingYearsSelectListItems };
            return View(exportViewModel);
        }


        [HttpPost]
        [Route("JournalEntries/Download")]
        public async Task<ActionResult> Download(ExportViewModel viewModel)
        {
            //Get year & company
            AccountingYear year = await _context.AccountingYears.FindAsync(int.Parse(viewModel.SelectedAccountingYear));
            Company company = await _context.Companies.FindAsync(viewModel.CompanyId);

            string xmlOutput = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?> <SieEntry xmlns = \"http://www.sie.se/sie5\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sie.se/sie5 http://www.sie.se/sie5.xsd\" >";

            xmlOutput += "<FileInfo ><SoftwareProduct name = \"AccountingApp\" verison = \"1.0\" />";
            xmlOutput += "<FileCreation time = \"" + DateTime.Now + "\" by = \"" + HttpContext.User.Identity.Name + "\" />";
            xmlOutput += "<Company organizationId = \"" + company.OfficialId + "\" name = \"" + company.Name + "\" />";
            xmlOutput += "<FiscalYears >";
            xmlOutput += "<FiscalYear start = \"" + year.StartDate.Year + "-" + year.StartDate.Month + "\" end = \"" + year.EndDate.Year + "-" + year.EndDate.Month + "\" primary=\"true\" />";
            xmlOutput += "</FiscalYears > <AccountingCurrency currency = \"SEK\" /> </FileInfo>";

            List<JournalEntry> journalEntries = await _context.JournalEntry.Include(je => je.Rows).Where(je => je.AccountingYear == year && je.Company == company).ToListAsync();
            foreach (JournalEntry journalEntry in journalEntries)
            {
                xmlOutput += "<JournalEntry journalDate=\"" + journalEntry.Date.ToShortDateString() + "\" text=\"" + journalEntry.Name + "\">";
                List<LedgerEntry> ledgerEntries = await _context.LedgerEntries.Include(le => le.Account).Where(le => le.JournalEntryId == journalEntry.Id).ToListAsync();
                foreach (LedgerEntry ledgerEntry in ledgerEntries)
                {
                    xmlOutput += "<LedgerEntry accountId=\"" + ledgerEntry.Account.AccountNumber + "\" amount=\"" + ledgerEntry.Value + "\"/>";
                }
                xmlOutput += "</JournalEntry>";
            }

            xmlOutput += "</SieEntry>";
            return File(Encoding.UTF8.GetBytes(xmlOutput), "application/xml", company.Name+"-export.xml");
        }

        private bool JournalEntryExists(int id)
        {
            return _context.JournalEntry.Any(e => e.Id == id);
        }

        private List<SelectListItem> GetAccountingYearSelectList(int companyId)
        {
            List<AccountingYear> years = _context.AccountingYears.Where(ay => ay.CompanyId == companyId).OrderBy(ay => ay.StartDate).ToList();
            List<SelectListItem> yearList = new List<SelectListItem>();
            foreach (AccountingYear y in years)
            {
                yearList.Add(new SelectListItem { Value = y.Id.ToString(), Text = y.StartDate.Year.ToString() });
            }
            return yearList;
        }
    }
}
