using System.Windows;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class FuncionarioEstagiarioDetailView : Window
{
    public FuncionarioEstagiarioDetailView()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = App.ServiceProvider.GetService(typeof(FuncionarioEstagiarioDetailViewModel)) as FuncionarioEstagiarioDetailViewModel;
        if (viewModel != null)
        {
            viewModel.FecharJanela = () => this.Close();
            DataContext = viewModel;
        }
    }
}
