using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTools.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGalleryPathsAddFilePathsAndFilePathTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryPaths");

            migrationBuilder.CreateTable(
                name: "FilePathTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Abrv = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePathTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilePaths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    PathTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePaths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilePaths_FilePathTypes_PathTypeId",
                        column: x => x.PathTypeId,
                        principalTable: "FilePathTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilePaths_PathTypeId",
                table: "FilePaths",
                column: "PathTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilePaths");

            migrationBuilder.DropTable(
                name: "FilePathTypes");

            migrationBuilder.CreateTable(
                name: "GalleryPaths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryPaths", x => x.Id);
                });
        }
    }
}
