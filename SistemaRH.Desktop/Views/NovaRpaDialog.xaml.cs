using System.Windows;

namespace SistemaRH.Desktop.Views;

public partial class NovaRpaDialog : Window
{
    public NovaRpaDialog()
    {
        InitializeComponent();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NumeroDocumentoTxt.Text) ||
            string.IsNullOrWhiteSpace(ValorBrutoTxt.Text) ||
            MesCombo.SelectedIndex < 0 ||
            string.IsNullOrWhiteSpace(AnoTxt.Text))
        {
            MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }
}
