using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.WPF.Infrastructure.Commands.XmlCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Commands
{
    public interface IAppCommands
    {
        IXmlCommands Xml { get; }




    }
}
