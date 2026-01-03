using ElectronicMaps.Documents.Core.Models;
using ElectronicMaps.Documents.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Templates.Store
{
    public class FileSystemTemplateStore : ITemplateStore
    {
        private readonly string _templatesRoot;

        public FileSystemTemplateStore(string templatesRoot)
        {
            _templatesRoot = templatesRoot;
        }

        public Stream OpenTemplate(TemplateId id)
        {
            var version = string.IsNullOrWhiteSpace(id.Version) ? "v1" : id.Version!;
            var path = Path.Combine(_templatesRoot, "Word", id.FormCode, $"{version}.docx");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Template not found: {path}", path);
            }

            return File.OpenRead(path);
        }
    }
}
