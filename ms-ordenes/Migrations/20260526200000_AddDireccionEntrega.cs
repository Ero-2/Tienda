using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsOrdenes.Migrations
{
    public partial class AddDireccionEntrega : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionEntrega",
                schema: "ordenes",
                table: "ordenes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DireccionEntrega",
                schema: "ordenes",
                table: "ordenes");
        }
    }
}
