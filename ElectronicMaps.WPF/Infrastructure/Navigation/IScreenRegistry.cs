using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    public interface IScreenRegistry
    {
        void Register(WpfScreenDescriptor descriptor);

        WpfScreenDescriptor? Find(string key);

        IReadOnlyCollection<WpfScreenDescriptor> GetAll();


    }
}
