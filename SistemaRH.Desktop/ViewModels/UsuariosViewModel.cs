using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class UsuariosViewModel : BaseViewModel
{
    private readonly IUsuarioService _usuarioService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<UsuarioDto> _usuarios = new();

    public ObservableCollection<UsuarioDto> Usuarios
    {
        get => _usuarios;
        set => SetProperty(ref _usuarios, value);
    }

    public ICommand LoadedCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand AtivarCommand { get; }
    public ICommand DesativarCommand { get; }

    public UsuariosViewModel(IUsuarioService usuarioService, IDialogService dialogService)
    {
        _usuarioService = usuarioService;
        _dialogService = dialogService;

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
        NovoCommand = new RelayCommand(_ => NovoUsuario());
        EditarCommand = new RelayCommand(param => EditarUsuario(param as UsuarioDto));
        AtivarCommand = new RelayCommand(param => _ = AlterarAtivoAsync(param as UsuarioDto, true));
        DesativarCommand = new RelayCommand(param => _ = AlterarAtivoAsync(param as UsuarioDto, false));
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            var lista = await _usuarioService.GetAllAsync();
            Usuarios = new ObservableCollection<UsuarioDto>(lista);
            StatusMessage = $"{Usuarios.Count} usuário(s)";
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

    private void NovoUsuario()
    {
        var vm = App.ServiceProvider.GetService(typeof(UsuarioDetailViewModel)) as UsuarioDetailViewModel;
        vm?.PrepararNovo();
        var dialog = new Views.UsuarioDetailView();
        dialog.ShowDialog();
        _ = CarregarAsync();
    }

    private void EditarUsuario(UsuarioDto? usuario)
    {
        if (usuario == null) return;
        var vm = App.ServiceProvider.GetService(typeof(UsuarioDetailViewModel)) as UsuarioDetailViewModel;
        vm?.PrepararEdicao(usuario);
        var dialog = new Views.UsuarioDetailView();
        dialog.ShowDialog();
        _ = CarregarAsync();
    }

    private async Task AlterarAtivoAsync(UsuarioDto? usuario, bool ativo)
    {
        if (usuario == null) return;

        try
        {
            await _usuarioService.UpdateAsync(usuario.Id, usuario.NomeUsuario, null, ativo);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }
}
