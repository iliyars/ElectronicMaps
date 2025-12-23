using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Tests.Infrastructure.Presistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Tests.Infrastructure.Commands
{
    public class EfSaveComponentTest : IClassFixture<SqliteDbFixture>
    {

        private readonly SqliteDbFixture _fx;

        public EfSaveComponentTest(SqliteDbFixture fx)
        {
            _fx = fx;
        }

        public async Task ExecuteAsync_creates_family_component_and_parameter_value()
        {
            await using var db = await _fx.CreateDbAsync();

            // ШАГ 1. Seed: формы (FormTypes)
            // EfSaveComponent резолвит FormTypeId по Code, поэтому FormTypes обязаны быть в БД.
            var form4 = new FormType { Code = "FORM_4", DisplayName = "Форма 4" };
            var form64 = new FormType { Code = "FORM_64", DisplayName = "Форма 64" };

            db.FormTypes.AddRange(form4, form64);
            await db.SaveChangesAsync();


            //ШАГ 2. Seed: определения параметров (ParameterDefinitions)
            // Эти Id потом будут в ParameterValueInput.

        }
    }
