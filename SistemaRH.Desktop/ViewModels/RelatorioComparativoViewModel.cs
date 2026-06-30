using System.IO;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class RelatorioComparativoViewModel : BaseViewModel
{
    private readonly IRelatorioService _relatorioService;
    private readonly IDialogService _dialogService;

    private RelatorioComparativoDto _relatorioAtual;
    private int _mesSelecionado = DateTime.Now.Month;
    private int _anoSelecionado = DateTime.Now.Year;

    public RelatorioComparativoDto RelatorioAtual
    {
        get => _relatorioAtual;
        set => SetProperty(ref _relatorioAtual, value);
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
    public ICommand GerarRelatorioCommand { get; }
    public ICommand ExportarExcelCommand { get; }

    public RelatorioComparativoViewModel(
        IRelatorioService relatorioService,
        IDialogService dialogService)
    {
        _relatorioService = relatorioService;
        _dialogService = dialogService;

        LoadedCommand = new RelayCommand(_ => _ = GerarRelatorioAsync());
        GerarRelatorioCommand = new RelayCommand(_ => _ = GerarRelatorioAsync());
        ExportarExcelCommand = new RelayCommand(_ => _ = ExportarAsync(), _ => RelatorioAtual != null);
    }

    private async Task GerarRelatorioAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Gerando relatório...";

            // TODO: Passar empresaId quando disponível
            RelatorioAtual = await _relatorioService.GerarRelatorioCustosAsync(1, MesSelecionado, AnoSelecionado);
            StatusMessage = $"Relatório gerado para {MesSelecionado:00}/{AnoSelecionado}";
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

    private async Task ExportarAsync()
    {
        if (RelatorioAtual == null)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "Exportando para Excel...";

            var excelBytes = await _relatorioService.ExportarRelatorioExcelAsync(RelatorioAtual);

            var caminhoSalvar = await _dialogService.SalvarArquivoAsync("Arquivos Excel|*.xlsx|Todos os arquivos|*.*");
            if (string.IsNullOrEmpty(caminhoSalvar))
            {
                StatusMessage = "Exportação cancelada";
                return;
            }

            await File.WriteAllBytesAsync(caminhoSalvar, excelBytes);
            await _dialogService.ShowInfoAsync(
                "Sucesso",
                $"Relatório exportado com sucesso!\nArquivo: {Path.GetFileName(caminhoSalvar)}");

            StatusMessage = "Exportação concluída";
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
}
