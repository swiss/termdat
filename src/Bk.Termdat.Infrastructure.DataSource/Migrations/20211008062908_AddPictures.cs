using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddPictures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pictures",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<byte[]>(type: "bytea", nullable: false),
                    file_extension =
                        table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_type =
                        table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entry_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pictures", x => x.id);
                    table.ForeignKey(
                        name: "fk_pictures_entries_entry_id",
                        column: x => x.entry_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "picture_translations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    language_id = table.Column<int>(type: "integer", nullable: false),
                    picture_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_picture_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_picture_translations_languages_language_id",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_picture_translations_pictures_picture_id",
                        column: x => x.picture_id,
                        principalTable: "pictures",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_picture_translations_language_id",
                table: "picture_translations",
                column: "language_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_picture_translations_picture_id",
                table: "picture_translations",
                column: "picture_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_pictures_entry_id",
                table: "pictures",
                column: "entry_id",
                unique: true,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "picture_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "pictures",
                schema: "data");
        }
    }
}
