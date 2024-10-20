﻿using Microsoft.EntityFrameworkCore;
using Eventos.Common;
using Eventos.Models;

namespace Eventos.DataAccess
{
    /// <summary>
    /// SQLlite db context
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DbSet<User> User { get; set; }


        /// <summary>   |
        /// Initializes sqlite
        /// </summary>
        public DatabaseContext()
        {
            SQLitePCL.Batteries_V2.Init();
            this.Database.EnsureCreated();
        }


        /// <summary>
        /// Configure the database
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, Constants.DatabaseName);
            optionsBuilder.UseSqlite($"Filename={databasePath}");
        }
    }
}
