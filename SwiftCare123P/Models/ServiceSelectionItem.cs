using CommunityToolkit.Mvvm.ComponentModel;

namespace SwiftCare123P.Models;

/// <summary>
/// A service loaded from the database, paired with whether the current caregiver
/// has it selected. Replaces a previous fixed-size, hand-typed list of five
/// checkboxes that had to be kept in sync with the database's seeded service list
/// by hand.
/// </summary>
public partial class ServiceSelectionItem : ObservableObject
{
    public int ServiceID { get; set; }
    public string ServiceName { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}
