using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTourDataViewer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorizationFieldsToApiSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizationType",
                table: "ApiSettings",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CredentialsPayload",
                table: "ApiSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenUrl",
                table: "ApiSettings",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizationType",
                table: "ApiSettings");

            migrationBuilder.DropColumn(
                name: "CredentialsPayload",
                table: "ApiSettings");

            migrationBuilder.DropColumn(
                name: "TokenUrl",
                table: "ApiSettings");
        }
    }
}
