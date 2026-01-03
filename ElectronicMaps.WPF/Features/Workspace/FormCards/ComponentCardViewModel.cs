using ElectronicMaps.Application.WorkspaceProject.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.FormCards
{
    public partial class ComponentCardViewModel : CardViewModelBase
    {
        public ComponentCardViewModel(string formCode, string formName, int number, ComponentDraft item)
           : base(formCode, formName, number, item)
        {

        }
    }
}
