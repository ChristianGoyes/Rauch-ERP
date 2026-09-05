using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RauchERP.Modules.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitOfMeasure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                schema: "catalog",
                table: "product_variants",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Unidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                schema: "catalog",
                table: "product_variants");
        }
    }
}
