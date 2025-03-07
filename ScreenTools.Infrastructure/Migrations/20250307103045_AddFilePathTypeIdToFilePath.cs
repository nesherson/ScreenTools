using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTools.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathTypeIdToFilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilePaths_FilePathTypes_PathTypeId",
                table: "FilePaths");

            migrationBuilder.RenameColumn(
                name: "PathTypeId",
                table: "FilePaths",
                newName: "FilePathTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_FilePaths_PathTypeId",
                table: "FilePaths",
                newName: "IX_FilePaths_FilePathTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilePaths_FilePathTypes_FilePathTypeId",
                table: "FilePaths",
                column: "FilePathTypeId",
                principalTable: "FilePathTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilePaths_FilePathTypes_FilePathTypeId",
                table: "FilePaths");

            migrationBuilder.RenameColumn(
                name: "FilePathTypeId",
                table: "FilePaths",
                newName: "PathTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_FilePaths_FilePathTypeId",
                table: "FilePaths",
                newName: "IX_FilePaths_PathTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilePaths_FilePathTypes_PathTypeId",
                table: "FilePaths",
                column: "PathTypeId",
                principalTable: "FilePathTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
