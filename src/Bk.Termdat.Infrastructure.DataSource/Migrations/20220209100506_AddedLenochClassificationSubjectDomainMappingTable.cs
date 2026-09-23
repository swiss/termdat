using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddedLenochClassificationSubjectDomainMappingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_picture_files_pictures_picture_entry_id",
                table: "picture_files",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_picture_translations_pictures_picture_entry_id",
                table: "picture_translations",
                schema: "data");

            migrationBuilder.CreateTable(
                name: "lenoch_classification_subject_domain_mapping",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lenoch_classification_id = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    domain_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lenoch_classification_subject_domain_mapping", x => x.id);
                    table.ForeignKey(
                        name: "fk_lenoch_classification_subject_domain_mapping_domains_domain",
                        column: x => x.domain_id,
                        principalTable: "domains",
                        principalColumn: "id",
                        principalSchema: "data");
                    table.ForeignKey(
                        name: "fk_lenoch_classification_subject_domain_mapping_lenoch_classif",
                        column: x => x.lenoch_classification_id,
                        principalTable: "lenoch_classifications",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lenoch_classification_subject_domain_mapping_subjects_subje",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lenoch_classification_subject_domain_mapping_domain_id",
                table: "lenoch_classification_subject_domain_mapping",
                column: "domain_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_lenoch_classification_subject_domain_mapping_lenoch_classif",
                table: "lenoch_classification_subject_domain_mapping",
                column: "lenoch_classification_id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_lenoch_classification_subject_domain_mapping_subject_id",
                table: "lenoch_classification_subject_domain_mapping",
                column: "subject_id",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_picture_files_pictures_picture_temp_id1",
                table: "picture_files",
                column: "entry_id",
                principalTable: "pictures",
                principalColumn: "entry_id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_picture_translations_pictures_picture_temp_id2",
                table: "picture_translations",
                column: "entry_id",
                principalTable: "pictures",
                principalColumn: "entry_id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_picture_files_pictures_picture_temp_id1",
                table: "picture_files",
                schema: "data");

            migrationBuilder.DropForeignKey(
                name: "fk_picture_translations_pictures_picture_temp_id2",
                table: "picture_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "lenoch_classification_subject_domain_mapping",
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_picture_files_pictures_picture_entry_id",
                table: "picture_files",
                column: "entry_id",
                principalTable: "pictures",
                principalColumn: "entry_id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_picture_translations_pictures_picture_entry_id",
                table: "picture_translations",
                column: "entry_id",
                principalTable: "pictures",
                principalColumn: "entry_id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }
    }
}
