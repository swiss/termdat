using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddUniqueEmailToUserAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_user_accounts_email",
                table: "user_accounts",
                column: "email",
                unique: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_accounts_email",
                table: "user_accounts",
                schema: "data");
        }
    }
}
