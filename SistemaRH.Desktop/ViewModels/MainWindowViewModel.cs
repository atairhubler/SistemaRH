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
    private readonly AuditoriaViewModel _auditoriaViewModel;
    private readonly ConfiguracaoService _configService;

    public UserControl CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    private bool _modoEscuro;
    private string _temaIcone;
    private string _menuAtivo = "Dashboard";
    private bool _sidebarRecolhida;

    public string TemaIcone
    {
        get => _temaIcone;
        set => SetProperty(ref _temaIcone, value);
    }

    public string MenuAtivo
    {
        get => _menuAtivo;
        set => SetProperty(ref _menuAtivo, value);
    }

    public bool SidebarRecolhida
    {
        get => _sidebarRecolhida;
        set => SetProperty(ref _sidebarRecolhida, value);
    }

    public ICommand DashboardCommand { get; }
    public ICommand EmpresasCommand { get; }
    public ICommand FuncionariosCommand { get; }
    public ICommand FeriasCommand { get; }
    public ICommand RpaNfCommand { get; }
    public ICommand RelatoriosCommand { get; }
    public ICommand AuditoriaCommand { get; }
    public ICommand ConfiguracoesCommand { get; }
    public ICommand FolhaPagamentoCommand { get; }
    public ICommand AlternarTemaCommand { get; }
    public ICommand AlternarSidebarCommand { get; }

    public MainWindowViewModel(
        DashboardViewModel dashboardViewModel,
        EmpresasViewModel empresasViewModel,
        FuncionariosViewModel funcionariosViewModel,
        FeriasViewModel feriasViewModel,
        RpaNfViewModel rpaNfViewModel,
        RelatorioComparativoViewModel relatorioViewModel,
        ConfiguracoesViewModel configuracoesViewModel,
        FolhaPagamentoViewModel folhaPagamentoViewModel,
        AuditoriaViewModel auditoriaViewModel,
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
        _auditoriaViewModel = auditoriaViewModel;
        _configService = configService;

        _modoEscuro = configService.ModoEscuro;
        _temaIcone = _modoEscuro ? "☀️" : "🌙";

        DashboardCommand = new RelayCommand(_ => MostrarDashboard());
        EmpresasCommand = new RelayCommand(_ => MostrarEmpresas());
        FuncionariosCommand = new RelayCommand(_ => MostrarFuncionarios());
        FeriasCommand = new RelayCommand(_ => MostrarFerias());
        RpaNfCommand = new RelayCommand(_ => MostrarRpaNf());
        RelatoriosCommand = new RelayCommand(_ => MostrarRelatorios());
        AuditoriaCommand = new RelayCommand(_ => MostrarAuditoria());
        ConfiguracoesCommand = new RelayCommand(_ => MostrarConfiguracoes());
        FolhaPagamentoCommand = new RelayCommand(_ => MostrarFolhaPagamento());
        AlternarTemaCommand = new RelayCommand(_ => AlternarTema());
        AlternarSidebarCommand = new RelayCommand(_ => SidebarRecolhida = !SidebarRecolhida);

        MostrarDashboard();
    }

    private void MostrarDashboard()
    {
        CurrentView = new DashboardView { DataContext = _dashboardViewModel };
        StatusMessage = "Dashboard";
        MenuAtivo = "Dashboard";
        _ = _dashboardViewModel.CarregarAsync();
    }

    private void MostrarEmpresas()
    {
        CurrentView = new EmpresasListView { DataContext = _empresasViewModel };
        StatusMessage = "Empresas";
        MenuAtivo = "Empresas";
        _ = _empresasViewModel.CarregarAsync();
    }

    private void MostrarFuncionarios()
    {
        CurrentView = new FuncionariosListView { DataContext = _funcionariosViewModel };
        StatusMessage = "Funcionários";
        MenuAtivo = "Funcionarios";
        _ = _funcionariosViewModel.CarregarAsync();
    }

    private void MostrarFerias()
    {
        CurrentView = new FeriasView { DataContext = _feriasViewModel };
        StatusMessage = "Férias";
        MenuAtivo = "Ferias";
        _ = _feriasViewModel.CarregarAsync();
    }

    private void MostrarRpaNf()
    {
        CurrentView = new RpaNfView { DataContext = _rpaNfViewModel };
        StatusMessage = "RPA / NF";
        MenuAtivo = "RpaNf";
    }

    private void MostrarRelatorios()
    {
        CurrentView = new RelatorioComparativoView { DataContext = _relatorioViewModel };
        StatusMessage = "Relatórios";
        MenuAtivo = "Relatorios";
    }

    private void MostrarAuditoria()
    {
        CurrentView = new AuditoriaView { DataContext = _auditoriaViewModel };
        StatusMessage = "Auditoria";
        MenuAtivo = "Auditoria";
        _ = _auditoriaViewModel.CarregarAsync();
    }

    private void MostrarConfiguracoes()
    {
        CurrentView = new ConfiguracoesView { DataContext = _configuracoesViewModel };
        StatusMessage = "Configurações";
        MenuAtivo = "Configuracoes";
    }

    private void MostrarFolhaPagamento()
    {
        CurrentView = new FolhaPagamentoView { DataContext = _folhaPagamentoViewModel };
        StatusMessage = "Folha de Pagamento";
        MenuAtivo = "FolhaCLT";
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
