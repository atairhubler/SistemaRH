using System.Windows;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class FuncionarioCLTDetailView : Window
{
    public FuncionarioCLTDetailView()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = App.ServiceProvider.GetService(typeof(FuncionarioCLTDetailViewModel)) as FuncionarioCLTDetailViewModel;
        if (viewModel != null)
        {
            viewModel.FecharJanela = () => this.Close();
            DataContext = viewModel;
        }
    }
}
