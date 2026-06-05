using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClarityCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGPPractices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GPPractices",
                columns: table => new
                {
                    GPPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PracticeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PracticeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LeadGPName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Town = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GPPractices", x => x.GPPracticeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GPPractices_PracticeCode",
                table: "GPPractices",
                column: "PracticeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GPPractices");
        }
    }
}
