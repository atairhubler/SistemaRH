using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class AuditoriaViewModel : BaseViewModel
{
    private readonly IAuditoriaService _auditoriaService;
    private readonly IDialogService _dialogService;

    private List<LogAuditoriaDto> _todosLogs = new();
    private ObservableCollection<LogAuditoriaDto> _logs = new();
    private LogAuditoriaDto? _logSelecionado;
    private string _filtroUsuario = "";
    private DateTime? _dataInicio;
    private DateTime? _dataFim;

    public ObservableCollection<LogAuditoriaDto> Logs
    {
        get => _logs;
        set => SetProperty(ref _logs, value);
    }

    public LogAuditoriaDto? LogSelecionado
    {
        get => _logSelecionado;
        set
        {
            if (SetProperty(ref _logSelecionado, value))
                OnPropertyChanged(nameof(TemLogSelecionado));
        }
    }

    public bool TemLogSelecionado => LogSelecionado != null;

    public string FiltroUsuario
    {
        get => _filtroUsuario;
        set
        {
            if (SetProperty(ref _filtroUsuario, value))
                AplicarFiltro();
        }
    }

    public DateTime? DataInicio
    {
        get => _dataInicio;
        set
        {
            if (SetProperty(ref _dataInicio, value))
                AplicarFiltro();
        }
    }

    public DateTime? DataFim
    {
        get => _dataFim;
        set
        {
            if (SetProperty(ref _dataFim, value))
                AplicarFiltro();
        }
    }

    public ICommand LoadedCommand { get; }
    public ICommand RefreshCommand { get; }

    public AuditoriaViewModel(IAuditoriaService auditoriaService, IDialogService dialogService)
    {
        _auditoriaService = auditoriaService;
        _dialogService = dialogService;

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
        RefreshCommand = new RelayCommand(_ => _ = CarregarAsync());
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando log de auditoria...";

            _todosLogs = (await _auditoriaService.GetAllAsync()).ToList();
            AplicarFiltro();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AplicarFiltro()
    {
        IEnumerable<LogAuditoriaDto> filtrados = _todosLogs;

        if (!string.IsNullOrWhiteSpace(_filtroUsuario))
            filtrados = filtrados.Where(l => l.Usuario.Contains(_filtroUsuario, StringComparison.OrdinalIgnoreCase));

        if (_dataInicio.HasValue)
            filtrados = filtrados.Where(l => l.DataHora.Date >= _dataInicio.Value.Date);

        if (_dataFim.HasValue)
            filtrados = filtrados.Where(l => l.DataHora.Date <= _dataFim.Value.Date);

        Logs = new ObservableCollection<LogAuditoriaDto>(filtrados);
        StatusMessage = $"{Logs.Count} registro(s) de auditoria";
    }
}
