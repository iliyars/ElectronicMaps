using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicMaps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationAndFixParameterTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentFamilyId",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentId",
                table: "ParameterValues");

            migrationBuilder.AlterColumn<int>(
                name: "ComponentId",
                table: "ParameterValues",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ParameterValues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "ParameterValues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Components",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Components",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Components",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Components",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNote",
                table: "Components",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "Components",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VerifiedAt",
                table: "Components",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByUserId",
                table: "Components",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Components",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "ComponentFamilies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "ComponentFamilies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ComponentFamilies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "ComponentFamilies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNote",
                table: "ComponentFamilies",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "ComponentFamilies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VerifiedAt",
                table: "ComponentFamilies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByUserId",
                table: "ComponentFamilies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ComponentFamilies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentFamilyId_ParameterDefinitionId",
                table: "ParameterValues",
                columns: new[] { "ComponentFamilyId", "ParameterDefinitionId" },
                unique: true,
                filter: "[ComponentFamilyId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentId_ParameterDefinitionId",
                table: "ParameterValues",
                columns: new[] { "ComponentId", "ParameterDefinitionId" },
                unique: true,
                filter: "[ComponentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Components_VerificationStatus",
                table: "Components",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Components_VerifiedByUserId",
                table: "Components",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentFamilies_Name",
                table: "ComponentFamilies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentFamilies_VerificationStatus",
                table: "ComponentFamilies",
                column: "VerificationStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Components_Users_VerifiedByUserId",
                table: "Components",
                column: "VerifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Components_Users_VerifiedByUserId",
                table: "Components");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentFamilyId_ParameterDefinitionId",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentId_ParameterDefinitionId",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_Components_VerificationStatus",
                table: "Components");

            migrationBuilder.DropIndex(
                name: "IX_Components_VerifiedByUserId",
                table: "Components");

            migrationBuilder.DropIndex(
                name: "IX_ComponentFamilies_Name",
                table: "ComponentFamilies");

            migrationBuilder.DropIndex(
                name: "IX_ComponentFamilies_VerificationStatus",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ParameterValues");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ParameterValues");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "VerificationNote",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "VerificationNote",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "ComponentFamilies");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ComponentFamilies");

            migrationBuilder.AlterColumn<int>(
                name: "ComponentId",
                table: "ParameterValues",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentFamilyId",
                table: "ParameterValues",
                column: "ComponentFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentId",
                table: "ParameterValues",
                column: "ComponentId");
        }
    }
}
