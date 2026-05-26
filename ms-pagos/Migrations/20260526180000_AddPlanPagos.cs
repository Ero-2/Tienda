using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsPagos.Migrations
{
    public partial class AddPlanPagos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "pagos");

            migrationBuilder.CreateTable(
                name: "plan_pagos",
                schema: "pagos",
                columns: table => new
                {
                    Id              = table.Column<int>(nullable: false)
                                          .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenId         = table.Column<int>(nullable: false),
                    UsuarioId       = table.Column<int>(nullable: false),
                    TotalFinanciado = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MesesMsi        = table.Column<int>(nullable: false),
                    CuotaMensual    = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    CuotasPagadas   = table.Column<int>(nullable: false, defaultValue: 1),
                    Estado          = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "activo"),
                    CreadoEn        = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_pagos", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "plan_pagos", schema: "pagos");
        }
    }
}
