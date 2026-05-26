using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsPagos.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutTarjeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenCheckout",
                schema: "pagos",
                table: "pagos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TarjetaEnmascarada",
                schema: "pagos",
                table: "pagos",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarcaTarjeta",
                schema: "pagos",
                table: "pagos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagos_TokenCheckout",
                schema: "pagos",
                table: "pagos",
                column: "TokenCheckout");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pagos_TokenCheckout",
                schema: "pagos",
                table: "pagos");

            migrationBuilder.DropColumn(name: "TokenCheckout",      schema: "pagos", table: "pagos");
            migrationBuilder.DropColumn(name: "TarjetaEnmascarada", schema: "pagos", table: "pagos");
            migrationBuilder.DropColumn(name: "MarcaTarjeta",       schema: "pagos", table: "pagos");
        }
    }
}
