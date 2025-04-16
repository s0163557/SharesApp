using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SharesApp.Server.Models;

namespace SharesApp.Server.Context;

public partial class SecuritiesContext : DbContext
{
    public SecuritiesContext()
    {
    }
    
    public SecuritiesContext(DbContextOptions<SecuritiesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SecurityInfo> SecurityInfos { get; set; }

    public virtual DbSet<SecurityTradeRecord> SecurityTradeRecords { get; set; }
    public virtual DbSet<SecurityTradeRecordByWeek> SecurityTradeRecordsByWeek { get; set; }
    public virtual DbSet<SecurityTradeRecordByMonth> SecurityTradeRecordsByMonth { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=185.105.226.205;Port=5432;Database=securities;Username=dbuser;Password=Nemes341");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SecurityInfo>(entity =>
        {
            entity.ToTable("security_infos");

            entity.Property(e => e.SecurityInfoId).HasColumnName("security_info_id");
            entity.Property(e => e.Isin).HasColumnName("isin");
            entity.Property(e => e.IssueDate).HasColumnName("issue_date");
            entity.Property(e => e.IssueSize).HasColumnName("issue_size");
            entity.Property(e => e.ListLevel).HasColumnName("list_level");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.SecurityId).HasColumnName("security_id");
        });

        modelBuilder.Entity<SecurityTradeRecord>(entity =>
        {
            entity.ToTable("security_trade_records");

            entity.HasKey(e => e.SecurityTradeRecordId);
            entity.HasIndex(e => e.SecurityInfoId, "IX_security_trade_records_security_info_id");

            entity.Property(e => e.SecurityTradeRecordId).HasColumnName("security_trade_record_id");
            entity.Property(e => e.Close).HasColumnName("close");
            entity.Property(e => e.DateOfTrade).HasColumnName("date_of_trade");
            entity.Property(e => e.High).HasColumnName("high");
            entity.Property(e => e.Low).HasColumnName("low");
            entity.Property(e => e.NumberOfTrades).HasColumnName("number_of_trades");
            entity.Property(e => e.Open).HasColumnName("open");
            entity.Property(e => e.SecurityInfoId).HasColumnName("security_info_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.SecurityInfo).WithMany(p => p.SecurityTradeRecords).HasForeignKey(d => d.SecurityInfoId);
        });

        modelBuilder.Entity<SecurityTradeRecordByWeek>(entity =>
        {
            entity.ToTable("security_trade_records_by_week");

            entity.HasKey(e => e.SecurityTradeRecordId);
            entity.HasIndex(e => e.SecurityInfoId, "IX_security_trade_records_by_week_security_info_id");

            entity.Property(e => e.SecurityTradeRecordId).HasColumnName("security_trade_record_id");
            entity.Property(e => e.Close).HasColumnName("close");
            entity.Property(e => e.DateOfTrade).HasColumnName("date_of_trade");
            entity.Property(e => e.High).HasColumnName("high");
            entity.Property(e => e.Low).HasColumnName("low");
            entity.Property(e => e.NumberOfTrades).HasColumnName("number_of_trades");
            entity.Property(e => e.Open).HasColumnName("open");
            entity.Property(e => e.SecurityInfoId).HasColumnName("security_info_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.SecurityInfo).WithMany(p => p.SecurityTradeRecordsByWeek).HasForeignKey(d => d.SecurityInfoId);
        });

        modelBuilder.Entity<SecurityTradeRecordByMonth>(entity =>
        {
            entity.ToTable("security_trade_records_by_month");

            entity.HasKey(e => e.SecurityTradeRecordId);
            entity.HasIndex(e => e.SecurityInfoId, "IX_security_trade_records_by_month_security_info_id");

            entity.Property(e => e.SecurityTradeRecordId).HasColumnName("security_trade_record_id");
            entity.Property(e => e.Close).HasColumnName("close");
            entity.Property(e => e.DateOfTrade).HasColumnName("date_of_trade");
            entity.Property(e => e.High).HasColumnName("high");
            entity.Property(e => e.Low).HasColumnName("low");
            entity.Property(e => e.NumberOfTrades).HasColumnName("number_of_trades");
            entity.Property(e => e.Open).HasColumnName("open");
            entity.Property(e => e.SecurityInfoId).HasColumnName("security_info_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.SecurityInfo).WithMany(p => p.SecurityTradeRecordsByMonth).HasForeignKey(d => d.SecurityInfoId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
