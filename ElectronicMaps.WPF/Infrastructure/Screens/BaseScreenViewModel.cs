using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.WPF.ViewModels;
using Navigation.Core.Abstractions;
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
    public abstract class BaseScreenViewModel : ObservableObject, IScreen
    {
        public string Title { get; }

        protected BaseScreenViewModel()
        {
        }

        public virtual Task OnEnterAsync(object? parameter) => Task.CompletedTask;

        public virtual Task OnLeaveAsync() => Task.CompletedTask;
    }
}
