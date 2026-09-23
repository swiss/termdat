using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    /// <inheritdoc />
    public partial class AddExportLindasFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "export_lindas",
                table: "collections",
                type: "boolean",
                nullable: true,
                defaultValue: false,
                schema: "data");

            migrationBuilder.Sql(@"
                UPDATE data.collections SET export_lindas = false;
                UPDATE data.collections
                    SET export_lindas = true
                    WHERE code IN (
                        'ABR24', 'ADFA23', 'AHV18', 'ARB17', 'ASY14', 'ATOM23', 'AUS14', 'AUT15', 'BAU17', 'BELA16',
                        'BIOME20', 'BPG17', 'BVG17', 'BVZ24', 'CHI15', 'CPC23', 'CPP18', 'CYBER20', 'DEC19', 'DIGIT22',
                        'DROIT21', 'DSCHU24', 'DURAB21', 'ECON18', 'BILD18', 'EGOV19', 'ENR22', 'ERB18', 'EWB17', 'FAMI14',
                        'FAMZU19', 'FHS19', 'FIBS21', 'FIF20', 'FMA16', 'FORME21', 'GENTE20', 'GEOD19', 'GEOL23', 'GRF19',
                        'GWG15', 'INFEC20', 'INI24', 'IRS17', 'IUFRO19', 'KAR20', 'KLIMA22', 'KRIMI18', 'LDA17', 'LOICH24',
                        'LUFT23', 'MEDAP21', 'MET16', 'METE14', 'MIET17', 'MWST22', 'NADI18', 'NAS23', 'NUC23', 'OAI13',
                        'OEREB16', 'ORV19', 'ORVER17', 'PDA12', 'PDAM18', 'PEN15', 'PERS18', 'PFS17', 'PNR24', 'POG22',
                        'POR19', 'PPM12', 'RETT18', 'RAUM18', 'RMAB24', 'SACH18', 'SBFIB19', 'SCHKG24', 'SNU23', 'SOC20',
                        'SPIEL19', 'SPRA20', 'STSCH23', 'SVG20', 'TIT15', 'TITPA12', 'TRAME21', 'TSS16', 'UFAG13', 'WAFFE18',
                        'WEA19', 'WWI17', 'ZGB20', 'ZIV15', 'ZSBS21', 'ZSW11'
                    );
            ");

            migrationBuilder.AlterColumn<bool>(
                name: "export_lindas",
                table: "collections",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                schema: "data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "export_lindas",
                table: "collections",
                schema: "data");
        }
    }
}
