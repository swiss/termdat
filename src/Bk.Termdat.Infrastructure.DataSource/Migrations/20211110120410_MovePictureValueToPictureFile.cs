using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bk.Termdat.Infrastructure.DataSource.Migrations
{
    public partial class MovePictureValueToPictureFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_picture_translations_pictures_picture_id",
                table: "picture_translations",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_pictures",
                table: "pictures",
                schema: "data");

            migrationBuilder.DropIndex(
                name: "ix_pictures_entry_id",
                table: "pictures",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "id",
                table: "pictures",
                schema: "data");

            migrationBuilder.DropColumn(
                name: "value",
                table: "pictures",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "picture_id",
                table: "picture_translations",
                newName: "entry_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_picture_translations_picture_id",
                table: "picture_translations",
                newName: "ix_picture_translations_entry_id",
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pictures",
                table: "pictures",
                column: "entry_id",
                schema: "data");

            migrationBuilder.CreateTable(
                name: "picture_files",
                schema: "data",
                columns: table => new
                {
                    entry_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_picture_files", x => x.entry_id);
                    table.ForeignKey(
                        name: "fk_picture_files_pictures_picture_entry_id",
                        column: x => x.entry_id,
                        principalTable: "pictures",
                        principalColumn: "entry_id",
                        principalSchema: "data",
                        onDelete: ReferentialAction.Cascade);
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_picture_translations_pictures_picture_entry_id",
                table: "picture_translations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "picture_files",
                schema: "data");

            migrationBuilder.DropPrimaryKey(
                name: "pk_pictures",
                table: "pictures",
                schema: "data");

            migrationBuilder.RenameColumn(
                name: "entry_id",
                table: "picture_translations",
                newName: "picture_id",
                schema: "data");

            migrationBuilder.RenameIndex(
                name: "ix_picture_translations_entry_id",
                table: "picture_translations",
                newName: "ix_picture_translations_picture_id",
                schema: "data");

            migrationBuilder.AddColumn<int>(
                    name: "id",
                    table: "pictures",
                    type: "integer",
                    nullable: false,
                    defaultValue: 0,
                    schema: "data")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "value",
                table: "pictures",
                type: "bytea",
                nullable: false,
                defaultValue: Array.Empty<byte>(),
                schema: "data");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pictures",
                table: "pictures",
                column: "id",
                schema: "data");

            migrationBuilder.CreateIndex(
                name: "ix_pictures_entry_id",
                table: "pictures",
                column: "entry_id",
                unique: true,
                schema: "data");

            migrationBuilder.AddForeignKey(
                name: "fk_picture_translations_pictures_picture_id",
                table: "picture_translations",
                column: "picture_id",
                principalTable: "pictures",
                principalColumn: "id",
                principalSchema: "data",
                onDelete: ReferentialAction.Cascade,
                schema: "data");
        }
    }
}
