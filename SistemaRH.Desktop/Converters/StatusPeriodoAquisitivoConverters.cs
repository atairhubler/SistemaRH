using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Desktop.Converters;

public class StatusPeriodoAquisitivoToTextoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is StatusPeriodoAquisitivo status ? status switch
        {
            StatusPeriodoAquisitivo.EmAquisicao => "Em Aquisição",
            StatusPeriodoAquisitivo.Pendente => "Pendente",
            StatusPeriodoAquisitivo.Regularizado => "Regularizado",
            StatusPeriodoAquisitivo.Vencido => "Vencido",
            _ => status.ToString()
        } : "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusPeriodoAquisitivoToCorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is StatusPeriodoAquisitivo status ? status switch
        {
            StatusPeriodoAquisitivo.EmAquisicao => new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
            StatusPeriodoAquisitivo.Pendente => new SolidColorBrush(Color.FromRgb(0xE0, 0xA5, 0x00)),
            StatusPeriodoAquisitivo.Regularizado => new SolidColorBrush(Color.FromRgb(0x2E, 0xA0, 0x4A)),
            StatusPeriodoAquisitivo.Vencido => new SolidColorBrush(Color.FromRgb(0xD0, 0x30, 0x30)),
            _ => new SolidColorBrush(Colors.Gray)
        } : new SolidColorBrush(Colors.Gray);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
