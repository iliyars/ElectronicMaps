using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectronicMaps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    { 1, "FORM_RESISTOR", "Форма резистора", 1, null },
                    { 2, "FORM_CHIP", "Форма микросхемы", 1, null },
                    { 3, "FORM_64", "Форма 64", 1, null }
                });

            migrationBuilder.InsertData(
                table: "ParameterDefinitions",
                columns: new[] { "Id", "Code", "DisplayName", "FormTypeId", "Group", "Order", "Unit", "ValueKind" },
                values: new object[,]
                {
                    { 1, "DcVoltage", "Постоянное напряжение", 1, null, 1, "В", 3 },
                    { 2, "AcVoltage", "Переменное напряжение", 1, null, 2, "В", 3 },
                    { 3, "ImpulseVoltage", "Импульсное напряжение", 1, null, 3, "В", 3 },
                    { 4, "SumVoltage", "Суммарное напряжение", 1, null, 4, "В", 3 },
                    { 5, "Frequancy", "Частота", 1, null, 5, "Гц", 2 },
                    { 6, "ImpulseDuration", "Длительность импульса", 1, null, 6, null, 3 },
                    { 7, "ImpulsePower", "Импульсная мощность", 1, null, 7, null, 3 },
                    { 8, "MeanPower", "Средняя мощность", 1, null, 8, null, 3 },
                    { 9, "LoadKoeffImpulse", "Коэффициент нагрузки (импульс)", 1, null, 9, null, 3 },
                    { 10, "CurrentMovingContact", "Ток через подвижный контакт", 1, null, 10, "А", 3 },
                    { 11, "AmbientTemperature", "Температура окружающей среды", 1, null, 11, "°C", 2 },
                    { 12, "SuperHeatTemperature", "Температура перегрева", 1, null, 12, "°C", 3 },
                    { 13, "SumPower", "Суммарная мощность", 1, null, 13, null, 3 },
                    { 14, "AmbientTemperatureCase", "Температура корпуса", 1, null, 14, "°C", 2 },
                    { 15, "LoadKoeff", "Коэффициент нагрузки", 1, null, 15, null, 1 },
                    { 16, "SupplayVoltage", "Напряжение питания", 2, null, 1, "В", 4 },
                    { 17, "SupplyOrder", "Порядок подачи напряжения питания", 2, null, 2, null, 4 },
                    { 18, "LowLevelVolatge", "Напряжение низкого уровня", 2, null, 3, "В", 4 },
                    { 19, "HighLevelVolatge", "Напряжение высокого уровня", 2, null, 4, "В", 4 },
                    { 20, "ImpulseDuration", "Длительность импульса", 2, null, 5, "нс", 4 },
                    { 21, "TurnOnTrasnsition", "Время перехода при включении", 2, null, 6, "нс", 4 },
                    { 22, "TurnOffTransition", "Время перехода при выключении", 2, null, 7, "нс", 4 },
                    { 23, "Frequency", "Частота", 2, null, 8, "МГц", 4 },
                    { 24, "Timet1", "Время t1", 2, null, 9, "нс", 4 },
                    { 25, "Timet2", "Время t2", 2, null, 10, "нс", 4 },
                    { 26, "OutCurrentLowLevel", "Выходной ток низкого уровня", 2, null, 11, "мА", 4 },
                    { 27, "OutCurrentHighLevel", "Выходной ток высокого уровня", 2, null, 12, "мА", 4 },
                    { 28, "CapacityLoad", "Ёмкость нагрузки", 2, null, 13, "пФ", 4 },
                    { 29, "PowerDissipation", "Мощность рассеивания", 2, null, 14, "мВт", 4 },
                    { 30, "PosName", "Позиционное обозначение и номера выводов", 2, null, 15, null, 4 },
                    { 31, "AmbientTemperatureCase", "Температура корпуса", 2, null, 16, "°C", 2 },
                    { 32, "LoadKoeff", "Коэффициент нагрузки", 2, null, 17, null, 1 },
                    { 33, "SupplayVoltage", "Напряжение питания", 3, null, 1, "В", 4 },
                    { 34, "SupplyOrder", "Порядок подачи напряжения питания", 3, null, 2, null, 4 },
                    { 35, "LowLevelVolatge", "Напряжение низкого уровня", 3, null, 3, "В", 4 },
                    { 36, "HighLevelVolatge", "Напряжение высокого уровня", 3, null, 4, "В", 4 },
                    { 37, "ImpulseDuration", "Длительность импульса", 3, null, 5, "нс", 4 },
                    { 38, "TurnOnTrasnsition", "Время перехода при включении", 3, null, 6, "нс", 4 },
                    { 39, "TurnOffTransition", "Время перехода при выключении", 3, null, 7, "нс", 4 },
                    { 40, "Frequency", "Частота", 3, null, 8, "МГц", 4 },
                    { 41, "Timet1", "Время t1", 3, null, 9, "нс", 4 },
                    { 42, "Timet2", "Время t2", 3, null, 10, "нс", 4 },
                    { 43, "OutCurrentLowLevel", "Выходной ток низкого уровня", 3, null, 11, "мА", 4 },
                    { 44, "OutCurrentHighLevel", "Выходной ток высокого уровня", 3, null, 12, "мА", 4 },
                    { 45, "CapacityLoad", "Ёмкость нагрузки", 3, null, 13, "пФ", 4 },
                    { 46, "PowerDissipation", "Мощность рассеивания", 3, null, 14, "мВт", 4 },
                    { 47, "AmbientTemperatureCase", "Температура корпуса", 3, null, 15, "°C", 4 },
                    { 48, "PosName", "Позиционное обозначение и номера выводов", 3, null, 16, null, 4 },
                    { 49, "LoadKoeff", "Коэффициент нагрузки", 3, null, 17, null, 4 }
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParameterValues");

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
