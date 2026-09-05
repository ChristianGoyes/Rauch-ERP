using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RauchERP.Modules.Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateTable(
                name: "sales",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sale_lines",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Sku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sale_lines_sales_SaleId",
                        column: x => x.SaleId,
                        principalSchema: "sales",
                        principalTable: "sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sale_lines_ProductVariantId",
                schema: "sales",
                table: "sale_lines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_sale_lines_SaleId",
                schema: "sales",
                table: "sale_lines",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_CompanyId_Number",
                schema: "sales",
                table: "sales",
                columns: new[] { "CompanyId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sale_lines",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "sales",
                schema: "sales");
        }
    }
}
