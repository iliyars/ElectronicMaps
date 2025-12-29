using ElectronicMaps.Documents.Abstractions.Models;
using ElectronicMaps.Documents.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.OpenXmlWord.Engine
{
    public class OpenXmlWordReader : IWordRender
    {
        private readonly ITemplateStore _templateStore;
        private readonly JsonShemasStore _schemasStore;
        private readonly GenericStdFormRender _formRender;


        public Task<DocumentBuildResult> BuildAsync(DocumentBuildRequest request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
