using ElectronicMaps.Application.Common.Navigation;
using ElectronicMaps.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Screens
{
    /// <summary>
    /// Базовый класс для всех экранов.
    /// Shell будет держать CurrentScreen типа BaseScreenViewModel.
    /// </summary>
    public abstract class BaseScreenViewModel : ViewModelBase
    {
        public string Title { get; }

        protected BaseScreenViewModel(string title)
        {
            Title = title;
        }

        public virtual Task OnEnterAsync(object? parameter) => Task.CompletedTask;

        public virtual Task OnLeaveAsync() => Task.CompletedTask;
    }
}
