using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.ViewModels
{
    public abstract class PageViewModelBase : ViewModelBase
    {
        public virtual string Title => string.Empty;

    }
}
