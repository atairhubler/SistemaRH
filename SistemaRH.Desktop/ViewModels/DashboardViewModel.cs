using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Desktop.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private static readonly EmpresaDto TodasAsEmpresas = new() { Id = 0, RazaoSocial = "Todas as Empresas" };

    private readonly IFuncionarioService _funcionarioService;
    private readonly IEmpresaService _empresaService;
    private readonly IFeriasService _feriasService;
    private readonly IDialogService _dialogService;

    private int _totalFuncionarios;
    private int _totalCLT;
    private int _totalPJ;
    private int _totalEstagiario;
    private decimal _custoCLTMensal;
    private decimal _custoPJMensal;
    private int _feriasProximas;
    private ObservableCollection<string> _funcionariosAlertaFerias = new();
    private bool _temAlertaFerias;
    private ObservableCollection<string> _aniversariantes = new();
    private bool _temAniversariantes;
    private ObservableCollection<string> _temposDeCasa = new();
    private bool _temTemposDeCasa;
    private ObservableCollection<EmpresaDto> _empresas = new();
    private EmpresaDto? _empresaSelecionada;

    public int TotalFuncionarios
    {
        get => _totalFuncionarios;
        set => SetProperty(ref _totalFuncionarios, value);
    }

    public int TotalCLT
    {
        get => _totalCLT;
        set => SetProperty(ref _totalCLT, value);
    }

    public int TotalPJ
    {
        get => _totalPJ;
        set => SetProperty(ref _totalPJ, value);
    }

    public int TotalEstagiario
    {
        get => _totalEstagiario;
        set => SetProperty(ref _totalEstagiario, value);
    }

    public decimal CustoCLTMensal
    {
        get => _custoCLTMensal;
        set => SetProperty(ref _custoCLTMensal, value);
    }

    public decimal CustoPJMensal
    {
        get => _custoPJMensal;
        set => SetProperty(ref _custoPJMensal, value);
    }

    public int FeriasProximas
    {
        get => _feriasProximas;
        set => SetProperty(ref _feriasProximas, value);
    }

    public ObservableCollection<string> FuncionariosAlertaFerias
    {
        get => _funcionariosAlertaFerias;
        set => SetProperty(ref _funcionariosAlertaFerias, value);
    }

    public bool TemAlertaFerias
    {
        get => _temAlertaFerias;
        set => SetProperty(ref _temAlertaFerias, value);
    }

    public ObservableCollection<string> Aniversariantes
    {
        get => _aniversariantes;
        set => SetProperty(ref _aniversariantes, value);
    }

    public bool TemAniversariantes
    {
        get => _temAniversariantes;
        set => SetProperty(ref _temAniversariantes, value);
    }

    public ObservableCollection<string> TemposDeCasa
    {
        get => _temposDeCasa;
        set => SetProperty(ref _temposDeCasa, value);
    }

    public bool TemTemposDeCasa
    {
        get => _temTemposDeCasa;
        set => SetProperty(ref _temTemposDeCasa, value);
    }

    public ObservableCollection<EmpresaDto> Empresas
    {
        get => _empresas;
        set => SetProperty(ref _empresas, value);
    }

    public EmpresaDto? EmpresaSelecionada
    {
        get => _empresaSelecionada;
        set
        {
            if (SetProperty(ref _empresaSelecionada, value))
                _ = AtualizarDadosAsync();
        }
    }

    public ICommand LoadedCommand { get; }

    public DashboardViewModel(
        IFuncionarioService funcionarioService,
        IEmpresaService empresaService,
        IFeriasService feriasService,
        IDialogService dialogService)
    {
        _funcionarioService = funcionarioService;
        _empresaService = empresaService;
        _feriasService = feriasService;
        _dialogService = dialogService;

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
    }

    private async Task CarregarEmpresasAsync()
    {
        try
        {
            var idAtual = _empresaSelecionada?.Id ?? 0;
            var lista = await _empresaService.GetAllAsync();
            var combinado = new ObservableCollection<EmpresaDto> { TodasAsEmpresas };
            foreach (var empresa in lista) combinado.Add(empresa);
            Empresas = combinado;
            _empresaSelecionada = combinado.FirstOrDefault(e => e.Id == idAtual) ?? TodasAsEmpresas;
            OnPropertyChanged(nameof(EmpresaSelecionada));
        }
        catch { }
    }

    public async Task CarregarAsync()
    {
        await CarregarEmpresasAsync();
        await AtualizarDadosAsync();
    }

    private async Task AtualizarDadosAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando dados...";

            var todos = (await _funcionarioService.GetAllAsync()).ToList();
            var clts = (await _funcionarioService.GetAllCltAsync()).ToList();
            var pjs = (await _funcionarioService.GetAllPJAsync()).ToList();
            var estagiarios = (await _funcionarioService.GetAllEstagiarioAsync()).ToList();

            var empresaId = EmpresaSelecionada?.Id ?? 0;
            if (empresaId > 0)
            {
                todos = todos.Where(f => f.EmpresaId == empresaId).ToList();
                clts = clts.Where(c => c.EmpresaId == empresaId).ToList();
                pjs = pjs.Where(p => p.EmpresaId == empresaId).ToList();
                estagiarios = estagiarios.Where(e => e.EmpresaId == empresaId).ToList();
            }

            TotalFuncionarios = todos.Count();
            TotalCLT = clts.Count();
            TotalPJ = pjs.Count();
            TotalEstagiario = estagiarios.Count();

            CustoCLTMensal = clts.Sum(c => c.SalarioBruto);
            CustoPJMensal = 0;

            AtualizarAniversariosEAdmissoes(clts, pjs, estagiarios);

            var alertas = new List<string>();
            foreach (var funcionario in todos.Where(f => f.IsAtivo))
            {
                if (!await _feriasService.EhElegivelAsync(funcionario.Id))
                    continue;

                await _feriasService.GarantirPeriodosAsync(funcionario.Id);
                var periodos = await _feriasService.GetPeriodosAquisitivosAsync(funcionario.Id);
                var problematico = periodos
                    .Where(p => p.Status == StatusPeriodoAquisitivo.Pendente || p.Status == StatusPeriodoAquisitivo.Vencido)
                    .OrderBy(p => p.NumeroPeriodo)
                    .FirstOrDefault();

                if (problematico == null) continue;

                var rotulo = problematico.Status == StatusPeriodoAquisitivo.Vencido ? "vencidas" : "pendentes";
                alertas.Add($"{funcionario.Nome} — férias {rotulo} (período aquisitivo nº {problematico.NumeroPeriodo})");
            }

            FuncionariosAlertaFerias = new ObservableCollection<string>(alertas);
            TemAlertaFerias = alertas.Count > 0;

            StatusMessage = $"Dashboard atualizado: {TotalFuncionarios} funcionários";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar dashboard: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AtualizarAniversariosEAdmissoes(
        List<FuncionarioCLTDto> clts, List<FuncionarioPJDto> pjs, List<FuncionarioEstagiarioDto> estagiarios)
    {
        var mesAtual = DateTime.Now.Month;
        var anoAtual = DateTime.Now.Year;

        var aniversariantes = new List<(int Dia, string Texto)>();
        foreach (var c in clts.Where(x => x.IsAtivo && x.DataNascimento.Month == mesAtual))
            aniversariantes.Add((c.DataNascimento.Day, $"{c.Nome} — dia {c.DataNascimento.Day:00}"));
        foreach (var p in pjs.Where(x => x.IsAtivo && x.DataNascimento.Month == mesAtual))
            aniversariantes.Add((p.DataNascimento.Day, $"{p.Nome} — dia {p.DataNascimento.Day:00}"));
        foreach (var e in estagiarios.Where(x => x.IsAtivo && x.DataNascimento.Month == mesAtual))
            aniversariantes.Add((e.DataNascimento.Day, $"{e.Nome} — dia {e.DataNascimento.Day:00}"));

        Aniversariantes = new ObservableCollection<string>(aniversariantes.OrderBy(a => a.Dia).Select(a => a.Texto));
        TemAniversariantes = aniversariantes.Count > 0;

        var temposDeCasa = new List<(int Dia, string Texto)>();
        foreach (var c in clts.Where(x => x.IsAtivo && x.DataAdmissao.Month == mesAtual && x.DataAdmissao.Year < anoAtual))
        {
            var anos = anoAtual - c.DataAdmissao.Year;
            temposDeCasa.Add((c.DataAdmissao.Day, $"{c.Nome} — {anos} ano(s) de empresa (dia {c.DataAdmissao.Day:00})"));
        }
        foreach (var e in estagiarios.Where(x => x.IsAtivo && x.DataAdmissao.Month == mesAtual && x.DataAdmissao.Year < anoAtual))
        {
            var anos = anoAtual - e.DataAdmissao.Year;
            temposDeCasa.Add((e.DataAdmissao.Day, $"{e.Nome} — {anos} ano(s) de empresa (dia {e.DataAdmissao.Day:00})"));
        }

        TemposDeCasa = new ObservableCollection<string>(temposDeCasa.OrderBy(a => a.Dia).Select(a => a.Texto));
        TemTemposDeCasa = temposDeCasa.Count > 0;
    }
}
