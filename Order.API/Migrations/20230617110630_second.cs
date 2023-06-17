using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.API.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MyProperty",
                table: "Orders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Address_Provider",
                table: "Orders",
                newName: "Address_Province");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Orders",
                newName: "MyProperty");

            migrationBuilder.RenameColumn(
                name: "Address_Province",
                table: "Orders",
                newName: "Address_Provider");
        }
    }
}
