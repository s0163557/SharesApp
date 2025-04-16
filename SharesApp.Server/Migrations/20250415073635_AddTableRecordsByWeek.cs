using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SharesApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTableRecordsByWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "security_trade_records_by_week",
                columns: table => new
                {
                    security_trade_record_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    security_info_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_trade = table.Column<DateOnly>(type: "date", nullable: false),
                    number_of_trades = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    open = table.Column<double>(type: "double precision", nullable: false),
                    low = table.Column<double>(type: "double precision", nullable: false),
                    high = table.Column<double>(type: "double precision", nullable: false),
                    close = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_trade_records_by_week", x => x.security_trade_record_id);
                    table.ForeignKey(
                        name: "FK_security_trade_records_by_week_security_infos_security_info~",
                        column: x => x.security_info_id,
                        principalTable: "security_infos",
                        principalColumn: "security_info_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_security_trade_records_by_week_security_info_id",
                table: "security_trade_records_by_week",
                column: "security_info_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "security_trade_records_by_week");

        }
    }
}
