using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SistemaRH.Desktop.Behaviors;

public static class MoedaMask
{
    private static readonly CultureInfo PtBR = new("pt-BR");

    public static readonly DependencyProperty ValorProperty = DependencyProperty.RegisterAttached(
        "Valor",
        typeof(decimal),
        typeof(MoedaMask),
        new FrameworkPropertyMetadata(decimal.MinValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValorChanged));

    public static decimal GetValor(DependencyObject obj) => (decimal)obj.GetValue(ValorProperty);
    public static void SetValor(DependencyObject obj, decimal value) => obj.SetValue(ValorProperty, value);

    private static readonly DependencyProperty AtualizandoProperty = DependencyProperty.RegisterAttached(
        "Atualizando", typeof(bool), typeof(MoedaMask), new PropertyMetadata(false));

    private static readonly DependencyProperty InicializadoProperty = DependencyProperty.RegisterAttached(
        "Inicializado", typeof(bool), typeof(MoedaMask), new PropertyMetadata(false));

    private static void OnValorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if (!(bool)tb.GetValue(InicializadoProperty))
        {
            tb.SetValue(InicializadoProperty, true);
            tb.TextChanged += (_, _) => AoDigitar(tb);
        }

        if ((bool)tb.GetValue(AtualizandoProperty)) return;

        ExibirValor(tb, (decimal)e.NewValue);
    }

    private static void ExibirValor(TextBox tb, decimal valor)
    {
        tb.SetValue(AtualizandoProperty, true);
        tb.Text = valor > 0 ? valor.ToString("N2", PtBR) : "";
        tb.CaretIndex = tb.Text.Length;
        tb.SetValue(AtualizandoProperty, false);
    }

    private static void AoDigitar(TextBox tb)
    {
        if ((bool)tb.GetValue(AtualizandoProperty)) return;

        var digitos = new string(tb.Text.Where(char.IsDigit).ToArray()).TrimStart('0');

        if (digitos.Length == 0)
        {
            ExibirValor(tb, 0m);
            SetValor(tb, 0m);
            return;
        }

        var centavos = digitos.PadLeft(3, '0');
        var parteInteira = centavos[..^2].TrimStart('0');
        if (parteInteira.Length == 0) parteInteira = "0";
        var parteDecimal = centavos[^2..];

        var valor = decimal.Parse($"{parteInteira}.{parteDecimal}", CultureInfo.InvariantCulture);

        ExibirValor(tb, valor);
        SetValor(tb, valor);
    }
}
