using ITTP_2023.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace ITTP_2023.DbContexts
{
    public class UserContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public UserContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(p => new { p.Login })
                .IsUnique(true);

            modelBuilder.Entity<User>()
                .HasIndex(p => new { p.Token })
                .IsUnique(true);
        }
    }
}
