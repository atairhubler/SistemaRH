using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Desktop.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IFeriasService _feriasService;
    private readonly IRelatorioService _relatorioService;
    private readonly IDialogService _dialogService;

    private int _totalFuncionarios;
    private int _totalCLT;
    private int _totalPJ;
    private decimal _custoCLTMensal;
    private decimal _custoPJMensal;
    private int _feriasProximas;
    private ObservableCollection<string> _funcionariosAlertaFerias = new();
    private bool _temAlertaFerias;

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

    public ICommand LoadedCommand { get; }

    public DashboardViewModel(
        IFuncionarioService funcionarioService,
        IFeriasService feriasService,
        IRelatorioService relatorioService,
        IDialogService dialogService)
    {
        _funcionarioService = funcionarioService;
        _feriasService = feriasService;
        _relatorioService = relatorioService;
        _dialogService = dialogService;

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando dados...";

            var todos = await _funcionarioService.GetAllAsync();
            var clts = await _funcionarioService.GetAllCltAsync();
            var pjs = await _funcionarioService.GetAllPJAsync();

            TotalFuncionarios = todos.Count();
            TotalCLT = clts.Count();
            TotalPJ = pjs.Count();

            CustoCLTMensal = clts.Sum(c => (c as FuncionarioCLTDto)?.SalarioBruto ?? 0);
            CustoPJMensal = 0;

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
}
