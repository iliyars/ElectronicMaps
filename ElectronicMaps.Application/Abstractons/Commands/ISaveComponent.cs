using ElectronicMaps.Application.DTO.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Commands
{
    public interface ISaveComponent
    {
        Task<SaveComponentResult> ExecuteAsync(SaveComponentRequest request, CancellationToken ct);
    }
}
