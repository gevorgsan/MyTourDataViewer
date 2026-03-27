using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTourDataViewer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeoutToApiSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeoutSeconds",
                table: "ApiSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeoutSeconds",
                table: "ApiSettings");
        }
    }
}
