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
using Microsoft.AspNetCore.Identity;

namespace AccountingApp.Controllers
{
    [Authorize]
    public class UserCompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserCompaniesController(ApplicationDbContext context)
        {
            
            _context = context;
        }

        // GET: UserCompanies
        public async Task<IActionResult> Index()
        {
            string userName = HttpContext.User.Identity.Name;
            return View(await _context.UserCompanies.Where(uc => uc.UserName == userName).ToListAsync());
        }

        // GET: UserCompanies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCompany = await _context.UserCompanies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userCompany == null)
            {
                return NotFound();
            }

            return View(userCompany);
        }
        [Authorize(Roles = "Admin")]
        // GET: UserCompanies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserCompanies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,UserId,CompanyId")] UserCompany userCompany)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userCompany);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userCompany);
        }

        // GET: UserCompanies/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCompany = await _context.UserCompanies.FindAsync(id);
            if (userCompany == null)
            {
                return NotFound();
            }
            return View(userCompany);
        }

        // POST: UserCompanies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CompanyId")] UserCompany userCompany)
        {
            if (id != userCompany.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userCompany);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserCompanyExists(userCompany.Id))
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
            return View(userCompany);
        }

        // GET: UserCompanies/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCompany = await _context.UserCompanies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userCompany == null)
            {
                return NotFound();
            }

            return View(userCompany);
        }

        // POST: UserCompanies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userCompany = await _context.UserCompanies.FindAsync(id);
            _context.UserCompanies.Remove(userCompany);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserCompanyExists(int id)
        {
            return _context.UserCompanies.Any(e => e.Id == id);
        }
    }
}
