using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SistemaRH.Desktop.Views.Controls;

public partial class LineChartView : UserControl
{
    private static readonly CultureInfo PtBR = new("pt-BR");

    public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
        nameof(Series), typeof(List<ChartSerie>), typeof(LineChartView),
        new PropertyMetadata(null, OnDadosAlterados));

    public static readonly DependencyProperty RotulosProperty = DependencyProperty.Register(
        nameof(Rotulos), typeof(List<string>), typeof(LineChartView),
        new PropertyMetadata(null, OnDadosAlterados));

    public static readonly DependencyProperty FormatoValorProperty = DependencyProperty.Register(
        nameof(FormatoValor), typeof(string), typeof(LineChartView),
        new PropertyMetadata("N0"));

    public List<ChartSerie>? Series
    {
        get => (List<ChartSerie>?)GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    public List<string>? Rotulos
    {
        get => (List<string>?)GetValue(RotulosProperty);
        set => SetValue(RotulosProperty, value);
    }

    public string FormatoValor
    {
        get => (string)GetValue(FormatoValorProperty);
        set => SetValue(FormatoValorProperty, value);
    }

    public LineChartView()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Desenhar();
    }

    private static void OnDadosAlterados(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (d as LineChartView)?.Desenhar();
    }

    private void Desenhar()
    {
        ChartCanvas.Children.Clear();
        LegendaPanel.Children.Clear();

        var series = Series;
        var rotulos = Rotulos;
        if (series == null || series.Count == 0 || rotulos == null || rotulos.Count == 0) return;

        var largura = ChartCanvas.ActualWidth;
        var altura = ChartCanvas.ActualHeight;
        if (largura <= 0 || altura <= 0) return;

        const double margemEsquerda = 55;
        const double margemInferior = 26;
        const double margemSuperior = 10;
        const double margemDireita = 10;

        var areaLargura = largura - margemEsquerda - margemDireita;
        var areaAltura = altura - margemInferior - margemSuperior;
        if (areaLargura <= 0 || areaAltura <= 0) return;

        var max = series.SelectMany(s => s.Valores).DefaultIfEmpty(0).Max();
        if (max <= 0) max = 1;

        var bordaCard = (Brush)FindResource("BorderCard");
        var textoFraco = (Brush)FindResource("TextFaint");

        const int linhasGrade = 4;
        for (var i = 0; i <= linhasGrade; i++)
        {
            var y = margemSuperior + areaAltura - areaAltura * i / linhasGrade;

            ChartCanvas.Children.Add(new Line
            {
                X1 = margemEsquerda, X2 = largura - margemDireita, Y1 = y, Y2 = y,
                Stroke = bordaCard, StrokeThickness = 1
            });

            var valorEixo = max * i / linhasGrade;
            var rotuloEixo = new TextBlock
            {
                Text = valorEixo.ToString(FormatoValor, PtBR),
                FontSize = 10,
                Foreground = textoFraco
            };
            Canvas.SetLeft(rotuloEixo, 0);
            Canvas.SetTop(rotuloEixo, y - 7);
            ChartCanvas.Children.Add(rotuloEixo);
        }

        var qtdPontos = rotulos.Count;
        var passoX = qtdPontos > 1 ? areaLargura / (qtdPontos - 1) : 0;

        for (var i = 0; i < qtdPontos; i++)
        {
            var x = margemEsquerda + passoX * i;
            var rotuloEixoX = new TextBlock
            {
                Text = rotulos[i],
                FontSize = 10,
                Foreground = textoFraco
            };
            Canvas.SetLeft(rotuloEixoX, x - 15);
            Canvas.SetTop(rotuloEixoX, altura - margemInferior + 6);
            ChartCanvas.Children.Add(rotuloEixoX);
        }

        foreach (var serie in series)
        {
            var pontos = new PointCollection();
            var qtd = Math.Min(serie.Valores.Count, qtdPontos);

            for (var i = 0; i < qtd; i++)
            {
                var x = margemEsquerda + passoX * i;
                var y = margemSuperior + areaAltura - areaAltura * serie.Valores[i] / max;
                pontos.Add(new Point(x, y));
            }

            ChartCanvas.Children.Add(new Polyline
            {
                Points = pontos,
                Stroke = serie.Cor,
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round
            });

            for (var i = 0; i < qtd; i++)
            {
                var ponto = pontos[i];
                var marcador = new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = serie.Cor,
                    ToolTip = $"{serie.Nome} — {rotulos[i]}: {serie.Valores[i].ToString(FormatoValor, PtBR)}"
                };
                Canvas.SetLeft(marcador, ponto.X - 3.5);
                Canvas.SetTop(marcador, ponto.Y - 3.5);
                ChartCanvas.Children.Add(marcador);
            }

            var itemLegenda = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 15, 0) };
            itemLegenda.Children.Add(new Border
            {
                Width = 10,
                Height = 10,
                Background = serie.Cor,
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            itemLegenda.Children.Add(new TextBlock
            {
                Text = serie.Nome,
                FontSize = 11,
                Foreground = (Brush)FindResource("TextMuted"),
                VerticalAlignment = VerticalAlignment.Center
            });
            LegendaPanel.Children.Add(itemLegenda);
        }
    }
}
