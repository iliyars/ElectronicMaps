using ElectronicMaps.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Features.Forms
{
    public interface IFormComponentService
    {
        IReadOnlyList<AnalyzedComponentDto> GetComponents(string formCode);

        void MergeByName(string formCode, string name, string description);
        bool CanUndo(string formCode);
        bool CanRedo(string formCode);

        bool Undo(string formCode);
        bool Redo(string formCode);



    }
}
