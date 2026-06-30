using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class RpaNfViewModel : BaseViewModel
{
    private readonly IRpaNfService _rpaNfService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<RpaNfDto> _rpasNfs;
    private int _mesSelecionado = DateTime.Now.Month;
    private int _anoSelecionado = DateTime.Now.Year;

    public ObservableCollection<RpaNfDto> RpasNfs
    {
        get => _rpasNfs;
        set => SetProperty(ref _rpasNfs, value);
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

    public ICommand LoadedCommand { get; }
    public ICommand FiltrarCommand { get; }
    public ICommand NovaRpaCommand { get; }

    public RpaNfViewModel(
        IRpaNfService rpaNfService,
        IDialogService dialogService)
    {
        _rpaNfService = rpaNfService;
        _dialogService = dialogService;

        RpasNfs = new ObservableCollection<RpaNfDto>();

        LoadedCommand = new RelayCommand(_ => _ = LoadRpasNfsAsync());
        FiltrarCommand = new RelayCommand(_ => _ = LoadRpasNfsAsync());
        NovaRpaCommand = new RelayCommand(_ => NovaRpa());
    }

    private async Task LoadRpasNfsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando RPA/NF...";

            // TODO: Passar empresaId quando disponível
            var rpasNfs = await _rpaNfService.GetByCompetenciaAsync(1, MesSelecionado, AnoSelecionado);
            RpasNfs = new ObservableCollection<RpaNfDto>(rpasNfs);
            StatusMessage = $"Carregadas {RpasNfs.Count} RPA/NF para {MesSelecionado:00}/{AnoSelecionado}";
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

    private void NovaRpa()
    {
        try
        {
            var dialog = new Views.NovaRpaDialog();
            if (dialog.ShowDialog() == true)
            {
                StatusMessage = "Nova RPA/NF registrada com sucesso";
                _ = LoadRpasNfsAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
            _ = _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }
}
