using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicMaps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToFormType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateKey",
                table: "FormTypes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FormTypes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "FormTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "");

            migrationBuilder.UpdateData(
                table: "FormTypes",
                keyColumn: "Id",
                keyValue: 68,
                column: "Description",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "FormTypes");

            migrationBuilder.AddColumn<string>(
                name: "TemplateKey",
                table: "FormTypes",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "FormTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TemplateKey",
                value: null);

            migrationBuilder.UpdateData(
                table: "FormTypes",
                keyColumn: "Id",
                keyValue: 68,
                column: "TemplateKey",
                value: null);
        }
    }
}
