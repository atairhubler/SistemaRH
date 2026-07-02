using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop.Views;

public partial class FuncionarioEstagiarioDetailView : Window
{
    private static readonly CultureInfo PtBR = new("pt-BR");
    private bool _formatando = false;

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
            PopularBolsa(viewModel.Bolsa);
        }
    }

    private void PopularBolsa(decimal valor)
    {
        _formatando = true;
        TxtBolsa.Text = valor > 0 ? FormatarBolsa(valor.ToString("F2", CultureInfo.InvariantCulture)) : "";
        _formatando = false;
    }

    private void TxtBolsa_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_formatando || sender is not TextBox tb) return;

        var texto = tb.Text;
        var caret = tb.CaretIndex;

        string parteInteira, parteDecimal = "";
        bool temVirgula = texto.Contains(',');

        if (temVirgula)
        {
            var partes = texto.Split(',');
            parteInteira = new string(partes[0].Where(char.IsDigit).ToArray());
            parteDecimal = new string(partes[1].Where(char.IsDigit).ToArray().Take(2).ToArray());
        }
        else
        {
            parteInteira = new string(texto.Where(char.IsDigit).ToArray());
        }

        if (!long.TryParse(string.IsNullOrEmpty(parteInteira) ? "0" : parteInteira, out var intVal))
            return;

        var inteiroFormatado = intVal == 0 && !temVirgula ? "" : intVal.ToString("N0", PtBR);
        var formatado = temVirgula ? $"{inteiroFormatado},{parteDecimal}" : inteiroFormatado;

        if (texto == formatado)
        {
            AtualizarViewModel(formatado);
            return;
        }

        _formatando = true;
        int pontosBefore = texto[..Math.Min(caret, texto.Length)].Count(c => c == '.');
        tb.Text = formatado;
        int pontosAfter = formatado[..Math.Min(caret, formatado.Length)].Count(c => c == '.');
        tb.CaretIndex = Math.Max(0, Math.Min(caret + (pontosAfter - pontosBefore), formatado.Length));
        _formatando = false;

        AtualizarViewModel(formatado);
    }

    private static string FormatarBolsa(string valorStr)
    {
        var partes = valorStr.Split('.');
        var inteiro = long.TryParse(partes[0], out var i) ? i : 0;
        var decimal_ = partes.Length > 1 ? partes[1].PadRight(2, '0')[..2] : "00";
        return $"{inteiro.ToString("N0", PtBR)},{decimal_}";
    }

    private void AtualizarViewModel(string formatado)
    {
        if (DataContext is not FuncionarioEstagiarioDetailViewModel vm) return;
        var limpo = formatado.Replace(".", "").Replace(",", ".");
        if (decimal.TryParse(limpo, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
            vm.Bolsa = valor;
    }
}
