using System.Windows;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class UsuariosView : Window
{
    public UsuariosView()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = App.ServiceProvider.GetService(typeof(UsuariosViewModel)) as UsuariosViewModel;
        DataContext = viewModel;
        _ = viewModel?.CarregarAsync();
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();
}
