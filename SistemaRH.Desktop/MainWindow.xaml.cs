using System.Windows;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var mainViewModel = App.ServiceProvider.GetService(typeof(MainWindowViewModel)) as MainWindowViewModel;
        DataContext = mainViewModel;
    }
}