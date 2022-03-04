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

        // GET: JournalEntries
        public async Task<IActionResult> Index()
        {
            List<JournalEntry> journalEntries = await _context.JournalEntry.Include(je => je.Rows).OrderBy(je => je.Date).ToListAsync();
            List<JournalEntriesListViewModel> journalEntriesListViewModels = new List<JournalEntriesListViewModel>();
            foreach(JournalEntry je in journalEntries)
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

        //// GET: JournalEntries/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: JournalEntries/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Date,Active,Name")] JournalEntry journalEntry)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(journalEntry);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(journalEntry);
        //}

        //// GET: JournalEntries/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var journalEntry = await _context.JournalEntry.FindAsync(id);
        //    if (journalEntry == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(journalEntry);
        //}

        //// POST: JournalEntries/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Active,Name")] JournalEntry journalEntry)
        //{
        //    if (id != journalEntry.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(journalEntry);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!JournalEntryExists(journalEntry.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(journalEntry);
        //}

        //// GET: JournalEntries/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var journalEntry = await _context.JournalEntry
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (journalEntry == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(journalEntry);
        //}

        //// POST: JournalEntries/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var journalEntry = await _context.JournalEntry.FindAsync(id);
        //    _context.JournalEntry.Remove(journalEntry);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool JournalEntryExists(int id)
        {
            return _context.JournalEntry.Any(e => e.Id == id);
        }
    }
}
