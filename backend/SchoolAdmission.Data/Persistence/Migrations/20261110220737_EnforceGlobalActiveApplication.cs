using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolAdmission.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceGlobalActiveApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holds_ClassId_StudentId",
                table: "Holds");

            migrationBuilder.CreateIndex(
                name: "IX_Holds_StudentId",
                table: "Holds",
                column: "StudentId",
                unique: true,
                filter: "\"Status\" IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holds_StudentId",
                table: "Holds");

            migrationBuilder.CreateIndex(
                name: "IX_Holds_ClassId_StudentId",
                table: "Holds",
                columns: new[] { "ClassId", "StudentId" },
                unique: true,
                filter: "\"Status\" IN (1, 2)");
        }
    }
}
