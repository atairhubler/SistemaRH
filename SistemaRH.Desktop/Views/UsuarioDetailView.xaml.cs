using System.Windows;
using System.Windows.Controls;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class UsuarioDetailView : Window
{
    public UsuarioDetailView()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = App.ServiceProvider.GetService(typeof(UsuarioDetailViewModel)) as UsuarioDetailViewModel;
        if (viewModel != null)
        {
            viewModel.FecharJanela = () => this.Close();
            DataContext = viewModel;
        }
    }

    private void PwdSenha_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UsuarioDetailViewModel vm)
            vm.Senha = ((PasswordBox)sender).Password;
    }

    private void PwdConfirmar_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UsuarioDetailViewModel vm)
            vm.ConfirmarSenha = ((PasswordBox)sender).Password;
    }
}
