using System.Collections;
using Visibility = System.Windows.Visibility;

namespace SolidIntersection.Models
{
    public partial class SelectedItems(string nameItem, bool isChecked) : ObservableObject
    {
        [ObservableProperty] private string _nameItem = nameItem;
        [ObservableProperty] private string? _value;
        [ObservableProperty] private bool _isChecked = isChecked;
        [ObservableProperty] private Visibility _visibleTextBox = Visibility.Visible;

        internal string GetName() => NameItem;
        internal void SetCheck(bool value) => IsChecked = value;
    }
}