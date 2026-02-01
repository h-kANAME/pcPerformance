using System.Windows.Input;

namespace OptimizerApp.ViewModels;

public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;

    public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null)
            return true;

        return parameter is T tParam ? _canExecute(tParam) : _canExecute(default);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T tParam)
            _execute(tParam);
        else
            _execute(default);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
