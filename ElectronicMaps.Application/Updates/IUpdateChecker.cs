using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Updates
{
    public interface IUpdateChecker
    {
        Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken ct = default);
    }
}
