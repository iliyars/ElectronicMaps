using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectronicMaps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialFormsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentFamilies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    FamilyFormCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentFamilies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Scope = table.Column<int>(type: "INTEGER", nullable: false),
                    TemplateKey = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WindowsIdentity = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Components",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComponentFamilyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FormCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CanonicalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Components_ComponentFamilies_ComponentFamilyId",
                        column: x => x.ComponentFamilyId,
                        principalTable: "ComponentFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParameterDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FormTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ValueKind = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Group = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterDefinitions_FormTypes_FormTypeId",
                        column: x => x.FormTypeId,
                        principalTable: "FormTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParameterValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParameterDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComponentFamilyId = table.Column<int>(type: "INTEGER", nullable: true),
                    ComponentId = table.Column<int>(type: "INTEGER", nullable: true),
                    StringValue = table.Column<string>(type: "TEXT", nullable: true),
                    DoubleValue = table.Column<double>(type: "REAL", nullable: true),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: true),
                    Pins = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParameterValues_ComponentFamilies_ComponentFamilyId",
                        column: x => x.ComponentFamilyId,
                        principalTable: "ComponentFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParameterValues_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParameterValues_ParameterDefinitions_ParameterDefinitionId",
                        column: x => x.ParameterDefinitionId,
                        principalTable: "ParameterDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FormTypes",
                columns: new[] { "Id", "Code", "DisplayName", "Scope", "TemplateKey" },
                values: new object[,]
                {
                    { 4, "FORM_4", "Форма 4", 0, null },
                    { 68, "FORM_68", "Форма 68", 0, null }
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
                name: "IX_Components_CanonicalName",
                table: "Components",
                column: "CanonicalName");

            migrationBuilder.CreateIndex(
                name: "IX_Components_ComponentFamilyId",
                table: "Components",
                column: "ComponentFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_Name",
                table: "Components",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_FormTypes_Code",
                table: "FormTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_FormTypeId",
                table: "ParameterDefinitions",
                column: "FormTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentFamilyId",
                table: "ParameterValues",
                column: "ComponentFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ComponentId",
                table: "ParameterValues",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterValues_ParameterDefinitionId",
                table: "ParameterValues",
                column: "ParameterDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WindowsIdentity",
                table: "Users",
                column: "WindowsIdentity",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParameterValues");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Components");

            migrationBuilder.DropTable(
                name: "ParameterDefinitions");

            migrationBuilder.DropTable(
                name: "ComponentFamilies");

            migrationBuilder.DropTable(
                name: "FormTypes");
        }
    }
}
