using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class HistoricoSalarialViewModel : BaseViewModel
{
    private readonly IHistoricoSalarialService _historicoService;
    private readonly IDialogService _dialogService;

    private int _funcionarioId;
    private string _rotuloValor = "Valor";
    private ObservableCollection<SalarioHistoricoDto> _historico = new();
    private DateTime _novaData = DateTime.Now;
    private string _novoMotivo = "";
    private decimal _novoValor;
    private string _novaObservacao = "";
    private decimal _valorAtual;

    public string RotuloValor { get => _rotuloValor; set => SetProperty(ref _rotuloValor, value); }
    public string Titulo => $"Histórico de {RotuloValor}";

    public ObservableCollection<SalarioHistoricoDto> Historico
    {
        get => _historico;
        set => SetProperty(ref _historico, value);
    }

    public DateTime NovaData { get => _novaData; set => SetProperty(ref _novaData, value); }
    public string NovoMotivo { get => _novoMotivo; set => SetProperty(ref _novoMotivo, value); }
    public decimal NovoValor { get => _novoValor; set => SetProperty(ref _novoValor, value); }
    public string NovaObservacao { get => _novaObservacao; set => SetProperty(ref _novaObservacao, value); }
    public decimal ValorAtual { get => _valorAtual; private set => SetProperty(ref _valorAtual, value); }

    public ICommand AdicionarCommand { get; }
    public ICommand FecharCommand { get; }

    public Action? FecharJanela { get; set; }

    public HistoricoSalarialViewModel(IHistoricoSalarialService historicoService, IDialogService dialogService)
    {
        _historicoService = historicoService;
        _dialogService = dialogService;

        AdicionarCommand = new RelayCommand(_ => _ = AdicionarAsync());
        FecharCommand = new RelayCommand(_ => FecharJanela?.Invoke());
    }

    public void Preparar(int funcionarioId, string rotuloValor)
    {
        _funcionarioId = funcionarioId;
        RotuloValor = rotuloValor;
        OnPropertyChanged(nameof(Titulo));
        NovaData = DateTime.Now;
        NovoMotivo = "";
        NovoValor = 0;
        NovaObservacao = "";
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            var lista = await _historicoService.GetByFuncionarioAsync(_funcionarioId);
            Historico = new ObservableCollection<SalarioHistoricoDto>(lista);
            if (Historico.Count > 0)
                ValorAtual = Historico[0].SalarioBruto;
            StatusMessage = $"{Historico.Count} registro(s)";
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

    private async Task AdicionarAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoMotivo) || NovoValor <= 0)
        {
            await _dialogService.ShowErrorAsync("Validação", "Informe o motivo e um valor maior que zero.");
            return;
        }

        try
        {
            IsLoading = true;
            await _historicoService.AdicionarAsync(_funcionarioId, NovaData, NovoValor, NovoMotivo, NovaObservacao);
            NovoMotivo = ""; NovoValor = 0; NovaObservacao = ""; NovaData = DateTime.Now;
            await CarregarAsync();
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
