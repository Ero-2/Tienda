using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsPagos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pagos");

            migrationBuilder.CreateTable(
                name: "pagos",
                schema: "pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MesesMsi = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdTransaccionOpenpay = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenciaOpenpay = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RespuestaOpenpay = table.Column<string>(type: "text", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcesadoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagos",
                schema: "pagos");
        }
    }
}
