using ElectronicMaps.Application.WorkspaceProject.Models;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.GridRows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Features.Workspace.ViewModels.FormCards
{
    public class ComponentCardViewModel : CardViewModelBase
    {
        public ComponentCardViewModel(string formCode, string formName, int number, ComponentDraft item) 
            : base(formCode,formName,number, item)
        {

        }
    }
}
