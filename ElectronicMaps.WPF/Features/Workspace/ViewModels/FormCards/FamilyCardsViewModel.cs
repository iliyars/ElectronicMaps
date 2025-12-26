using ElectronicMaps.Application.WorkspaceProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Features.Workspace.ViewModels.FormCards
{
    public class FamilyCardsViewModel : CardViewModelBase
    {

        public string FamilyName { get; }


        public FamilyCardsViewModel(string formCode, string formName, int number, ComponentDraft item) : base(formCode, formName, number, item)
        {
            FamilyName = item.Name;


        }
    }
}
