using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SwiftCare123P.MVVM;

/// <summary>
/// Base class for all ViewModels. Provides property-change notification
/// so the UI (View) automatically updates when bound data changes.
/// Every ViewModel in this app inherits from this class (OOP: inheritance).
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets a backing field and raises PropertyChanged only if the value actually changed.
    /// Call this from every property setter instead of repeating boilerplate.
    /// </summary>
    protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
            return false;

        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
