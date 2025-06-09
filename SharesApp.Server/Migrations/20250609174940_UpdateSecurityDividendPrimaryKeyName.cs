using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SharesApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSecurityDividendPrimaryKeyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inflations",
                columns: table => new
                {
                    inflation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date_of_record = table.Column<DateOnly>(type: "date", nullable: false),
                    key_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    inflation_value = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inflations_pkey", x => x.inflation_id);
                });

            migrationBuilder.CreateTable(
                name: "security_dividends",
                columns: table => new
                {
                    security_dividends_id = table.Column<int>(type: "integer", nullable: false),
                    security_info_id = table.Column<int>(type: "integer", nullable: false),
                    registry = table.Column<DateOnly>(type: "date", nullable: true),
                    date_of_payment = table.Column<DateOnly>(type: "date", nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: true),
                    dividend = table.Column<double>(type: "double precision", nullable: false),
                    income = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("security_dividend_pkey", x => x.security_dividends_id);
                    table.ForeignKey(
                        name: "FK_security_dividends_security_infos_security_info_id",
                        column: x => x.security_info_id,
                        principalTable: "security_infos",
                        principalColumn: "security_info_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_security_dividends_security_info_id",
                table: "security_dividends",
                column: "security_info_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inflations");

            migrationBuilder.DropTable(
                name: "security_dividends");
        }
    }
}
