using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaWebPapeleria.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsuarioAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "LastName", "Name", "Password", "RoleId", "Status" },
                values: new object[] { 1, "santiago321lumbarojas456@gmail.com", "Lumba Rojas", "Santiago", "$2b$11$iCeKOqnHlYN6CyNQQdbXWejONCaW85YoXr9vWiB4s9x25oOegDCWO", 1, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);
        }
    }
}
