using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "consultorio",
                columns: table => new
                {
                    id_consultorio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    numero = table.Column<int>(type: "int", nullable: false),
                    piso = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    equipamiento = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultorio", x => x.id_consultorio);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "odontologo",
                columns: table => new
                {
                    id_odontologo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apellido = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dni = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    matricula = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    especialidad = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_odontologo", x => x.id_odontologo);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "paciente",
                columns: table => new
                {
                    id_paciente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apellido = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dni = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    direccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    obra_social = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numero_afiliado = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paciente", x => x.id_paciente);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tratamiento",
                columns: table => new
                {
                    id_tratamiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    costo = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    duracion_estimada = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tratamiento", x => x.id_tratamiento);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "turno",
                columns: table => new
                {
                    id_turno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hora = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    duracion = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observaciones = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_paciente = table.Column<int>(type: "int", nullable: false),
                    id_odontologo = table.Column<int>(type: "int", nullable: false),
                    id_consultorio = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turno", x => x.id_turno);
                    table.ForeignKey(
                        name: "FK_turno_consultorio_id_consultorio",
                        column: x => x.id_consultorio,
                        principalTable: "consultorio",
                        principalColumn: "id_consultorio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_turno_odontologo_id_odontologo",
                        column: x => x.id_odontologo,
                        principalTable: "odontologo",
                        principalColumn: "id_odontologo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_turno_paciente_id_paciente",
                        column: x => x.id_paciente,
                        principalTable: "paciente",
                        principalColumn: "id_paciente",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "turno_tratamiento",
                columns: table => new
                {
                    id_tratamiento = table.Column<int>(type: "int", nullable: false),
                    id_turno = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turno_tratamiento", x => new { x.id_tratamiento, x.id_turno });
                    table.ForeignKey(
                        name: "FK_turno_tratamiento_tratamiento_id_tratamiento",
                        column: x => x.id_tratamiento,
                        principalTable: "tratamiento",
                        principalColumn: "id_tratamiento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_turno_tratamiento_turno_id_turno",
                        column: x => x.id_turno,
                        principalTable: "turno",
                        principalColumn: "id_turno",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_odontologo_dni",
                table: "odontologo",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_odontologo_email",
                table: "odontologo",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_odontologo_matricula",
                table: "odontologo",
                column: "matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paciente_dni",
                table: "paciente",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paciente_email",
                table: "paciente",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tratamiento_codigo",
                table: "tratamiento",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_turno_id_consultorio_fecha_hora",
                table: "turno",
                columns: new[] { "id_consultorio", "fecha", "hora" });

            migrationBuilder.CreateIndex(
                name: "IX_turno_id_odontologo_fecha_hora",
                table: "turno",
                columns: new[] { "id_odontologo", "fecha", "hora" });

            migrationBuilder.CreateIndex(
                name: "IX_turno_id_paciente",
                table: "turno",
                column: "id_paciente");

            migrationBuilder.CreateIndex(
                name: "IX_turno_tratamiento_id_turno",
                table: "turno_tratamiento",
                column: "id_turno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "turno_tratamiento");

            migrationBuilder.DropTable(
                name: "tratamiento");

            migrationBuilder.DropTable(
                name: "turno");

            migrationBuilder.DropTable(
                name: "consultorio");

            migrationBuilder.DropTable(
                name: "odontologo");

            migrationBuilder.DropTable(
                name: "paciente");
        }
    }
}
