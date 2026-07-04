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

    private string SenhaAtual => TxtSenhaVisivel.Visibility == Visibility.Visible ? TxtSenhaVisivel.Text : PwdSenha.Password;

    private async void BtnEntrar_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            await vm.EntrarAsync(SenhaAtual);
    }

    private async void PwdSenha_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
            await vm.EntrarAsync(SenhaAtual);
    }

    private void BtnToggleSenha_Click(object sender, RoutedEventArgs e)
    {
        var mostrando = TxtSenhaVisivel.Visibility == Visibility.Visible;
        if (mostrando)
        {
            PwdSenha.Password = TxtSenhaVisivel.Text;
            PwdSenha.Visibility = Visibility.Visible;
            TxtSenhaVisivel.Visibility = Visibility.Collapsed;
            BtnToggleSenha.Content = "👁";
        }
        else
        {
            TxtSenhaVisivel.Text = PwdSenha.Password;
            TxtSenhaVisivel.Visibility = Visibility.Visible;
            PwdSenha.Visibility = Visibility.Collapsed;
            BtnToggleSenha.Content = "🙈";
        }
    }
}
