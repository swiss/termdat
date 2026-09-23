using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNoteSubCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new codes PART and TOUT
            migrationBuilder.Sql(@"INSERT INTO data.note_sub_codes(id, code, created, created_by, modified, modified_by)
                VALUES
                (13, 'PART', now(), 'SYSTEM', now(), 'SYSTEM'),
                (14, 'TOUT', now(), 'SYSTEM', now(), 'SYSTEM');");

            // Add translations for PART: DE, FR, IT, RM, EN
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (13, 2, 'Teil'), (13, 6, 'Partie'), (13, 7, 'Parte'), (13, 8, 'Part'), (13, 3, 'Part');");

            // Add translations for TOUT: DE, FR, IT, RM, EN
            migrationBuilder.Sql("INSERT INTO data.note_sub_code_translations (note_sub_code_id, language_id, name) VALUES (14, 2, 'Ganzes'), (14, 6, 'Tout'), (14, 7, 'Intero'), (14, 8, 'Entir'), (14, 3, 'Whole');");

            // Update changed translations for existing codes
            // ANT: IT (Antonimo -> Contrario), EN (antonym -> Antonyme)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Contrario' WHERE note_sub_code_id = 1 AND language_id = 7;");
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Antonyme' WHERE note_sub_code_id = 1 AND language_id = 3;");

            // DOM: EN (domain -> DOM: Domain)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Domain' WHERE note_sub_code_id = 2 AND language_id = 3;");

            // EXP: EN (explanatory note -> Explanatory note)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Explanatory note' WHERE note_sub_code_id = 3 AND language_id = 3;");

            // GEN: RM (Noziun generala -> Term general), EN (broader term -> Broader term)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Term general' WHERE note_sub_code_id = 4 AND language_id = 8;");
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Broader term' WHERE note_sub_code_id = 4 AND language_id = 3;");

            // GRM: EN (grammar -> Grammar)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Grammar' WHERE note_sub_code_id = 5 AND language_id = 3;");

            // HIS: EN (history -> History)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'History' WHERE note_sub_code_id = 6 AND language_id = 3;");

            // LIN: EN (language -> Language)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Language' WHERE note_sub_code_id = 7 AND language_id = 3;");

            // REG: EN (region -> Region)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Region' WHERE note_sub_code_id = 8 AND language_id = 3;");

            // SPE: RM (Noziun subordinada -> Term specific), EN (narrower term -> Narrower term)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Term specific' WHERE note_sub_code_id = 9 AND language_id = 8;");
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Narrower term' WHERE note_sub_code_id = 9 AND language_id = 3;");

            // STA: EN (status -> Status)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Status' WHERE note_sub_code_id = 10 AND language_id = 3;");

            // SYS: EN (classification system -> Classification system)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Classification system' WHERE note_sub_code_id = 11 AND language_id = 3;");

            // USG: EN (usage note -> Usage note)
            migrationBuilder.Sql("UPDATE data.note_sub_code_translations SET name = 'Usage note' WHERE note_sub_code_id = 12 AND language_id = 3;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
