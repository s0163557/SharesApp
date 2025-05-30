﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SharesApp.Server.Models;

#nullable disable

namespace SharesApp.Server.Migrations
{
    [DbContext(typeof(SecuritiesContext))]
    [Migration("20250415110248_AddSeparateModel")]
    partial class AddSeparateModel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SharesApp.Server.Models.SecurityInfo", b =>
                {
                    b.Property<int>("SecurityInfoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("security_info_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SecurityInfoId"));

                    b.Property<string>("Isin")
                        .HasColumnType("text")
                        .HasColumnName("isin");

                    b.Property<DateOnly>("IssueDate")
                        .HasColumnType("date")
                        .HasColumnName("issue_date");

                    b.Property<long>("IssueSize")
                        .HasColumnType("bigint")
                        .HasColumnName("issue_size");

                    b.Property<int>("ListLevel")
                        .HasColumnType("integer")
                        .HasColumnName("list_level");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("SecurityId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("security_id");

                    b.HasKey("SecurityInfoId");

                    b.ToTable("security_infos", (string)null);
                });

            modelBuilder.Entity("SharesApp.Server.Models.SecurityTradeRecord", b =>
                {
                    b.Property<int>("SecurityTradeRecordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("security_trade_record_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SecurityTradeRecordId"));

                    b.Property<double>("Close")
                        .HasColumnType("double precision")
                        .HasColumnName("close");

                    b.Property<DateOnly>("DateOfTrade")
                        .HasColumnType("date")
                        .HasColumnName("date_of_trade");

                    b.Property<double>("High")
                        .HasColumnType("double precision")
                        .HasColumnName("high");

                    b.Property<double>("Low")
                        .HasColumnType("double precision")
                        .HasColumnName("low");

                    b.Property<int>("NumberOfTrades")
                        .HasColumnType("integer")
                        .HasColumnName("number_of_trades");

                    b.Property<double>("Open")
                        .HasColumnType("double precision")
                        .HasColumnName("open");

                    b.Property<int>("SecurityInfoId")
                        .HasColumnType("integer")
                        .HasColumnName("security_info_id");

                    b.Property<double>("Value")
                        .HasColumnType("double precision")
                        .HasColumnName("value");

                    b.HasKey("SecurityTradeRecordId");

                    b.HasIndex(new[] { "SecurityInfoId" }, "IX_security_trade_records_security_info_id");

                    b.ToTable("security_trade_records", (string)null);
                });

            modelBuilder.Entity("SharesApp.Server.Models.SecurityTradeRecordByWeek", b =>
                {
                    b.Property<int>("SecurityTradeRecordByWeekId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("security_trade_record_by_week_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SecurityTradeRecordByWeekId"));

                    b.Property<double>("Close")
                        .HasColumnType("double precision")
                        .HasColumnName("close");

                    b.Property<DateOnly>("DateOfTrade")
                        .HasColumnType("date")
                        .HasColumnName("date_of_trade");

                    b.Property<double>("High")
                        .HasColumnType("double precision")
                        .HasColumnName("high");

                    b.Property<double>("Low")
                        .HasColumnType("double precision")
                        .HasColumnName("low");

                    b.Property<int>("NumberOfTrades")
                        .HasColumnType("integer")
                        .HasColumnName("number_of_trades");

                    b.Property<double>("Open")
                        .HasColumnType("double precision")
                        .HasColumnName("open");

                    b.Property<int>("SecurityInfoId")
                        .HasColumnType("integer")
                        .HasColumnName("security_info_id");

                    b.Property<double>("Value")
                        .HasColumnType("double precision")
                        .HasColumnName("value");

                    b.HasKey("SecurityTradeRecordByWeekId");

                    b.HasIndex(new[] { "SecurityInfoId" }, "IX_security_trade_records_by_week_security_info_id");

                    b.ToTable("security_trade_records_by_week", (string)null);
                });

            modelBuilder.Entity("SharesApp.Server.Models.SecurityTradeRecord", b =>
                {
                    b.HasOne("SharesApp.Server.Models.SecurityInfo", "SecurityInfo")
                        .WithMany("SecurityTradeRecords")
                        .HasForeignKey("SecurityInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SecurityInfo");
                });

            modelBuilder.Entity("SharesApp.Server.Models.SecurityTradeRecordByWeek", b =>
                {
                    b.HasOne("SharesApp.Server.Models.SecurityInfo", "SecurityInfo")
                        .WithMany("SecurityTradeRecordsByWeek")
                        .HasForeignKey("SecurityInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SecurityInfo");
                });

            modelBuilder.Entity("SharesApp.Server.Models.SecurityInfo", b =>
                {
                    b.Navigation("SecurityTradeRecords");

                    b.Navigation("SecurityTradeRecordsByWeek");
                });
#pragma warning restore 612, 618
        }
    }
}
