using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ExporterModels.AttachedProperties;

public static class DataGridMultiSelect
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(DataGridMultiSelect),
            new PropertyMetadata(null, OnSelectedItemsChanged));

    public static void SetSelectedItems(DependencyObject element, IList value)
    {
        element.SetValue(SelectedItemsProperty, value);
    }

    public static IList GetSelectedItems(DependencyObject element)
    {
        return (IList)element.GetValue(SelectedItemsProperty);
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        grid.SelectionChanged -= Grid_SelectionChanged;
        grid.SelectionChanged += Grid_SelectionChanged;
    }

    private static void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid grid) return;
        var list = GetSelectedItems(grid);
        if (list == null) return;

        foreach (var removed in e.RemovedItems) list.Remove(removed);
        foreach (var added in e.AddedItems)
            if (!list.Contains(added))
                list.Add(added);
    }
}