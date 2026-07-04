using System.Windows;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class CamposPersonalizadosView : Window
{
    public CamposPersonalizadosView()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = App.ServiceProvider.GetService(typeof(CamposPersonalizadosViewModel)) as CamposPersonalizadosViewModel;
        DataContext = viewModel;
        _ = viewModel?.CarregarAsync();
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();
}
