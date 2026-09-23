using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema("data");

            migrationBuilder.CreateTable(
                name: "Collections",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionTranslations",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    CollectionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionTranslations_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanguageTranslations",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LanguageTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryTranslations",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Definition = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Name = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Terminus = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Abbreviation = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Phraseology = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    EntryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryTranslations_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntryTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTranslations_CollectionId",
                table: "CollectionTranslations",
                column: "CollectionId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionTranslations_LanguageId",
                table: "CollectionTranslations",
                column: "LanguageId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_CollectionId",
                table: "Entries",
                column: "CollectionId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTranslations_EntryId",
                table: "EntryTranslations",
                column: "EntryId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_EntryTranslations_LanguageId",
                table: "EntryTranslations",
                column: "LanguageId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageTranslations_LanguageId",
                table: "LanguageTranslations",
                column: "LanguageId",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionTranslations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "EntryTranslations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "LanguageTranslations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Entries",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Languages",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Collections",
                schema: "data");
        }
    }
}
