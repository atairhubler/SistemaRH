using System.Windows;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class CampoPersonalizadoDetailView : Window
{
    public CampoPersonalizadoDetailView()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = App.ServiceProvider.GetService(typeof(CampoPersonalizadoDetailViewModel)) as CampoPersonalizadoDetailViewModel;
        if (viewModel != null)
        {
            viewModel.FecharJanela = () => this.Close();
            DataContext = viewModel;
        }
    }
}
