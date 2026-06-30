using System.Windows;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class EmpresaDetailView : Window
{
    public EmpresaDetailView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is EmpresaDetailViewModel vm)
                vm.FecharJanela = () => this.Close();
        };
    }
}
