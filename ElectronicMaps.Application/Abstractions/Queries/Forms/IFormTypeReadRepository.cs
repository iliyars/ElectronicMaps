using ElectronicMaps.Application.DTOs.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Forms
{
    public interface IFormTypeReadRepository
    {
        Task<FormTypeDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<FormTypeDto?> GetByCodeAsync(string code, CancellationToken ct);

        Task<IReadOnlyList<FormTypeDto>> GetAllAsync(CancellationToken ct);



    }
}
