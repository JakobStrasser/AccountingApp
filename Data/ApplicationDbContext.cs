﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using AccountingApp.Models;

namespace AccountingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public DbSet<Company> Companies { get; set; }
        public DbSet<AccountingYear> AccountingYears { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountGroup> AccountGroups { get; set; }
        public DbSet<AccountClass> AccountClasses { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<JournalEntry> JournalEntry { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<ObjectReference> ObjectReferences { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<DimensionItem> DimensionItems { get; set; }
        public DbSet<UserCompany> UserCompanies { get; set; }
     
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
