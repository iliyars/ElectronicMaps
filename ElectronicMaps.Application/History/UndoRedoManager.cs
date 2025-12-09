using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.History
{
    public sealed class UndoRedoManager
    {
        private readonly Stack<IUndoableAction> _undo = new();
        private readonly Stack<IUndoableAction> _redo = new();

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Execute(IUndoableAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            action.Do();
            _undo.Push(action);
            _redo.Clear();
        }

        public bool Undo()
        {
            if(_undo.Count ==0) return false;

            var action = _undo.Pop();
            action.Undo();
            _redo.Push(action);
            return true;
        }

        public bool Redo()
        {
            if(_redo.Count ==0) return false;
            var action = _redo.Pop();
            action.Do();
            _undo.Push(action);
            return true;
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
        }




    }
}
