using Microsoft.EntityFrameworkCore.Migrations;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class UseSnakeCaseConvention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionTranslation_Collection_CollectionId",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionTranslation_Language_LanguageId",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainEntry_Domain_DomainsId",
                table: "DomainEntry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainEntry_Entry_EntriesId",
                table: "DomainEntry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainTranslation_Domain_DomainId",
                table: "DomainTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainTranslation_Language_LanguageId",
                table: "DomainTranslation",
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
                name: "FK_EntryCollection_Collection_RelatedCollectionsId",
                table: "EntryCollection",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryCollection_Entry_RelatedEntriesId",
                table: "EntryCollection",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryLanguage_Entry_EntryId",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryLanguage_Language_LanguageId",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_LanguageTranslation_Language_LanguageId",
                table: "LanguageTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeTranslation_Language_LanguageId",
                table: "OfficeTranslation",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeTranslation_Office_OfficeId",
                table: "OfficeTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfficeTranslation",
                table: "OfficeTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Office",
                table: "Office",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LanguageTranslation",
                table: "LanguageTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Language",
                table: "Language",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryLanguage",
                table: "EntryLanguage",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntryCollection",
                table: "EntryCollection",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entry",
                table: "Entry",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DomainTranslation",
                table: "DomainTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DomainEntry",
                table: "DomainEntry",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Domain",
                table: "Domain",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CollectionTranslation",
                table: "CollectionTranslation",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Collection",
                table: "Collection",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "OfficeTranslation",
                newName: "office_translations",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Office",
                newName: "offices",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "LanguageTranslation",
                newName: "language_translations",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Language",
                newName: "languages",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "EntryLanguage",
                newName: "entry_languages",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "EntryCollection",
                newName: "collection_entry",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Entry",
                newName: "entries",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "DomainTranslation",
                newName: "domain_translations",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "DomainEntry",
                newName: "domain_entry",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Domain",
                newName: "domains",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "CollectionTranslation",
                newName: "collection_translations",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "Collection",
                newName: "collections",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "office_translations",
                newName: "name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "office_translations",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "OfficeId",
                table: "office_translations",
                newName: "office_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "office_translations",
                newName: "language_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeTranslation_OfficeId",
                table: "office_translations",
                newName: "ix_office_translations_office_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeTranslation_LanguageId",
                table: "office_translations",
                newName: "ix_office_translations_language_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "offices",
                newName: "value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "offices",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "language_translations",
                newName: "name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "language_translations",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "language_translations",
                newName: "language_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "IsoCode",
                table: "language_translations",
                newName: "iso_code",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_LanguageTranslation_LanguageId",
                table: "language_translations",
                newName: "ix_language_translations_language_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "languages",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "SortIndex",
                table: "languages",
                newName: "sort_index",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "IsoCode",
                table: "languages",
                newName: "iso_code",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "IsUiLanguage",
                table: "languages",
                newName: "is_ui_language",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Terminus",
                table: "entry_languages",
                newName: "terminus",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Sequence",
                table: "entry_languages",
                newName: "sequence",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Phraseology",
                table: "entry_languages",
                newName: "phraseology",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "entry_languages",
                newName: "name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Definition",
                table: "entry_languages",
                newName: "definition",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Abbreviation",
                table: "entry_languages",
                newName: "abbreviation",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "entry_languages",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "entry_languages",
                newName: "language_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "EntryId",
                table: "entry_languages",
                newName: "entry_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryLanguage_LanguageId",
                table: "entry_languages",
                newName: "ix_entry_languages_language_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryLanguage_EntryId",
                table: "entry_languages",
                newName: "ix_entry_languages_entry_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "RelatedEntriesId",
                table: "collection_entry",
                newName: "related_entries_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "RelatedCollectionsId",
                table: "collection_entry",
                newName: "related_collections_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_EntryCollection_RelatedEntriesId",
                table: "collection_entry",
                newName: "ix_collection_entry_related_entries_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "entries",
                newName: "status",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Reliability",
                table: "entries",
                newName: "reliability",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Modified",
                table: "entries",
                newName: "modified",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Identification",
                table: "entries",
                newName: "identification",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "entries",
                newName: "created",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "entries",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "OfficeId",
                table: "entries",
                newName: "office_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "CollectionId",
                table: "entries",
                newName: "collection_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_Entry_OfficeId",
                table: "entries",
                newName: "ix_entries_office_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_Entry_CollectionId",
                table: "entries",
                newName: "ix_entries_collection_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "domain_translations",
                newName: "name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "domain_translations",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "domain_translations",
                newName: "language_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "DomainId",
                table: "domain_translations",
                newName: "domain_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_DomainTranslation_LanguageId",
                table: "domain_translations",
                newName: "ix_domain_translations_language_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_DomainTranslation_DomainId",
                table: "domain_translations",
                newName: "ix_domain_translations_domain_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "EntriesId",
                table: "domain_entry",
                newName: "entries_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "DomainsId",
                table: "domain_entry",
                newName: "domains_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_DomainEntry_EntriesId",
                table: "domain_entry",
                newName: "ix_domain_entry_entries_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "domains",
                newName: "value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "domains",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "collection_translations",
                newName: "name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "collection_translations",
                newName: "id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "collection_translations",
                newName: "language_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "CollectionId",
                table: "collection_translations",
                newName: "collection_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_CollectionTranslation_LanguageId",
                table: "collection_translations",
                newName: "ix_collection_translations_language_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "IX_CollectionTranslation_CollectionId",
                table: "collection_translations",
                newName: "ix_collection_translations_collection_id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "collections",
                newName: "value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "collections",
                newName: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_office_translations",
                table: "office_translations",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_offices",
                table: "offices",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_language_translations",
                table: "language_translations",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_languages",
                table: "languages",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_entry_languages",
                table: "entry_languages",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_collection_entry",
                table: "collection_entry",
                columns: new[] { "related_collections_id", "related_entries_id" },
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_entries",
                table: "entries",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_domain_translations",
                table: "domain_translations",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_domain_entry",
                table: "domain_entry",
                columns: new[] { "domains_id", "entries_id" },
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_domains",
                table: "domains",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_collection_translations",
                table: "collection_translations",
                column: "id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_collections",
                table: "collections",
                column: "id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_collection_entry_collections_related_collections_id",
                table: "collection_entry",
                column: "related_collections_id",
                principalTable: "collections",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_collection_entry_entries_related_entries_id",
                table: "collection_entry",
                column: "related_entries_id",
                principalTable: "entries",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_collection_translations_collections_collection_id",
                table: "collection_translations",
                column: "collection_id",
                principalTable: "collections",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_collection_translations_languages_language_id",
                table: "collection_translations",
                column: "language_id",
                principalTable: "languages",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_domain_entry_domains_domains_id",
                table: "domain_entry",
                column: "domains_id",
                principalTable: "domains",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_domain_entry_entries_entries_id",
                table: "domain_entry",
                column: "entries_id",
                principalTable: "entries",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_domain_translations_domains_domain_id",
                table: "domain_translations",
                column: "domain_id",
                principalTable: "domains",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_domain_translations_languages_language_id",
                table: "domain_translations",
                column: "language_id",
                principalTable: "languages",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entries_collections_collection_id",
                table: "entries",
                column: "collection_id",
                principalTable: "collections",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entries_offices_office_id",
                table: "entries",
                column: "office_id",
                principalTable: "offices",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entry_languages_entries_entry_id",
                table: "entry_languages",
                column: "entry_id",
                principalTable: "entries",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_entry_languages_languages_language_id",
                table: "entry_languages",
                column: "language_id",
                principalTable: "languages",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_language_translations_languages_language_id",
                table: "language_translations",
                column: "language_id",
                principalTable: "languages",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_office_translations_languages_language_id",
                table: "office_translations",
                column: "language_id",
                principalTable: "languages",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_office_translations_offices_office_id",
                table: "office_translations",
                column: "office_id",
                principalTable: "offices",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collection_entry_collections_related_collections_id",
                table: "collection_entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_collection_entry_entries_related_entries_id",
                table: "collection_entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_collection_translations_collections_collection_id",
                table: "collection_translations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_collection_translations_languages_language_id",
                table: "collection_translations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_domain_entry_domains_domains_id",
                table: "domain_entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_domain_entry_entries_entries_id",
                table: "domain_entry",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_domain_translations_domains_domain_id",
                table: "domain_translations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_domain_translations_languages_language_id",
                table: "domain_translations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_entries_collections_collection_id",
                table: "entries",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_entries_offices_office_id",
                table: "entries",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_entry_languages_entries_entry_id",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_entry_languages_languages_language_id",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_language_translations_languages_language_id",
                table: "language_translations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_office_translations_languages_language_id",
                table: "office_translations",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_office_translations_offices_office_id",
                table: "office_translations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_offices",
                table: "offices",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_office_translations",
                table: "office_translations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_languages",
                table: "languages",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_language_translations",
                table: "language_translations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_entry_languages",
                table: "entry_languages",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_entries",
                table: "entries",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_domains",
                table: "domains",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_domain_translations",
                table: "domain_translations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_domain_entry",
                table: "domain_entry",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_collections",
                table: "collections",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_collection_translations",
                table: "collection_translations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_collection_entry",
                table: "collection_entry",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "offices",
                newName: "Office",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "office_translations",
                newName: "OfficeTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "languages",
                newName: "Language",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "language_translations",
                newName: "LanguageTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "entry_languages",
                newName: "EntryLanguage",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "entries",
                newName: "Entry",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "domains",
                newName: "Domain",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "domain_translations",
                newName: "DomainTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "domain_entry",
                newName: "DomainEntry",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "collections",
                newName: "Collection",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "collection_translations",
                newName: "CollectionTranslation",
                schema: "data");

            migrationBuilder.RenameTable(
                name: "collection_entry",
                newName: "EntryCollection",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "Office",
                newName: "Value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Office",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "OfficeTranslation",
                newName: "Name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OfficeTranslation",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "office_id",
                table: "OfficeTranslation",
                newName: "OfficeId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "language_id",
                table: "OfficeTranslation",
                newName: "LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_office_translations_office_id",
                table: "OfficeTranslation",
                newName: "IX_OfficeTranslation_OfficeId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_office_translations_language_id",
                table: "OfficeTranslation",
                newName: "IX_OfficeTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Language",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "sort_index",
                table: "Language",
                newName: "SortIndex",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "iso_code",
                table: "Language",
                newName: "IsoCode",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "is_ui_language",
                table: "Language",
                newName: "IsUiLanguage",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "LanguageTranslation",
                newName: "Name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "LanguageTranslation",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "language_id",
                table: "LanguageTranslation",
                newName: "LanguageId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "iso_code",
                table: "LanguageTranslation",
                newName: "IsoCode",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_language_translations_language_id",
                table: "LanguageTranslation",
                newName: "IX_LanguageTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "terminus",
                table: "EntryLanguage",
                newName: "Terminus",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "sequence",
                table: "EntryLanguage",
                newName: "Sequence",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "phraseology",
                table: "EntryLanguage",
                newName: "Phraseology",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "EntryLanguage",
                newName: "Name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "definition",
                table: "EntryLanguage",
                newName: "Definition",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "abbreviation",
                table: "EntryLanguage",
                newName: "Abbreviation",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "EntryLanguage",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "language_id",
                table: "EntryLanguage",
                newName: "LanguageId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "entry_id",
                table: "EntryLanguage",
                newName: "EntryId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_entry_languages_language_id",
                table: "EntryLanguage",
                newName: "IX_EntryLanguage_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_entry_languages_entry_id",
                table: "EntryLanguage",
                newName: "IX_EntryLanguage_EntryId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Entry",
                newName: "Status",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "reliability",
                table: "Entry",
                newName: "Reliability",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "modified",
                table: "Entry",
                newName: "Modified",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "identification",
                table: "Entry",
                newName: "Identification",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "Entry",
                newName: "Created",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Entry",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "office_id",
                table: "Entry",
                newName: "OfficeId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "collection_id",
                table: "Entry",
                newName: "CollectionId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_entries_office_id",
                table: "Entry",
                newName: "IX_Entry_OfficeId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_entries_collection_id",
                table: "Entry",
                newName: "IX_Entry_CollectionId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "Domain",
                newName: "Value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Domain",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "DomainTranslation",
                newName: "Name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DomainTranslation",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "language_id",
                table: "DomainTranslation",
                newName: "LanguageId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "domain_id",
                table: "DomainTranslation",
                newName: "DomainId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_domain_translations_language_id",
                table: "DomainTranslation",
                newName: "IX_DomainTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_domain_translations_domain_id",
                table: "DomainTranslation",
                newName: "IX_DomainTranslation_DomainId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "entries_id",
                table: "DomainEntry",
                newName: "EntriesId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "domains_id",
                table: "DomainEntry",
                newName: "DomainsId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_domain_entry_entries_id",
                table: "DomainEntry",
                newName: "IX_DomainEntry_EntriesId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "Collection",
                newName: "Value",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Collection",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "CollectionTranslation",
                newName: "Name",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CollectionTranslation",
                newName: "Id",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "language_id",
                table: "CollectionTranslation",
                newName: "LanguageId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "collection_id",
                table: "CollectionTranslation",
                newName: "CollectionId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_collection_translations_language_id",
                table: "CollectionTranslation",
                newName: "IX_CollectionTranslation_LanguageId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_collection_translations_collection_id",
                table: "CollectionTranslation",
                newName: "IX_CollectionTranslation_CollectionId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "related_entries_id",
                table: "EntryCollection",
                newName: "RelatedEntriesId",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "related_collections_id",
                table: "EntryCollection",
                newName: "RelatedCollectionsId",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_collection_entry_related_entries_id",
                table: "EntryCollection",
                newName: "IX_EntryCollection_RelatedEntriesId",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Office",
                table: "Office",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeTranslation",
                table: "OfficeTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Language",
                table: "Language",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LanguageTranslation",
                table: "LanguageTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryLanguage",
                table: "EntryLanguage",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entry",
                table: "Entry",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Domain",
                table: "Domain",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DomainTranslation",
                table: "DomainTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DomainEntry",
                table: "DomainEntry",
                columns: new[] { "DomainsId", "EntriesId" },
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Collection",
                table: "Collection",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CollectionTranslation",
                table: "CollectionTranslation",
                column: "Id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EntryCollection",
                table: "EntryCollection",
                columns: new[] { "RelatedCollectionsId", "RelatedEntriesId" },
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
                name: "FK_CollectionTranslation_Language_LanguageId",
                table: "CollectionTranslation",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainEntry_Domain_DomainsId",
                table: "DomainEntry",
                column: "DomainsId",
                principalTable: "Domain",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainEntry_Entry_EntriesId",
                table: "DomainEntry",
                column: "EntriesId",
                principalTable: "Entry",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainTranslation_Domain_DomainId",
                table: "DomainTranslation",
                column: "DomainId",
                principalTable: "Domain",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainTranslation_Language_LanguageId",
                table: "DomainTranslation",
                column: "LanguageId",
                principalTable: "Language",
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
                name: "FK_EntryCollection_Collection_RelatedCollectionsId",
                table: "EntryCollection",
                column: "RelatedCollectionsId",
                principalTable: "Collection",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_EntryCollection_Entry_RelatedEntriesId",
                table: "EntryCollection",
                column: "RelatedEntriesId",
                principalTable: "Entry",
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

            migrationBuilder.AddForeignKey(
                name: "FK_EntryLanguage_Language_LanguageId",
                table: "EntryLanguage",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_LanguageTranslation_Language_LanguageId",
                table: "LanguageTranslation",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeTranslation_Language_LanguageId",
                table: "OfficeTranslation",
                column: "LanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeTranslation_Office_OfficeId",
                table: "OfficeTranslation",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }
    }
}
