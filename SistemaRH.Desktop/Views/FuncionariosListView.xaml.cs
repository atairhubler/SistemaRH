using System.Windows.Controls;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class FuncionariosListView : UserControl
{
    public FuncionariosListView()
    {
        InitializeComponent();
    }

    private void FiltroTipo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not FuncionariosViewModel vm) return;
        var item = (sender as ComboBox)?.SelectedItem as ComboBoxItem;
        vm.FiltroTipo = item?.Content?.ToString() ?? "Todos";
        _ = vm.CarregarAsync();
    }
}
