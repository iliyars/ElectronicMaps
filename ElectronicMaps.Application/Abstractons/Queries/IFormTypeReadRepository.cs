using ElectronicMaps.Application.DTO.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    public interface IFormTypeReadRepository
    {
        Task<FormTypeDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<FormTypeDto?> GetByCodeAsync(string code, CancellationToken ct);

        Task<IReadOnlyList<FormTypeDto>> GetAllAsync(CancellationToken ct);



    }
}
