using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddedEntryReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reference",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.CreateTable(
                name: "entry_references",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entry_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    referenced_entry_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entry_references", x => x.id);
                    table.ForeignKey(
                        name: "fk_entry_references_entries_entry_id",
                        column: x => x.entry_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entry_references_entries_referenced_entry_id",
                        column: x => x.referenced_entry_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_entry_references_entry_id",
                table: "entry_references",
                column: "entry_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_entry_references_referenced_entry_id",
                table: "entry_references",
                column: "referenced_entry_id",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entry_references",
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "reference",
                table: "entry_languages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                schema: "data");
        }
    }
}
