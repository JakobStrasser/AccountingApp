using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AccountingApp.Data;
using AccountingApp.Models;

namespace AccountingApp.Controllers
{
    public class AccountingYearsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountingYearsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AccountingYears
        public async Task<IActionResult> Index()
        {
            return View(await _context.AccountingYears.ToListAsync());
        }

        // GET: AccountingYears/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountingYear = await _context.AccountingYears
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accountingYear == null)
            {
                return NotFound();
            }

            return View(accountingYear);
        }

        // GET: AccountingYears/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AccountingYears/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,StartDate,EndDate,PreviousAccountingYearId,NextAccountingYearId")] AccountingYear accountingYear)
        {
            if (ModelState.IsValid)
            {
                _context.Add(accountingYear);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(accountingYear);
        }

        // GET: AccountingYears/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountingYear = await _context.AccountingYears.FindAsync(id);
            if (accountingYear == null)
            {
                return NotFound();
            }
            return View(accountingYear);
        }

        // POST: AccountingYears/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,StartDate,EndDate,PreviousAccountingYearId,NextAccountingYearId")] AccountingYear accountingYear)
        {
            if (id != accountingYear.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accountingYear);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountingYearExists(accountingYear.Id))
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
            return View(accountingYear);
        }

        // GET: AccountingYears/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountingYear = await _context.AccountingYears
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accountingYear == null)
            {
                return NotFound();
            }

            return View(accountingYear);
        }

        // POST: AccountingYears/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accountingYear = await _context.AccountingYears.FindAsync(id);
            _context.AccountingYears.Remove(accountingYear);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountingYearExists(int id)
        {
            return _context.AccountingYears.Any(e => e.Id == id);
        }
    }
}
