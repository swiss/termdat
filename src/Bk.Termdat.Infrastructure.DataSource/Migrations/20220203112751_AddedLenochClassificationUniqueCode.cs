using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddedLenochClassificationUniqueCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_lenoch_classifications_code",
                table: "lenoch_classifications",
                column: "code",
                unique: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_lenoch_classifications_code",
                table: "lenoch_classifications",
                schema: "data");
        }
    }
}
