using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class AddAdminUnitTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_info",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_info", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admin_info_entry",
                schema: "data",
                columns: table => new
                {
                    admin_infos_id = table.Column<int>(type: "integer", nullable: false),
                    entries_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_info_entry", x => new { x.admin_infos_id, x.entries_id });
                    table.ForeignKey(
                        name: "fk_admin_info_entry_admin_info_admin_infos_id",
                        column: x => x.admin_infos_id,
                        principalTable: "admin_info",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_admin_info_entry_entries_entries_id",
                        column: x => x.entries_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_info_entry_entries_id",
                table: "admin_info_entry",
                column: "entries_id",
                schema: "data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_info_entry",
                schema: "data");

            migrationBuilder.DropTable(
                name: "admin_info",
                schema: "data");
        }
    }
}
