using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Commands.XmlCommands
{
    public interface IXmlCommands
    {
        IAsyncRelayCommand OpenXml {  get; }
        Task OpenXmlAsync();
    }
}
