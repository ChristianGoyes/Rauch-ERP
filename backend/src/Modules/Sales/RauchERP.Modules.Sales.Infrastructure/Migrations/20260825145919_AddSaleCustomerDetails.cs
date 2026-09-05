using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RauchERP.Modules.Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleCustomerDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                schema: "sales",
                table: "sales",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerIdentification",
                schema: "sales",
                table: "sales",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalConsumer",
                schema: "sales",
                table: "sales",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                schema: "sales",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "CustomerIdentification",
                schema: "sales",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "IsFinalConsumer",
                schema: "sales",
                table: "sales");
        }
    }
}
