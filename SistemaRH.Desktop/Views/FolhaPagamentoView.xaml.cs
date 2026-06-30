using System.Windows.Controls;
using System.Windows.Input;

namespace SistemaRH.Desktop.Views;

public partial class FolhaPagamentoView : UserControl
{
    public FolhaPagamentoView()
    {
        InitializeComponent();
    }

    private void FuncionarioFiltro_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Return or Key.Escape or Key.Tab or Key.Up or Key.Down)
            return;

        var cb = (ComboBox)sender;
        if (!cb.IsDropDownOpen)
            cb.IsDropDownOpen = true;
    }
}
