using ElectronicMaps.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace ElectronicMaps.Tests.Parsers
{
    public class ComponentNameParserTest
    {
        private readonly ComponentNameParser _parser = new();


        [Fact]
        public void Parse_ShouldExtractTypeFamilyName()
        {
            var raw = "Конденсатор К10-79-25 В-0,1 мкФ+80%-20%-Н90 АЖЯР.673511.004ТУ";

            var result = _parser.Parse(raw);

            Assert.Equal("Конденсатор", result.Type);
            Assert.Equal("К10-79", result.Family);
            Assert.StartsWith("К10-79-25", result.Name);
        }


    }
}
