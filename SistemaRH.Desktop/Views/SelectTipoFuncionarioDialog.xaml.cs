using System.Windows;
using System.Windows.Controls;

namespace SistemaRH.Desktop.Views;

public partial class SelectTipoFuncionarioDialog : Window
{
    public string? SelectedTipo { get; private set; }

    public SelectTipoFuncionarioDialog()
    {
        InitializeComponent();
    }

    private void TipoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            SelectedTipo = button.Tag?.ToString();
            DialogResult = true;
            Close();
        }
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
