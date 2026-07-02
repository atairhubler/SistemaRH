using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SistemaRH.Desktop.Services;
using SistemaRH.Desktop.Views;
using WpfApp = System.Windows.Application;

namespace SistemaRH.Desktop.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private UserControl _currentView;
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly EmpresasViewModel _empresasViewModel;
    private readonly FuncionariosViewModel _funcionariosViewModel;
    private readonly FeriasViewModel _feriasViewModel;
    private readonly RpaNfViewModel _rpaNfViewModel;
    private readonly RelatorioComparativoViewModel _relatorioViewModel;
    private readonly ConfiguracoesViewModel _configuracoesViewModel;
    private readonly FolhaPagamentoViewModel _folhaPagamentoViewModel;
    private readonly ConfiguracaoService _configService;

    public UserControl CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    private bool _modoEscuro;
    private string _temaIcone;

    public string TemaIcone
    {
        get => _temaIcone;
        set => SetProperty(ref _temaIcone, value);
    }

    public ICommand DashboardCommand { get; }
    public ICommand EmpresasCommand { get; }
    public ICommand FuncionariosCommand { get; }
    public ICommand FeriasCommand { get; }
    public ICommand RpaNfCommand { get; }
    public ICommand RelatoriosCommand { get; }
    public ICommand ConfiguracoesCommand { get; }
    public ICommand FolhaPagamentoCommand { get; }
    public ICommand AlternarTemaCommand { get; }

    public MainWindowViewModel(
        DashboardViewModel dashboardViewModel,
        EmpresasViewModel empresasViewModel,
        FuncionariosViewModel funcionariosViewModel,
        FeriasViewModel feriasViewModel,
        RpaNfViewModel rpaNfViewModel,
        RelatorioComparativoViewModel relatorioViewModel,
        ConfiguracoesViewModel configuracoesViewModel,
        FolhaPagamentoViewModel folhaPagamentoViewModel,
        ConfiguracaoService configService)
    {
        _dashboardViewModel = dashboardViewModel;
        _empresasViewModel = empresasViewModel;
        _funcionariosViewModel = funcionariosViewModel;
        _feriasViewModel = feriasViewModel;
        _rpaNfViewModel = rpaNfViewModel;
        _relatorioViewModel = relatorioViewModel;
        _configuracoesViewModel = configuracoesViewModel;
        _folhaPagamentoViewModel = folhaPagamentoViewModel;
        _configService = configService;

        _modoEscuro = configService.ModoEscuro;
        _temaIcone = _modoEscuro ? "☀️" : "🌙";

        DashboardCommand = new RelayCommand(_ => MostrarDashboard());
        EmpresasCommand = new RelayCommand(_ => MostrarEmpresas());
        FuncionariosCommand = new RelayCommand(_ => MostrarFuncionarios());
        FeriasCommand = new RelayCommand(_ => MostrarFerias());
        RpaNfCommand = new RelayCommand(_ => MostrarRpaNf());
        RelatoriosCommand = new RelayCommand(_ => MostrarRelatorios());
        ConfiguracoesCommand = new RelayCommand(_ => MostrarConfiguracoes());
        FolhaPagamentoCommand = new RelayCommand(_ => MostrarFolhaPagamento());
        AlternarTemaCommand = new RelayCommand(_ => AlternarTema());

        MostrarDashboard();
    }

    private void MostrarDashboard()
    {
        CurrentView = new DashboardView { DataContext = _dashboardViewModel };
        StatusMessage = "Dashboard";
        _ = _dashboardViewModel.CarregarAsync();
    }

    private void MostrarEmpresas()
    {
        CurrentView = new EmpresasListView { DataContext = _empresasViewModel };
        StatusMessage = "Empresas";
        _ = _empresasViewModel.CarregarAsync();
    }

    private void MostrarFuncionarios()
    {
        CurrentView = new FuncionariosListView { DataContext = _funcionariosViewModel };
        StatusMessage = "Funcionários";
        _ = _funcionariosViewModel.CarregarAsync();
    }

    private void MostrarFerias()
    {
        CurrentView = new FeriasView { DataContext = _feriasViewModel };
        StatusMessage = "Férias";
    }

    private void MostrarRpaNf()
    {
        CurrentView = new RpaNfView { DataContext = _rpaNfViewModel };
        StatusMessage = "RPA / NF";
    }

    private void MostrarRelatorios()
    {
        CurrentView = new RelatorioComparativoView { DataContext = _relatorioViewModel };
        StatusMessage = "Relatórios";
    }

    private void MostrarConfiguracoes()
    {
        CurrentView = new ConfiguracoesView { DataContext = _configuracoesViewModel };
        StatusMessage = "Configurações";
    }

    private void MostrarFolhaPagamento()
    {
        CurrentView = new FolhaPagamentoView { DataContext = _folhaPagamentoViewModel };
        StatusMessage = "Folha de Pagamento";
        _ = _folhaPagamentoViewModel.CarregarAsync();
    }

    private void AlternarTema()
    {
        _modoEscuro = !_modoEscuro;
        _configService.ModoEscuro = _modoEscuro;
        var dicts = WpfApp.Current.Resources.MergedDictionaries;
        var temaAtual = dicts.FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);
        if (temaAtual != null) dicts.Remove(temaAtual);
        dicts.Add(new ResourceDictionary { Source = new Uri($"pack://application:,,,/Themes/{ObterArquivoTema()}.xaml") });
        TemaIcone = _modoEscuro ? "☀️" : "🌙";
    }

    private string ObterArquivoTema()
    {
        if (_modoEscuro) return "DarkTheme";
        return _configService.TemaCor switch
        {
            "Verde" => "LightTheme_Verde",
            "Roxo" => "LightTheme_Roxo",
            "Laranja" => "LightTheme_Laranja",
            "Rosa" => "LightTheme_Rosa",
            "Vermelho" => "LightTheme_Vermelho",
            "Cinza" => "LightTheme_Cinza",
            _ => "LightTheme"
        };
    }
}
