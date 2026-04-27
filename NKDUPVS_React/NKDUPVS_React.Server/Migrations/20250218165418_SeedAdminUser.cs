using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NKDUPVS_React.Server.Migrations
{
    public partial class SeedAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "user",
                columns: new[] { "Code", "Email", "IsAdmin", "LastName", "Name", "Password", "PhoneNumber", "Username" },
                values: new object[] { "admin001", "admin@example.com", true, "User", "Admin", BCrypt.Net.BCrypt.HashPassword("admin123"), "1234567890", "admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the seed data if the migration is rolled back
            migrationBuilder.DeleteData(
                table: "user",
                keyColumn: "Code",
                keyValue: "admin001");
        }
    }
}