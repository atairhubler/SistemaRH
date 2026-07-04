using System.Windows.Media;

namespace SistemaRH.Desktop.Views.Controls;

public class ChartSerie
{
    public string Nome { get; set; } = "";
    public Brush Cor { get; set; } = Brushes.Black;
    public List<double> Valores { get; set; } = new();
}
