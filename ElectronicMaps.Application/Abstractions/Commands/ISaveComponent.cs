using ElectronicMaps.Application.DTOs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Commands
{
    public interface ISaveComponent
    {
        Task<SaveComponentResult> ExecuteAsync(SaveComponentRequest request, CancellationToken ct);
    }
}
