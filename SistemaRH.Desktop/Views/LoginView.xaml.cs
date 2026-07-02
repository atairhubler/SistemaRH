using System.Windows;
using System.Windows.Input;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = App.ServiceProvider.GetService(typeof(LoginViewModel)) as LoginViewModel;
        if (viewModel != null)
        {
            viewModel.FecharJanela = () => { DialogResult = viewModel.LoginBemSucedido; Close(); };
            DataContext = viewModel;
        }
    }

    private async void BtnEntrar_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            await vm.EntrarAsync(PwdSenha.Password);
    }

    private async void PwdSenha_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
            await vm.EntrarAsync(PwdSenha.Password);
    }
}
