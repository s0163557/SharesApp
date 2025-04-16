using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SharesApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSeparateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_security_trade_records_security_info_id",
                table: "security_trade_records_by_week");

            migrationBuilder.CreateIndex(
                name: "IX_security_trade_records_security_info_id",
                table: "security_trade_records",
                column: "security_info_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_security_trade_records_security_info_id",
                table: "security_trade_records_by_week",
                column: "security_info_id");
        }
    }
}
