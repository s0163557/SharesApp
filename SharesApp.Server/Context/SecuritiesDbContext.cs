using Microsoft.EntityFrameworkCore;
using Stock_Analysis_Web_App.Controllers;
using Stock_Analysis_Web_App.Models;
using Stock_Analysis_Web_App.Tools;

namespace Stock_Analysis_Web_App.Context
{
    public class SecuritiesDbContext : DbContext
    {
        public DbSet<SecurityInfo> SecurityInfos { get; set; }
        public DbSet<SecurityTradeRecord> SecurityTradeRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(UrlQueryMaker.GetDbConnectionUrl());
        }

        public bool FindSecurityInfoBySecurityId(string secId)
        {
            SecurityInfos.Include(u => u.SecurityId == secId);
            if (SecurityInfos.ToList().Count() > 0)
            {
                SecurityInfo? searchedSecurityInfo = SecurityInfos.Local.ToList().FirstOrDefault(security => security.SecurityId == secId, null);
                return searchedSecurityInfo == null ? false : true;
            }
            else return false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //modelBuilder.Entity<SecurityInfo>().HasIndex(u => u.Isin).IsUnique();

            modelBuilder.Entity<SecurityTradeRecord>()
                .HasOne(u => u.SecurityInfo)
                .WithMany(u => u.TradeRecords)
                .HasForeignKey("security_info_id");
        }
    }
}
