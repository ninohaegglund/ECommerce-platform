using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentNotificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                table: "Payments",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                table: "Payments");
        }
    }
}
