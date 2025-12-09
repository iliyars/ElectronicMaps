using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Updates
{
    public class UpdateInfo
    {
        public Version CurrentVersion { get; init; } = default!;
        public Version LatestVersion { get; init; } = default!;
        public string? PackageName { get; init; }
        public string? ReleaseNotes { get; init; }



    }
}
