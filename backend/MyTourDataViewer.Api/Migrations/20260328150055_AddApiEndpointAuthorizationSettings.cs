using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTourDataViewer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddApiEndpointAuthorizationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiEndpointSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiSettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    HttpMethod = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RequiresAuthorization = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthorizationType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TokenEndpointUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ClientSecret = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiEndpointSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiEndpointSettings_ApiSettings_ApiSettingsId",
                        column: x => x.ApiSettingsId,
                        principalTable: "ApiSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiEndpointHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiEndpointSettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiEndpointHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiEndpointHeaders_ApiEndpointSettings_ApiEndpointSettingsId",
                        column: x => x.ApiEndpointSettingsId,
                        principalTable: "ApiEndpointSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiEndpointHeaders_ApiEndpointSettingsId",
                table: "ApiEndpointHeaders",
                column: "ApiEndpointSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiEndpointSettings_ApiSettingsId",
                table: "ApiEndpointSettings",
                column: "ApiSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiEndpointHeaders");

            migrationBuilder.DropTable(
                name: "ApiEndpointSettings");
        }
    }
}
