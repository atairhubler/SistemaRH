using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class EmpresasViewModel : BaseViewModel
{
    private readonly IEmpresaService _empresaService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<EmpresaDto> _empresas = new();
    private EmpresaDto? _empresaSelecionada;

    public ObservableCollection<EmpresaDto> Empresas
    {
        get => _empresas;
        set => SetProperty(ref _empresas, value);
    }

    public EmpresaDto? EmpresaSelecionada
    {
        get => _empresaSelecionada;
        set => SetProperty(ref _empresaSelecionada, value);
    }

    public ICommand NovaEmpresaCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand ExcluirCommand { get; }
    public ICommand AtualizarCommand { get; }

    public EmpresasViewModel(IEmpresaService empresaService, IDialogService dialogService)
    {
        _empresaService = empresaService;
        _dialogService = dialogService;

        NovaEmpresaCommand = new RelayCommand(_ => AbrirNovaEmpresa());
        EditarCommand = new RelayCommand(param => AbrirEditar(param as EmpresaDto));
        ExcluirCommand = new RelayCommand(param => _ = ExcluirAsync(param as EmpresaDto));
        AtualizarCommand = new RelayCommand(_ => _ = CarregarAsync());

        _ = CarregarAsync();
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando empresas...";

            var lista = await _empresaService.GetAllAsync();
            Empresas = new ObservableCollection<EmpresaDto>(lista);
            StatusMessage = $"{Empresas.Count} empresa(s) encontrada(s)";
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

    private void AbrirNovaEmpresa()
    {
        var vm = new EmpresaDetailViewModel(_empresaService, _dialogService);
        vm.Salvo += async () => await CarregarAsync();

        var view = new Views.EmpresaDetailView();
        view.DataContext = vm;
        view.ShowDialog();
    }

    private void AbrirEditar(EmpresaDto? empresa)
    {
        if (empresa == null) return;

        var vm = new EmpresaDetailViewModel(_empresaService, _dialogService, empresa);
        vm.Salvo += async () => await CarregarAsync();

        var view = new Views.EmpresaDetailView();
        view.DataContext = vm;
        view.ShowDialog();
    }

    private async Task ExcluirAsync(EmpresaDto? empresa)
    {
        if (empresa == null) return;

        var confirmar = await _dialogService.ShowConfirmAsync(
            "Confirmar exclusão",
            $"Deseja excluir a empresa '{empresa.RazaoSocial}'?");

        if (!confirmar) return;

        try
        {
            await _empresaService.DeleteAsync(empresa.Id);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }
}
