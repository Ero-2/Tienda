using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsOrdenes.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "idempotencias",
                schema: "ordenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotencias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_idempotencias_IdempotencyKey",
                schema: "ordenes",
                table: "idempotencias",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotencias",
                schema: "ordenes");
        }
    }
}
