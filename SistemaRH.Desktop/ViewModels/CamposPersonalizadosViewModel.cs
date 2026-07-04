using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class CamposPersonalizadosViewModel : BaseViewModel
{
    private readonly ICampoPersonalizadoService _campoService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<CampoPersonalizadoDto> _campos = new();

    public ObservableCollection<CampoPersonalizadoDto> Campos
    {
        get => _campos;
        set => SetProperty(ref _campos, value);
    }

    public ICommand LoadedCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand ExcluirCommand { get; }

    public CamposPersonalizadosViewModel(ICampoPersonalizadoService campoService, IDialogService dialogService)
    {
        _campoService = campoService;
        _dialogService = dialogService;

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
        NovoCommand = new RelayCommand(_ => NovoCampo());
        EditarCommand = new RelayCommand(param => EditarCampo(param as CampoPersonalizadoDto));
        ExcluirCommand = new RelayCommand(param => _ = ExcluirAsync(param as CampoPersonalizadoDto));
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            var lista = await _campoService.GetAllAsync();
            Campos = new ObservableCollection<CampoPersonalizadoDto>(lista);
            StatusMessage = $"{Campos.Count} campo(s) personalizado(s)";
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

    private void NovoCampo()
    {
        var vm = App.ServiceProvider.GetService(typeof(CampoPersonalizadoDetailViewModel)) as CampoPersonalizadoDetailViewModel;
        vm?.PrepararNovo();
        var dialog = new Views.CampoPersonalizadoDetailView();
        dialog.ShowDialog();
        _ = CarregarAsync();
    }

    private void EditarCampo(CampoPersonalizadoDto? campo)
    {
        if (campo == null) return;
        var vm = App.ServiceProvider.GetService(typeof(CampoPersonalizadoDetailViewModel)) as CampoPersonalizadoDetailViewModel;
        vm?.PrepararEdicao(campo);
        var dialog = new Views.CampoPersonalizadoDetailView();
        dialog.ShowDialog();
        _ = CarregarAsync();
    }

    private async Task ExcluirAsync(CampoPersonalizadoDto? campo)
    {
        if (campo == null) return;

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Confirmar exclusão",
            $"Deseja excluir o campo '{campo.Rotulo}'? Valores já preenchidos em funcionários serão mantidos, mas o campo deixará de aparecer nos formulários.");

        if (!confirmar) return;

        try
        {
            await _campoService.DeleteAsync(campo.Id);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }
}
