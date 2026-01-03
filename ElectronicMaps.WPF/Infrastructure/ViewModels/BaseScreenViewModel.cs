using CommunityToolkit.Mvvm.ComponentModel;
using Navigation.Core.Abstractions;


namespace ElectronicMaps.WPF.Infrastructure.ViewModels
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
