using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Duckmension_Website.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CookieCount",
                table: "UserProfiles",
                newName: "Cookies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cookies",
                table: "UserProfiles",
                newName: "CookieCount");
        }
    }
}
