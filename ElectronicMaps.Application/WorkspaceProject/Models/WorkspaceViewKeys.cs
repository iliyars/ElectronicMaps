using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject.Models
{
    /// <summary>
    /// Ключи представлений и служебные коды Workspace.
    /// Используются Store и ViewModel, но не UI напрямую.
    /// </summary>
    public static class WorkspaceViewKeys
    {
        public const string UndefinedForm = "FORM_UNDEFINED";

        public const string NoFamily = "NOFAM";

        public const string FamilyFormCode = "FORM_4";
        public const string FamilyFormName = "Форма 4";
    }
}
