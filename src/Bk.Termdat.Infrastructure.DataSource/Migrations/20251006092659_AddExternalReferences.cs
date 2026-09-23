using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_picture_files_pictures_picture_temp_id1",
                schema: "data",
                table: "picture_files");

            migrationBuilder.DropForeignKey(
                name: "fk_picture_translations_pictures_picture_temp_id2",
                schema: "data",
                table: "picture_translations");

            migrationBuilder.CreateTable(
                name: "entry_external_links",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    entry_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entry_external_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_entry_external_links_entries_entry_id",
                        column: x => x.entry_id,
                        principalSchema: "data",
                        principalTable: "entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_entry_external_links_entry_id",
                schema: "data",
                table: "entry_external_links",
                column: "entry_id");

            migrationBuilder.AddForeignKey(
                name: "fk_picture_files_pictures_entry_id",
                schema: "data",
                table: "picture_files",
                column: "entry_id",
                principalSchema: "data",
                principalTable: "pictures",
                principalColumn: "entry_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_picture_translations_pictures_entry_id",
                schema: "data",
                table: "picture_translations",
                column: "entry_id",
                principalSchema: "data",
                principalTable: "pictures",
                principalColumn: "entry_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_picture_files_pictures_entry_id",
                schema: "data",
                table: "picture_files");

            migrationBuilder.DropForeignKey(
                name: "fk_picture_translations_pictures_entry_id",
                schema: "data",
                table: "picture_translations");

            migrationBuilder.DropTable(
                name: "entry_external_links",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_picture_files_pictures_picture_temp_id1",
                schema: "data",
                table: "picture_files",
                column: "entry_id",
                principalSchema: "data",
                principalTable: "pictures",
                principalColumn: "entry_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_picture_translations_pictures_picture_temp_id2",
                schema: "data",
                table: "picture_translations",
                column: "entry_id",
                principalSchema: "data",
                principalTable: "pictures",
                principalColumn: "entry_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
