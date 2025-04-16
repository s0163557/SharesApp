using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharesApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangePrimaryColumnname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "security_trade_record_id",
                table: "security_trade_records_by_week",
                newName: "security_trade_record_by_week_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "security_trade_record_by_week_id",
                table: "security_trade_records_by_week",
                newName: "security_trade_record_id");
        }
    }
}
