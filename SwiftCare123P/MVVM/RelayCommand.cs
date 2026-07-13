using System.Windows.Input;

namespace SwiftCare123P.MVVM;

/// <summary>
/// A reusable ICommand implementation for synchronous actions.
/// Lets XAML bind a button/tap directly to a ViewModel method via Command="{Binding SomeCommand}",
/// instead of the View's code-behind handling a Clicked/Tapped event.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
