using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectronicMaps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParameterValueRemark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentFamilyRemark");

            migrationBuilder.DropTable(
                name: "ComponentRemark");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentFamilyId_ParameterDefinitionId",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentId_ParameterDefinitionId",
                table: "ParameterValues");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ParameterValues_Target",
                table: "ParameterValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Remark",
                table: "Remark");

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "ParameterDefinitions",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "FormTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "FormTypes",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DropColumn(
                name: "Group",
                table: "ParameterDefinitions");

            migrationBuilder.RenameTable(
                name: "Remark",
                newName: "Remarks");

            migrationBuilder.RenameIndex(
                name: "IX_Remark_Text",
                table: "Remarks",
                newName: "IX_Remarks_Text");

            migrationBuilder.AlterColumn<double>(
                name: "DoubleValue",
                table: "ParameterValues",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParameterDefinitionId1",
                table: "ParameterValues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DataType",
                table: "ParameterDefinitions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "ParameterDefinitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxValue",
                table: "ParameterDefinitions",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinValue",
                table: "ParameterDefinitions",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationMessage",
                table: "ParameterDefinitions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationPattern",
                table: "ParameterDefinitions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Remarks",
                table: "Remarks",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ParameterValueRemarks",
                columns: table => new
                {
                    ParameterValueId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemarkId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterValueRemarks", x => new { x.ParameterValueId, x.RemarkId });
                    table.ForeignKey(
                        name: "FK_ParameterValueRemarks_ParameterValues_ParameterValueId",
                        column: x => x.ParameterValueId,
                        principalTable: "ParameterValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParameterValueRemarks_Remarks_RemarkId",
                        column: x => x.RemarkId,
                        principalTable: "Remarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentFamilyId_ParameterDefinitionId",
                table: "ParameterValues",
                columns: new[] { "ComponentFamilyId", "ParameterDefinitionId" },
                unique: true,
                filter: "\"ComponentFamilyId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentId_ParameterDefinitionId",
                table: "ParameterValues",
                columns: new[] { "ComponentId", "ParameterDefinitionId" },
                unique: true,
                filter: "\"ComponentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ParameterDefinitionId1",
                table: "ParameterValues",
                column: "ParameterDefinitionId1");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_UpdatedByUserId",
                table: "ParameterValues",
                column: "UpdatedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParameterValues_Target",
                table: "ParameterValues",
                sql: "((\"ComponentId\" IS NOT NULL AND \"ComponentFamilyId\" IS NULL) OR (\"ComponentId\" IS NULL AND \"ComponentFamilyId\" IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_FormTypeId_Code",
                table: "ParameterDefinitions",
                columns: new[] { "FormTypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValueRemarks_RemarkId",
                table: "ParameterValueRemarks",
                column: "RemarkId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParameterValues_ParameterDefinitions_ParameterDefinitionId1",
                table: "ParameterValues",
                column: "ParameterDefinitionId1",
                principalTable: "ParameterDefinitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParameterValues_Users_UpdatedByUserId",
                table: "ParameterValues",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParameterValues_ParameterDefinitions_ParameterDefinitionId1",
                table: "ParameterValues");

            migrationBuilder.DropForeignKey(
                name: "FK_ParameterValues_Users_UpdatedByUserId",
                table: "ParameterValues");

            migrationBuilder.DropTable(
                name: "ParameterValueRemarks");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentFamilyId_ParameterDefinitionId",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ComponentId_ParameterDefinitionId",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_ParameterDefinitionId1",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_ParameterValues_UpdatedByUserId",
                table: "ParameterValues");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ParameterValues_Target",
                table: "ParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_ParameterDefinitions_FormTypeId_Code",
                table: "ParameterDefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Remarks",
                table: "Remarks");

            migrationBuilder.DropColumn(
                name: "ParameterDefinitionId1",
                table: "ParameterValues");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "ParameterDefinitions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "ParameterDefinitions");

            migrationBuilder.DropColumn(
                name: "MaxValue",
                table: "ParameterDefinitions");

            migrationBuilder.DropColumn(
                name: "MinValue",
                table: "ParameterDefinitions");

            migrationBuilder.DropColumn(
                name: "ValidationMessage",
                table: "ParameterDefinitions");

            migrationBuilder.DropColumn(
                name: "ValidationPattern",
                table: "ParameterDefinitions");

            migrationBuilder.RenameTable(
                name: "Remarks",
                newName: "Remark");

            migrationBuilder.RenameIndex(
                name: "IX_Remarks_Text",
                table: "Remark",
                newName: "IX_Remark_Text");

            migrationBuilder.AlterColumn<double>(
                name: "DoubleValue",
                table: "ParameterValues",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "ParameterDefinitions",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Remark",
                table: "Remark",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ComponentFamilyRemark",
                columns: table => new
                {
                    ComponentFamilyId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemarkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentFamilyRemark", x => new { x.ComponentFamilyId, x.RemarkId });
                    table.ForeignKey(
                        name: "FK_ComponentFamilyRemark_ComponentFamilies_ComponentFamilyId",
                        column: x => x.ComponentFamilyId,
                        principalTable: "ComponentFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponentFamilyRemark_Remark_RemarkId",
                        column: x => x.RemarkId,
                        principalTable: "Remark",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponentRemark",
                columns: table => new
                {
                    ComponentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemarkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentRemark", x => new { x.ComponentId, x.RemarkId });
                    table.ForeignKey(
                        name: "FK_ComponentRemark_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponentRemark_Remark_RemarkId",
                        column: x => x.RemarkId,
                        principalTable: "Remark",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FormTypes",
                columns: new[] { "Id", "Code", "Description", "DisplayName" },
                values: new object[,]
                {
                    { 4, "FORM_4", "", "Форма 4" },
                    { 68, "FORM_68", "", "Форма 68" }
                });

            migrationBuilder.InsertData(
                table: "ParameterDefinitions",
                columns: new[] { "Id", "Code", "DisplayName", "FormTypeId", "Group", "Order", "Unit", "ValueKind" },
                values: new object[,]
                {
                    { 1, "DcVoltage", "Постоянное напряжение", 68, null, 1, "В", 3 },
                    { 2, "AcVoltage", "Переменное напряжение", 68, null, 2, "В", 3 },
                    { 3, "ImpulseVoltage", "Импульсное напряжение", 68, null, 3, "В", 3 },
                    { 4, "SumVoltage", "Суммарное напряжение", 68, null, 4, "В", 3 },
                    { 5, "Frequancy", "Частота", 68, null, 5, "Гц", 2 },
                    { 6, "ImpulseDuration", "Длительность импульса", 68, null, 6, null, 3 },
                    { 7, "ImpulsePower", "Импульсная мощность", 68, null, 7, null, 3 },
                    { 8, "MeanPower", "Средняя мощность", 68, null, 8, null, 3 },
                    { 9, "LoadKoeffImpulse", "Коэффициент нагрузки (импульсный режим)", 68, null, 9, null, 3 },
                    { 10, "CurrentMovingContact", "Ток через подвижный контакт", 68, null, 10, "А", 3 },
                    { 11, "AmbientTemperature", "Температура окружающей среды", 68, null, 11, "°C", 2 },
                    { 12, "SuperHeatTemperature", "Температура перегрева", 68, null, 12, "°C", 3 },
                    { 13, "SumPower", "Суммарная мощность", 68, null, 13, null, 3 },
                    { 14, "AmbientTemperatureCase", "Температура корпуса", 68, null, 14, "°C", 2 },
                    { 15, "LoadKoeff", "Коэффициент нагрузки", 68, null, 15, null, 1 },
                    { 16, "InListTTZ", "Наличие в перечнях при утверждении ТТЗ", 4, null, 1, null, 1 },
                    { 17, "LastEditions", "Наличие в перечнях последних редакций", 4, null, 2, null, 1 },
                    { 18, "ResourceHours", "Показатель ресурса, ч", 4, null, 3, "ч", 1 },
                    { 19, "LifeTimeYears", "Показатель срока службы, лет", 4, null, 4, "лет", 1 },
                    { 20, "PreservationYears", "Показатель сохраняемости, лет", 4, null, 5, "лет", 1 },
                    { 21, "FrequencyRange", "Диапазон частот, Гц", 4, null, 6, "Гц", 1 },
                    { 22, "SoundPressure", "Уровень звукового давления, дБ", 4, null, 7, "дБ", 1 },
                    { 23, "LineAcceleration", "Линейное ускорение, м·с⁻² (G)", 4, null, 8, null, 1 },
                    { 24, "LowPressure", "Давление окр. среды пониженное", 4, null, 9, null, 1 },
                    { 25, "HighPressure", "Давление окр. среды повышенное", 4, null, 10, null, 1 },
                    { 26, "LowTemperature", "Предельная температура пониженная, °C", 4, null, 11, "°C", 1 },
                    { 27, "HighTemperature", "Предельная температура повышенная, °C", 4, null, 12, "°C", 1 },
                    { 28, "HumidityPercent", "Относительная влажность, %", 4, null, 13, "%", 1 },
                    { 29, "HumidityCelcius", "Температура при заданной влажности, °C", 4, null, 14, "°C", 1 },
                    { 30, "Dew", "Роса, иней", 4, null, 15, null, 1 },
                    { 31, "SpecialFactors", "Стойкость к ВССФ", 4, null, 16, null, 1 },
                    { 32, "Note", "Примечание", 4, null, 17, null, 1 }
                });

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_ParameterValues_Target",
                table: "ParameterValues",
                sql: "(([ComponentId] IS NOT NULL AND [ComponentFamilyId] IS NULL) OR ([ComponentId] IS NULL AND [ComponentFamilyId] IS NOT NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentFamilyRemark_RemarkId",
                table: "ComponentFamilyRemark",
                column: "RemarkId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentRemark_RemarkId",
                table: "ComponentRemark",
                column: "RemarkId");
        }
    }
}
