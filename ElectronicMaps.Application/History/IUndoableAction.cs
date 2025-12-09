using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.History
{
    public interface IUndoableAction
    {
        string Description { get; }

        void Do();
        void Undo();


    }
}
