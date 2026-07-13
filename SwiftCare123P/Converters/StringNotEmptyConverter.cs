using System.Globalization;

namespace SwiftCare123P.Converters;

/// <summary>
/// Converts a string to true/false based on whether it has content.
/// Used to bind a Label's IsVisible to an ErrorMessage property —
/// the label only shows up once the ViewModel actually sets an error.
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value as string);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
