using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class FuncionariosViewModel : BaseViewModel
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IDialogService _dialogService;

    private List<FuncionarioDto> _todosFuncionarios = new();
    private ObservableCollection<FuncionarioDto> _funcionarios;
    private FuncionarioDto _selectedFuncionario;
    private string _filtroTipo = "Todos";
    private string _filtroNome = "";

    public ObservableCollection<FuncionarioDto> Funcionarios
    {
        get => _funcionarios;
        set => SetProperty(ref _funcionarios, value);
    }

    public FuncionarioDto SelectedFuncionario
    {
        get => _selectedFuncionario;
        set => SetProperty(ref _selectedFuncionario, value);
    }

    public string FiltroTipo
    {
        get => _filtroTipo;
        set => SetProperty(ref _filtroTipo, value);
    }

    public string FiltroNome
    {
        get => _filtroNome;
        set
        {
            if (SetProperty(ref _filtroNome, value))
                AplicarFiltro();
        }
    }

    public ICommand LoadedCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand NovoCommand { get; }
    public ICommand EditarCommand { get; }
    public ICommand DeletarCommand { get; }
    public ICommand ExportarCsvCommand { get; }

    public FuncionariosViewModel(
        IFuncionarioService funcionarioService,
        IDialogService dialogService)
    {
        _funcionarioService = funcionarioService;
        _dialogService = dialogService;

        Funcionarios = new ObservableCollection<FuncionarioDto>();

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
        RefreshCommand = new RelayCommand(_ => _ = CarregarAsync());
        NovoCommand = new RelayCommand(_ => NovoFuncionario());
        EditarCommand = new RelayCommand(param => _ = EditarFuncionario(param as FuncionarioDto));
        DeletarCommand = new RelayCommand(param => _ = DeletarFuncionario(param as FuncionarioDto));
        ExportarCsvCommand = new RelayCommand(_ => _ = ExportarCsvAsync());

        _ = CarregarAsync();
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando funcionários...";

            IEnumerable<FuncionarioDto> dados = FiltroTipo switch
            {
                "CLT" => await _funcionarioService.GetAllCltAsync(),
                "PJ" => await _funcionarioService.GetAllPJAsync(),
                _ => await _funcionarioService.GetAllAsync()
            };

            _todosFuncionarios = dados.ToList();
            AplicarFiltro();
            StatusMessage = $"{Funcionarios.Count} funcionário(s) encontrado(s)";
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
        var filtrados = string.IsNullOrWhiteSpace(_filtroNome)
            ? _todosFuncionarios
            : _todosFuncionarios.Where(f =>
                f.Nome?.Contains(_filtroNome, StringComparison.OrdinalIgnoreCase) == true).ToList();

        Funcionarios = new ObservableCollection<FuncionarioDto>(filtrados);
        StatusMessage = $"{Funcionarios.Count} funcionário(s) encontrado(s)";
    }

    private async void NovoFuncionario()
    {
        try
        {
            var tipo = await _dialogService.SelectTipoFuncionarioAsync();
            if (string.IsNullOrEmpty(tipo))
            {
                StatusMessage = "Operação cancelada";
                return;
            }

            if (tipo == "CLT")
            {
                var vm = App.ServiceProvider.GetService(typeof(FuncionarioCLTDetailViewModel)) as FuncionarioCLTDetailViewModel;
                vm?.PrepararNovo();
                var dialog = new Views.FuncionarioCLTDetailView();
                dialog.ShowDialog();
            }
            else
            {
                var vm = App.ServiceProvider.GetService(typeof(FuncionarioPJDetailViewModel)) as FuncionarioPJDetailViewModel;
                vm?.PrepararNovo();
                var dialog = new Views.FuncionarioPJDetailView();
                dialog.ShowDialog();
            }

            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }

    private async Task EditarFuncionario(FuncionarioDto? funcionario)
    {
        if (funcionario == null) return;

        if (funcionario is FuncionarioCLTDto cltDto)
        {
            var vm = App.ServiceProvider.GetService(typeof(FuncionarioCLTDetailViewModel)) as FuncionarioCLTDetailViewModel;
            vm?.PrepararEdicao(cltDto);
            var dialog = new Views.FuncionarioCLTDetailView();
            dialog.ShowDialog();
        }
        else if (funcionario is FuncionarioPJDto pjDto)
        {
            var vm = App.ServiceProvider.GetService(typeof(FuncionarioPJDetailViewModel)) as FuncionarioPJDetailViewModel;
            vm?.PrepararEdicao(pjDto);
            var dialog = new Views.FuncionarioPJDetailView();
            dialog.ShowDialog();
        }

        await CarregarAsync();
    }

    private async Task DeletarFuncionario(FuncionarioDto? funcionario)
    {
        if (funcionario == null) return;

        var confirma = await _dialogService.ShowConfirmAsync(
            "Confirmar Exclusão",
            $"Deseja excluir {funcionario.Nome}?");

        if (!confirma) return;

        try
        {
            IsLoading = true;
            var resultado = await _funcionarioService.DeleteAsync(funcionario.Id);
            if (resultado)
            {
                await _dialogService.ShowInfoAsync("Sucesso", "Funcionário excluído com sucesso.");
                await CarregarAsync();
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

    private async Task ExportarCsvAsync()
    {
        if (Funcionarios == null || Funcionarios.Count == 0)
        {
            await _dialogService.ShowErrorAsync("Exportar CSV", "Nenhum funcionário na lista para exportar.");
            return;
        }

        var caminho = await _dialogService.SalvarArquivoAsync("Arquivo CSV|*.csv");
        if (string.IsNullOrEmpty(caminho)) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Exportando CSV...";

            var sb = new StringBuilder();
            sb.AppendLine("Nome;Tipo;Email;Telefone;Status;Cargo/Razão Social;Salário/CNPJ");

            foreach (var f in Funcionarios)
            {
                if (f is FuncionarioCLTDto clt)
                    sb.AppendLine($"{Csv(clt.Nome)};CLT;{Csv(clt.Email)};{Csv(clt.Telefone)};{clt.Status};{Csv(clt.Cargo)};{clt.SalarioBruto:F2}");
                else if (f is FuncionarioPJDto pj)
                    sb.AppendLine($"{Csv(pj.Nome)};PJ;{Csv(pj.Email)};{Csv(pj.Telefone)};{pj.Status};{Csv(pj.RazaoSocial)};{Csv(pj.Cnpj)}");
                else
                    sb.AppendLine($"{Csv(f.Nome)};{f.Tipo};{Csv(f.Email)};{Csv(f.Telefone)};{f.Status};;");
            }

            await File.WriteAllTextAsync(caminho, sb.ToString(), Encoding.UTF8);
            await _dialogService.ShowInfoAsync("Exportar CSV", $"Arquivo salvo com sucesso!\n{caminho}");
            StatusMessage = $"CSV exportado: {Funcionarios.Count} funcionário(s)";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro ao exportar", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string Csv(string? valor) =>
        string.IsNullOrEmpty(valor) ? "" : valor.Contains(';') ? $"\"{valor}\"" : valor;
}
