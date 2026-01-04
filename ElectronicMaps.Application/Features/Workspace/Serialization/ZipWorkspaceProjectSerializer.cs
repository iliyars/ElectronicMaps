using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Workspace.Serialization
{
    public class ZipWorkspaceProjectSerializer : IWorkspaceProjectSerializer
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public async Task SaveAsync(string filePath, Models.WorkspaceProject project, IReadOnlyCollection<WordDocumentBinary> docs, CancellationToken ct)
        {
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);

            //mainfest.json
            //await WriteJson(zip, "mainfest.json", project.Meta, ct);

            ////components.json
            //await WriteJson(zip, "components/components.json", new
            //{
            //    project.Components,
            //    project.Forms,
            //    project.WordDocuments
            //}, ct);

            //// docs/index.json 
            //await WriteJson(zip, "docs/index.json", project.WordDocuments, ct);

            //// docks binaries
            //foreach(var doc in docs)
            //{
            //    ct.ThrowIfCancellationRequested();
            //    var enrty = zip.CreateEntry($"docs/{doc.DocumentId}.docx", CompressionLevel.Optimal);
            //    await using var entryStream = enrty.Open();
            //    await entryStream.WriteAsync(doc.Content, ct);
            //}

                
        }


        public async Task<WorkspaceProjectLoadResult> LoadAsync(string filePath, CancellationToken ct)
        {
            //using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //using var zip = new ZipArchive(fs, ZipArchiveMode.Read);

            //var meta = await ReadJson<ProjectMeta>(zip, "mainfest.json", ct);

            //// читаем components/comonents.json
            //var payload = await ReadJson<ProjectPayLoad>(zip, "components/components.json", ct);

            //var project = new WorkspaceProject.Models.WorkspaceProject(
            //    Meta: meta,
            //    Components: payload.Components,
            //    Forms: payload.Forms,
            //    WordDocuments: payload.WordDocuments
            //);

            //// читаем docs/index.json
            //var docs = new List<WordDocumentBinary>();
            //foreach(var docRef in project.WordDocuments)
            //{
            //    var entry = zip.GetEntry(docRef.PackagePath);
            //    if(entry is null)
            //    {
            //        continue;
            //    }

            //    await using var s = entry.Open();
            //    using var ms = new MemoryStream();
            //    await s.CopyToAsync(ms, ct);
            //    docs.Add(new WordDocumentBinary(docRef.DocumentId, ms.ToArray()));
            //}

           // return new WorkspaceProjectLoadResult(project, docs);
           throw new NotImplementedException();
        }


        private static async Task WriteJson<T>(ZipArchive zip, string path, T obj, CancellationToken ct)
        {
            var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
            await using var s = entry.Open();
            await JsonSerializer.SerializeAsync(s, obj, JsonOptions, ct);
        }

        private static async Task<T> ReadJson<T>(ZipArchive zip, string path, CancellationToken ct)
        {
            var entry = zip.GetEntry(path) ?? throw new InvalidDataException($"Missimg entry {path}");
            await using var s = entry.Open();
            var obj = await JsonSerializer.DeserializeAsync<T>(s, JsonOptions, ct);
            return obj ?? throw new InvalidDataException($"Failed to deserialize: {path}");
        }

        private sealed record ProjectPayload(
      //  IReadOnlyList<ComponentDraft> Components,
      //  IReadOnlyDictionary<string, FormViewState> Forms,
      //  IReadOnlyList<WordDocumentRef> WordDocuments
    );

    }
}
