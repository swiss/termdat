using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "source_templates",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text_de = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    text_fr = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    text_it = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    text_en = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    text_rm = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_source_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_source_templates_user_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "user_accounts",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_source_templates_user_id",
                table: "source_templates",
                column: "user_id",
                schema: "data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "source_templates",
                schema: "data");
        }
    }
}
