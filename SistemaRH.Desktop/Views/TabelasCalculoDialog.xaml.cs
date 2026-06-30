using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class TabelasCalculoDialog : Window
{
    public TabelasCalculoDialog(TabelasCalculoViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.AoFechar = () => Close();
    }

    private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) return;
        e.Handled = true;
        var sv = EncontrarPai<ScrollViewer>((DependencyObject)sender);
        sv?.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent,
            Source = sender
        });
    }

    private static T? EncontrarPai<T>(DependencyObject filho) where T : DependencyObject
    {
        var atual = VisualTreeHelper.GetParent(filho);
        while (atual != null)
        {
            if (atual is T found) return found;
            atual = VisualTreeHelper.GetParent(atual);
        }
        return null;
    }
}
