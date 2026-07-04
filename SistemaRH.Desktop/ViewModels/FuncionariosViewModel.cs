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
    private readonly IExcelImportService _excelImportService;
    private readonly IDialogService _dialogService;

    private List<FuncionarioDto> _todosFuncionarios = new();
    private ObservableCollection<FuncionarioDto> _funcionarios;
    private FuncionarioDto _selectedFuncionario;
    private string _filtroTipo = "Todos";
    private string _filtroStatus = "Ativos";
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

    public string FiltroStatus
    {
        get => _filtroStatus;
        set
        {
            if (SetProperty(ref _filtroStatus, value))
                AplicarFiltro();
        }
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
    public ICommand PerfilCommand { get; }
    public ICommand AtivarCommand { get; }
    public ICommand DesativarCommand { get; }
    public ICommand ExportarCsvCommand { get; }
    public ICommand ImportarPlanilhaCommand { get; }

    public FuncionariosViewModel(
        IFuncionarioService funcionarioService,
        IExcelImportService excelImportService,
        IDialogService dialogService)
    {
        _funcionarioService = funcionarioService;
        _excelImportService = excelImportService;
        _dialogService = dialogService;

        Funcionarios = new ObservableCollection<FuncionarioDto>();

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
        RefreshCommand = new RelayCommand(_ => _ = CarregarAsync());
        NovoCommand = new RelayCommand(_ => NovoFuncionario());
        EditarCommand = new RelayCommand(param => _ = EditarFuncionario(param as FuncionarioDto));
        PerfilCommand = new RelayCommand(param => AbrirPerfil(param as FuncionarioDto));
        AtivarCommand = new RelayCommand(param => _ = AtivarFuncionario(param as FuncionarioDto));
        DesativarCommand = new RelayCommand(param => _ = DesativarFuncionario(param as FuncionarioDto));
        ExportarCsvCommand = new RelayCommand(_ => _ = ExportarCsvAsync());
        ImportarPlanilhaCommand = new RelayCommand(_ => _ = ImportarPlanilhaAsync());

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
                "Estagiário" => await _funcionarioService.GetAllEstagiarioAsync(),
                _ => await _funcionarioService.GetAllAsync()
            };

            _todosFuncionarios = dados.ToList();
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
        IEnumerable<FuncionarioDto> filtrados = _todosFuncionarios;

        filtrados = _filtroStatus switch
        {
            "Ativos" => filtrados.Where(f => f.IsAtivo),
            "Inativos" => filtrados.Where(f => !f.IsAtivo),
            _ => filtrados
        };

        if (!string.IsNullOrWhiteSpace(_filtroNome))
            filtrados = filtrados.Where(f =>
                f.Nome?.Contains(_filtroNome, StringComparison.OrdinalIgnoreCase) == true);

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
            else if (tipo == "Estagiario")
            {
                var vm = App.ServiceProvider.GetService(typeof(FuncionarioEstagiarioDetailViewModel)) as FuncionarioEstagiarioDetailViewModel;
                vm?.PrepararNovo();
                var dialog = new Views.FuncionarioEstagiarioDetailView();
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
        else if (funcionario is FuncionarioEstagiarioDto estagiarioDto)
        {
            var vm = App.ServiceProvider.GetService(typeof(FuncionarioEstagiarioDetailViewModel)) as FuncionarioEstagiarioDetailViewModel;
            vm?.PrepararEdicao(estagiarioDto);
            var dialog = new Views.FuncionarioEstagiarioDetailView();
            dialog.ShowDialog();
        }

        await CarregarAsync();
    }

    private void AbrirPerfil(FuncionarioDto? funcionario)
    {
        if (funcionario == null) return;

        var vm = App.ServiceProvider.GetService(typeof(FuncionarioPerfilViewModel)) as FuncionarioPerfilViewModel;
        vm?.Preparar(funcionario);
        var dialog = new Views.FuncionarioPerfilView();
        dialog.ShowDialog();
    }

    private async Task AtivarFuncionario(FuncionarioDto? funcionario)
    {
        if (funcionario == null) return;

        try
        {
            IsLoading = true;
            await _funcionarioService.AtivarAsync(funcionario.Id);
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

    private async Task DesativarFuncionario(FuncionarioDto? funcionario)
    {
        if (funcionario == null) return;

        var confirma = await _dialogService.ShowConfirmAsync(
            "Confirmar Desativação",
            $"Deseja desativar {funcionario.Nome}?");

        if (!confirma) return;

        try
        {
            IsLoading = true;
            await _funcionarioService.DesativarAsync(funcionario.Id);
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

    private async Task ImportarPlanilhaAsync()
    {
        var caminho = await _dialogService.AbrirArquivoAsync("Arquivos Excel|*.xlsx");
        if (string.IsNullOrEmpty(caminho)) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Importando planilha...";

            var resultado = await _excelImportService.ImportarAgilTelecomAsync(caminho);

            var mensagem = $"PJ: {resultado.PjImportados} importados, {resultado.PjIgnorados} já existentes\n" +
                           $"CLT: {resultado.CltImportados} importados, {resultado.CltIgnorados} já existentes\n" +
                           $"Estagiários: {resultado.EstagiarioImportados} importados, {resultado.EstagiarioIgnorados} já existentes";

            if (resultado.Avisos.Count > 0)
                mensagem += "\n\nAvisos:\n" + string.Join("\n", resultado.Avisos);

            await _dialogService.ShowInfoAsync("Importação concluída", mensagem);
            StatusMessage = "Importação concluída";
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao importar: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro ao importar", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string Csv(string? valor) =>
        string.IsNullOrEmpty(valor) ? "" : valor.Contains(';') ? $"\"{valor}\"" : valor;
}
