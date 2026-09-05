using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RauchERP.Modules.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_receipts",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_receipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_receipt_lines",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    NewSalePrice = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_receipt_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_receipt_lines_purchase_receipts_PurchaseReceiptId",
                        column: x => x.PurchaseReceiptId,
                        principalSchema: "inventory",
                        principalTable: "purchase_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipt_lines_PurchaseReceiptId",
                schema: "inventory",
                table: "purchase_receipt_lines",
                column: "PurchaseReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_receipts_CompanyId_WarehouseId_ReceivedAt",
                schema: "inventory",
                table: "purchase_receipts",
                columns: new[] { "CompanyId", "WarehouseId", "ReceivedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_receipt_lines",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "purchase_receipts",
                schema: "inventory");
        }
    }
}
