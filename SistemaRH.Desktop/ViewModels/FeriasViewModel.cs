using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class FeriasViewModel : BaseViewModel
{
    private readonly IFeriasService _feriasService;
    private readonly IFuncionarioService _funcionarioService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<FuncionarioDto> _funcionarios;
    private FuncionarioDto _selectedFuncionario;
    private FeriasDto _feriasAtual;
    private DateTime _dataInicio = DateTime.Now;
    private DateTime _dataFim = DateTime.Now.AddDays(10);
    private bool _remunerada = true;

    public ObservableCollection<FuncionarioDto> Funcionarios
    {
        get => _funcionarios;
        set => SetProperty(ref _funcionarios, value);
    }

    public FuncionarioDto SelectedFuncionario
    {
        get => _selectedFuncionario;
        set
        {
            SetProperty(ref _selectedFuncionario, value);
            if (value != null)
                _ = CarregarFeriasAsync();
        }
    }

    public FeriasDto FeriasAtual
    {
        get => _feriasAtual;
        set => SetProperty(ref _feriasAtual, value);
    }

    public DateTime DataInicio
    {
        get => _dataInicio;
        set => SetProperty(ref _dataInicio, value);
    }

    public DateTime DataFim
    {
        get => _dataFim;
        set => SetProperty(ref _dataFim, value);
    }

    public bool Remunerada
    {
        get => _remunerada;
        set => SetProperty(ref _remunerada, value);
    }

    public ICommand LoadedCommand { get; }
    public ICommand CalcularCommand { get; }
    public ICommand AdicionarPeriodoCommand { get; }

    public FeriasViewModel(
        IFeriasService feriasService,
        IFuncionarioService funcionarioService,
        IDialogService dialogService)
    {
        _feriasService = feriasService;
        _funcionarioService = funcionarioService;
        _dialogService = dialogService;

        Funcionarios = new ObservableCollection<FuncionarioDto>();

        LoadedCommand = new RelayCommand(_ => _ = LoadFuncionariosAsync());
        CalcularCommand = new RelayCommand(_ => _ = CalcularDireitoAsync(), _ => SelectedFuncionario != null);
        AdicionarPeriodoCommand = new RelayCommand(_ => _ = AdicionarPeriodoAsync(), _ => FeriasAtual != null);
    }

    private async Task LoadFuncionariosAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando funcionários...";

            var todos = await _funcionarioService.GetAllAsync();
            Funcionarios = new ObservableCollection<FuncionarioDto>(todos);
            StatusMessage = $"Carregados {Funcionarios.Count} funcionários";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CarregarFeriasAsync()
    {
        if (SelectedFuncionario == null)
            return;

        try
        {
            IsLoading = true;
            FeriasAtual = await _feriasService.GetByFuncionarioAsync(SelectedFuncionario.Id);
            StatusMessage = $"Férias carregadas para {SelectedFuncionario.Nome}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CalcularDireitoAsync()
    {
        if (SelectedFuncionario == null)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "Calculando direito a férias...";

            FeriasAtual = await _feriasService.CalcularDireitoAsync(SelectedFuncionario.Id);
            await _dialogService.ShowInfoAsync(
                "Sucesso",
                $"Dias disponíveis: {FeriasAtual.DiasDisponiveis}\n" +
                $"Dias utilizados: {FeriasAtual.DiasUtilizados}\n" +
                $"Saldo: {FeriasAtual.SaldoDisponivel}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AdicionarPeriodoAsync()
    {
        if (FeriasAtual == null || DataFim < DataInicio)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "Adicionando período de férias...";

            var sucesso = await _feriasService.AdicionarPeriodoFeriasAsync(
                FeriasAtual.Id, DataInicio, DataFim, Remunerada);

            if (sucesso)
            {
                await _dialogService.ShowInfoAsync("Sucesso", "Período de férias adicionado");
                await CarregarFeriasAsync();
                DataInicio = DateTime.Now;
                DataFim = DateTime.Now.AddDays(10);
            }
            else
            {
                await _dialogService.ShowErrorAsync("Erro", "Não foi possível adicionar o período");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
