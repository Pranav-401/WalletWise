using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletWise.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete existing expenses before adding user ownership.
            migrationBuilder.Sql("DELETE FROM [Expense]");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Expense",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Expense");
        }
    }
}
