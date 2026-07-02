using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class FolhaPagamentoViewModel : BaseViewModel
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IFolhaPagamentoService _folhaService;
    private readonly IDialogService _dialogService;
    private readonly TabelasFolhaService _tabelasService;
    private readonly IAuditoriaService _auditoriaService;

    // Fonte estável — nunca substituída, só populada. Isso garante que o ItemsSource
    // do ComboBox não mude de referência ao filtrar, evitando o reset do Text pelo WPF.
    private readonly ObservableCollection<FuncionarioCLTDto> _fonte = new();
    private List<FuncionarioCLTDto> _todosFuncionarios = new();
    private HashSet<FuncionarioCLTDto> _top5 = new();

    private FuncionarioCLTDto? _funcionarioSelecionado;
    private string _filtroNome = "";
    private int _numeroDependentes = 0;
    private int _mesSelecionado = DateTime.Now.Month;
    private int _anoSelecionado = DateTime.Now.Year;
    private FolhaPagamentoDto? _resultado;
    private bool _mostrarResultado = false;

    // ICollectionView sobre _fonte: filtro in-place, sem trocar referência do ItemsSource
    public ICollectionView Funcionarios { get; }

    public FuncionarioCLTDto? FuncionarioSelecionado
    {
        get => _funcionarioSelecionado;
        set => SetProperty(ref _funcionarioSelecionado, value);
    }

    public string FiltroNome
    {
        get => _filtroNome;
        set
        {
            if (SetProperty(ref _filtroNome, value))
                AplicarFiltro();
        }
    }

    public int NumeroDependentes
    {
        get => _numeroDependentes;
        set => SetProperty(ref _numeroDependentes, value);
    }

    public int MesSelecionado
    {
        get => _mesSelecionado;
        set => SetProperty(ref _mesSelecionado, value);
    }

    public int AnoSelecionado
    {
        get => _anoSelecionado;
        set => SetProperty(ref _anoSelecionado, value);
    }

    public FolhaPagamentoDto? Resultado
    {
        get => _resultado;
        set => SetProperty(ref _resultado, value);
    }

    public bool MostrarResultado
    {
        get => _mostrarResultado;
        set => SetProperty(ref _mostrarResultado, value);
    }

    public ICommand CarregarCommand { get; }
    public ICommand CalcularCommand { get; }
    public ICommand ExportarExcelCommand { get; }
    public ICommand AbrirTabelasCommand { get; }

    public FolhaPagamentoViewModel(
        IFuncionarioService funcionarioService,
        IFolhaPagamentoService folhaService,
        IDialogService dialogService,
        TabelasFolhaService tabelasService,
        IAuditoriaService auditoriaService)
    {
        _funcionarioService = funcionarioService;
        _folhaService = folhaService;
        _dialogService = dialogService;
        _tabelasService = tabelasService;
        _auditoriaService = auditoriaService;

        Funcionarios = CollectionViewSource.GetDefaultView(_fonte);
        Funcionarios.Filter = FiltrarItem;

        CarregarCommand = new RelayCommand(_ => _ = CarregarAsync());
        CalcularCommand = new RelayCommand(_ => Calcular());
        ExportarExcelCommand = new RelayCommand(_ => _ = ExportarExcelAsync());
        AbrirTabelasCommand = new RelayCommand(_ => AbrirTabelasDialog());
    }

    private bool FiltrarItem(object obj)
    {
        if (obj is not FuncionarioCLTDto f) return false;
        if (string.IsNullOrWhiteSpace(_filtroNome)) return true;
        return _top5.Contains(f);
    }

    private void AplicarFiltro()
    {
        if (string.IsNullOrWhiteSpace(_filtroNome))
        {
            _top5 = new HashSet<FuncionarioCLTDto>();
        }
        else
        {
            var termo = _filtroNome.Trim();
            _top5 = _todosFuncionarios
                .Where(f => f.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.Nome.StartsWith(termo, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(f => f.Nome)
                .Take(5)
                .ToHashSet();
        }

        // Refresh in-place: não troca o ItemsSource, não reseta o Text do ComboBox
        Funcionarios.Refresh();
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            var clts = await _funcionarioService.GetAllCltAsync();
            _todosFuncionarios = clts.ToList();

            _fonte.Clear();
            foreach (var c in _todosFuncionarios)
                _fonte.Add(c);

            Funcionarios.Refresh();
            FuncionarioSelecionado = null;
            StatusMessage = $"{_fonte.Count} funcionário(s) CLT carregado(s)";
            MostrarResultado = false;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally { IsLoading = false; }
    }

    private void Calcular()
    {
        if (FuncionarioSelecionado == null)
        {
            StatusMessage = "Selecione um funcionário.";
            return;
        }
        var tabelas = _tabelasService.GetTabelas();
        Resultado = _folhaService.Calcular(FuncionarioSelecionado, NumeroDependentes, MesSelecionado, AnoSelecionado, tabelas);
        MostrarResultado = true;
        StatusMessage = $"Holerite calculado para {Resultado.NomeFuncionario} — {Resultado.Competencia}";
    }

    private void AbrirTabelasDialog()
    {
        var vm = App.ServiceProvider.GetRequiredService<TabelasCalculoViewModel>();
        vm.CarregarTabelas();
        var dialog = new Views.TabelasCalculoDialog(vm);
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        dialog.ShowDialog();
    }

    private async Task ExportarExcelAsync()
    {
        if (Resultado == null) { StatusMessage = "Calcule a folha antes de exportar."; return; }
        var caminho = await _dialogService.SalvarArquivoAsync("Arquivo Excel|*.xlsx");
        if (string.IsNullOrEmpty(caminho)) return;
        try
        {
            IsLoading = true;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Holerite");

            ws.Cell("A1").Value = "HOLERITE DE PAGAMENTO";
            ws.Range("A1:D1").Merge().Style.Font.Bold = true;
            ws.Cell("A2").Value = $"Competência: {Resultado.Competencia}";
            ws.Cell("A3").Value = $"Funcionário: {Resultado.NomeFuncionario}";
            ws.Cell("A4").Value = $"Cargo: {Resultado.Cargo}";
            ws.Cell("A5").Value = $"Dependentes: {Resultado.NumeroDependentes}";

            ws.Cell("A7").Value = "DESCRIÇÃO";
            ws.Cell("B7").Value = "PROVENTOS";
            ws.Cell("C7").Value = "DESCONTOS";
            ws.Row(7).Style.Font.Bold = true;

            ws.Cell("A8").Value = "Salário Bruto";
            ws.Cell("B8").Value = Resultado.SalarioBruto;
            ws.Cell("A9").Value = "INSS";
            ws.Cell("C9").Value = Resultado.DescontoINSS;
            ws.Cell("A10").Value = "IRRF";
            ws.Cell("C10").Value = Resultado.DescontoIRRF;

            ws.Cell("A12").Value = "Salário Líquido";
            ws.Cell("B12").Value = Resultado.SalarioLiquido;
            ws.Row(12).Style.Font.Bold = true;

            ws.Cell("A14").Value = "FGTS (encargo empresa)";
            ws.Cell("B14").Value = Resultado.FGTS;
            ws.Cell("A15").Value = "Custo Total Empresa";
            ws.Cell("B15").Value = Resultado.CustoTotalEmpresa;

            foreach (var col in new[] { "B", "C" })
                ws.Column(col).Style.NumberFormat.Format = "R$ #,##0.00";

            ws.Columns().AdjustToContents();
            wb.SaveAs(caminho);

            await _auditoriaService.RegistrarAsync("Geração", "Folha de Pagamento",
                $"Holerite gerado para {Resultado.NomeFuncionario} — competência {Resultado.Competencia}");

            await _dialogService.ShowInfoAsync("Exportar", $"Holerite exportado com sucesso!\n{caminho}");
            StatusMessage = "Holerite exportado.";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro ao exportar", ex.Message);
        }
        finally { IsLoading = false; }
    }
}
