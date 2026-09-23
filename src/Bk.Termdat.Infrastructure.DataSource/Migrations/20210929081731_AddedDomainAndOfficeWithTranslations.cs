using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddedDomainAndOfficeWithTranslations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionTranslation_Collection_CollectionId",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_Entry_Collection_CollectionId",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryLanguage_Entry_EntryId",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.AlterColumn<int>(
                name: "EntryId",
                table: "EntryLanguage",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Entry",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "InProgress",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                schema: "data");

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "Entry",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Entry",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                schema: "data");

            migrationBuilder.AddColumn<int>(
                name: "Identification",
                table: "Entry",
                type: "integer",
                nullable: true,
                schema: "data");

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Entry",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                schema: "data");

            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "Entry",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                schema: "data");

            migrationBuilder.AddColumn<string>(
                name: "Reliability",
                table: "Entry",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "FirstDraft",
                schema: "data");

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "CollectionTranslation",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                schema: "data");

            migrationBuilder.CreateTable(
                name: "Domain",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domain", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntryCollection",
                schema: "data",
                columns: table => new
                {
                    RelatedCollectionsId = table.Column<int>(type: "integer", nullable: false),
                    RelatedEntriesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryCollection", x => new { x.RelatedCollectionsId, x.RelatedEntriesId });
                    table.ForeignKey(
                        name: "FK_EntryCollection_Collection_RelatedCollectionsId",
                        column: x => x.RelatedCollectionsId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntryCollection_Entry_RelatedEntriesId",
                        column: x => x.RelatedEntriesId,
                        principalTable: "Entry",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Office",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Office", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainEntry",
                schema: "data",
                columns: table => new
                {
                    DomainsId = table.Column<int>(type: "integer", nullable: false),
                    EntriesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEntry", x => new { x.DomainsId, x.EntriesId });
                    table.ForeignKey(
                        name: "FK_DomainEntry_Domain_DomainsId",
                        column: x => x.DomainsId,
                        principalTable: "Domain",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainEntry_Entry_EntriesId",
                        column: x => x.EntriesId,
                        principalTable: "Entry",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainTranslation",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    DomainId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainTranslation_Domain_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domain",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainTranslation_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfficeTranslation",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    OfficeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficeTranslation_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfficeTranslation_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entry_OfficeId",
                table: "Entry",
                column: "OfficeId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_DomainEntry_EntriesId",
                table: "DomainEntry",
                column: "EntriesId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_DomainTranslation_DomainId",
                table: "DomainTranslation",
                column: "DomainId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_DomainTranslation_LanguageId",
                table: "DomainTranslation",
                column: "LanguageId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_EntryCollection_RelatedEntriesId",
                table: "EntryCollection",
                column: "RelatedEntriesId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeTranslation_LanguageId",
                table: "OfficeTranslation",
                column: "LanguageId",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeTranslation_OfficeId",
                table: "OfficeTranslation",
                column: "OfficeId",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionTranslation_Collection_CollectionId",
                table: "CollectionTranslation",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_Entry_Collection_CollectionId",
                table: "Entry",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_Entry_Office_OfficeId",
                table: "Entry",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryLanguage_Entry_EntryId",
                table: "EntryLanguage",
                column: "EntryId",
                principalTable: "Entry",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionTranslation_Collection_CollectionId",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_Entry_Collection_CollectionId",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_Entry_Office_OfficeId",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryLanguage_Entry_EntryId",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DomainEntry",
                schema: "data");

            migrationBuilder.DropTable(
                name: "DomainTranslation",
                schema: "data");

            migrationBuilder.DropTable(
                name: "EntryCollection",
                schema: "data");

            migrationBuilder.DropTable(
                name: "OfficeTranslation",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Domain",
                schema: "data");

            migrationBuilder.DropTable(
                name: "Office",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "IX_Entry_OfficeId",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "Identification",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "Reliability",
                table: "Entry",
                schema: "data");

            migrationBuilder.AlterColumn<int>(
                name: "EntryId",
                table: "EntryLanguage",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                schema: "data");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Entry",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldDefaultValue: "InProgress",
                schema: "data");

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "Entry",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                schema: "data");

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "CollectionTranslation",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionTranslation_Collection_CollectionId",
                table: "CollectionTranslation",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_Entry_Collection_CollectionId",
                table: "Entry",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryLanguage_Entry_EntryId",
                table: "EntryLanguage",
                column: "EntryId",
                principalTable: "Entry",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Restrict,
                schema: "data");
        }
    }
}
