using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.API.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippingAddress_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_StreetLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_StreetLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress_PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_StreetLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_StreetLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress_PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payment_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Payment_Method = table.Column<int>(type: "int", nullable: false),
                    Payment_Status = table.Column<int>(type: "int", nullable: false),
                    Payment_Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payment_TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payment_Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payment_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Payment_AuthorizedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payment_CapturedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
