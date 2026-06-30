using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class TabelasCalculoViewModel : BaseViewModel
{
    private readonly TabelasFolhaService _tabelasService;
    private readonly IDialogService _dialogService;

    private decimal _deducaoPorDependenteConfig;

    public decimal DeducaoPorDependenteConfig
    {
        get => _deducaoPorDependenteConfig;
        set => SetProperty(ref _deducaoPorDependenteConfig, value);
    }

    public ObservableCollection<FaixaINSSItem> FaixasINSS { get; } = new();
    public ObservableCollection<FaixaIRRFItem> FaixasIRRF { get; } = new();

    public Action? AoFechar { get; set; }

    public ICommand SalvarTabelasCommand { get; }
    public ICommand RestaurarPadraoCommand { get; }

    public TabelasCalculoViewModel(TabelasFolhaService tabelasService, IDialogService dialogService)
    {
        _tabelasService = tabelasService;
        _dialogService = dialogService;

        SalvarTabelasCommand = new RelayCommand(_ => _ = SalvarTabelasAsync());
        RestaurarPadraoCommand = new RelayCommand(_ => RestaurarPadrao());
    }

    public void CarregarTabelas()
    {
        var config = _tabelasService.GetTabelas();
        DeducaoPorDependenteConfig = config.DeducaoPorDependente;

        FaixasINSS.Clear();
        var inssOrdenado = config.FaixasINSS.OrderBy(f => f.LimiteSuperior).ToList();
        for (int i = 0; i < inssOrdenado.Count; i++)
        {
            var f = inssOrdenado[i];
            FaixasINSS.Add(new FaixaINSSItem
            {
                Descricao = $"{i + 1}ª Faixa",
                LimiteSuperior = f.LimiteSuperior,
                AliquotaPercent = Math.Round(f.Aliquota * 100, 4)
            });
        }

        FaixasIRRF.Clear();
        var irrf = config.FaixasIRRF.OrderBy(f => f.LimiteSuperior).ToList();
        for (int i = 0; i < irrf.Count; i++)
        {
            var f = irrf[i];
            bool isUltima = i == irrf.Count - 1;
            FaixasIRRF.Add(new FaixaIRRFItem
            {
                Descricao = isUltima ? "Acima" : $"{i + 1}ª Faixa",
                IsUltimaFaixa = isUltima,
                LimiteSuperior = f.LimiteSuperior,
                AliquotaPercent = Math.Round(f.Aliquota * 100, 4),
                Deducao = f.Deducao
            });
        }
    }

    private async Task SalvarTabelasAsync()
    {
        try
        {
            _tabelasService.Salvar(ObterTabelasDaUI());
            StatusMessage = "Tabelas salvas com sucesso!";
            await _dialogService.ShowInfoAsync("Tabelas Salvas",
                "As tabelas de cálculo foram salvas com sucesso!\n\nO próximo cálculo já usará os novos valores.");
            AoFechar?.Invoke();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro ao Salvar", ex.Message);
        }
    }

    private void RestaurarPadrao()
    {
        _tabelasService.RestaurarPadrao();
        CarregarTabelas();
        StatusMessage = "Tabelas restauradas para o padrão 2024.";
    }

    private TabelasFolhaConfig ObterTabelasDaUI() => new()
    {
        DeducaoPorDependente = DeducaoPorDependenteConfig,
        FaixasINSS = FaixasINSS.Select(f => new FaixaINSSConfig
        {
            LimiteSuperior = f.LimiteSuperior,
            Aliquota = f.AliquotaPercent / 100
        }).ToList(),
        FaixasIRRF = FaixasIRRF.Select(f => new FaixaIRRFConfig
        {
            LimiteSuperior = f.LimiteSuperior,
            Aliquota = f.AliquotaPercent / 100,
            Deducao = f.Deducao
        }).ToList()
    };
}

public class FaixaINSSItem : INotifyPropertyChanged
{
    public string Descricao { get; init; } = "";

    private decimal _limiteSuperior;
    public decimal LimiteSuperior
    {
        get => _limiteSuperior;
        set { _limiteSuperior = value; OnPropertyChanged(); }
    }

    private decimal _aliquotaPercent;
    public decimal AliquotaPercent
    {
        get => _aliquotaPercent;
        set { _aliquotaPercent = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class FaixaIRRFItem : INotifyPropertyChanged
{
    public string Descricao { get; init; } = "";
    public bool IsUltimaFaixa { get; init; }
    public bool PodeEditarLimite => !IsUltimaFaixa;
    public string LimiteSuperiorDisplay => IsUltimaFaixa ? "∞" : LimiteSuperior.ToString("N2");

    private decimal _limiteSuperior;
    public decimal LimiteSuperior
    {
        get => _limiteSuperior;
        set { _limiteSuperior = value; OnPropertyChanged(); OnPropertyChanged(nameof(LimiteSuperiorDisplay)); }
    }

    private decimal _aliquotaPercent;
    public decimal AliquotaPercent
    {
        get => _aliquotaPercent;
        set { _aliquotaPercent = value; OnPropertyChanged(); }
    }

    private decimal _deducao;
    public decimal Deducao
    {
        get => _deducao;
        set { _deducao = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
