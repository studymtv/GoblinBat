﻿using System.Data.Entity;
using ShareInvest.Models;

namespace ShareInvest.GoblinBatContext
{
    public class GoblinBatDbContext : DbContext
    {
        public GoblinBatDbContext() : base(new Secret().ConnectionString)
        {

        }
        public override int SaveChanges()
        {
            return this.BatchSaveChanges();
        }
        public DbSet<Codes> Codes
        {
            get; set;
        }
        public DbSet<Futures> Futures
        {
            get; set;
        }
        public DbSet<Options> Options
        {
            get; set;
        }
        public DbSet<Stocks> Stocks
        {
            get; set;
        }
        public DbSet<Days> Days
        {
            get; set;
        }
        public DbSet<Logs> Logs
        {
            get; set;
        }
    }
}