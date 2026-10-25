using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAutenticacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "paciente",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "odontologo",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_consultorio_piso_numero",
                table: "consultorio",
                columns: new[] { "piso", "numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_consultorio_piso_numero",
                table: "consultorio");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "odontologo");
        }
    }
}
