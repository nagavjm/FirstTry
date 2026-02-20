using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstTry.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChildApplicationsAndAuthType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthenticationType",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChildApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LaunchUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserApplicationAccesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChildApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GrantedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplicationAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserApplicationAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserApplicationAccesses_ChildApplications_ChildApplicationId",
                        column: x => x.ChildApplicationId,
                        principalTable: "ChildApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildApplications_Name",
                table: "ChildApplications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserApplicationAccesses_ChildApplicationId",
                table: "UserApplicationAccesses",
                column: "ChildApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplicationAccesses_UserId_ChildApplicationId",
                table: "UserApplicationAccesses",
                columns: new[] { "UserId", "ChildApplicationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserApplicationAccesses");

            migrationBuilder.DropTable(
                name: "ChildApplications");

            migrationBuilder.DropColumn(
                name: "AuthenticationType",
                table: "AspNetUsers");
        }
    }
}
