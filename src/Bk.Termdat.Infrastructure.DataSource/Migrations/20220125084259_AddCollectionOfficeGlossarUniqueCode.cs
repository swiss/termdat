using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddCollectionOfficeGlossarUniqueCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_offices_code",
                table: "offices",
                column: "code",
                unique: true,
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_glossaries_code",
                table: "glossaries",
                column: "code",
                unique: true,
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_collections_code",
                table: "collections",
                column: "code",
                unique: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_offices_code",
                table: "offices",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_glossaries_code",
                table: "glossaries",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_collections_code",
                table: "collections",
                schema: "data");
        }
    }
}
