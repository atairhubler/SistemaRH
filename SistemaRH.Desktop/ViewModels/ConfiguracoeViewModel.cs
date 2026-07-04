using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;
using WpfApp = System.Windows.Application;

namespace SistemaRH.Desktop.ViewModels;

public class ConfiguracoesViewModel : BaseViewModel
{
    private readonly IBackupService _backupService;
    private readonly IDialogService _dialogService;
    private readonly ConfiguracaoService _configService;

    private string _temaCor;

    public string TemaCor
    {
        get => _temaCor;
        set
        {
            if (SetProperty(ref _temaCor, value))
            {
                OnPropertyChanged(nameof(IsAzul));
                OnPropertyChanged(nameof(IsVerde));
                OnPropertyChanged(nameof(IsRoxo));
                OnPropertyChanged(nameof(IsLaranja));
                OnPropertyChanged(nameof(IsRosa));
                OnPropertyChanged(nameof(IsVermelho));
                OnPropertyChanged(nameof(IsCinza));
                _configService.TemaCor = value;
                AplicarTemaAtual();
            }
        }
    }

    public bool IsAzul => _temaCor == "Azul";
    public bool IsVerde => _temaCor == "Verde";
    public bool IsRoxo => _temaCor == "Roxo";
    public bool IsLaranja => _temaCor == "Laranja";
    public bool IsRosa => _temaCor == "Rosa";
    public bool IsVermelho => _temaCor == "Vermelho";
    public bool IsCinza => _temaCor == "Cinza";

    public ICommand LoadedCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand RestaurarBackupCommand { get; }
    public ICommand AbrirPastaBackupCommand { get; }
    public ICommand SelecionarCorCommand { get; }
    public ICommand GerenciarUsuariosCommand { get; }
    public ICommand GerenciarCamposCommand { get; }

    public ConfiguracoesViewModel(
        IBackupService backupService,
        IDialogService dialogService,
        ConfiguracaoService configService)
    {
        _backupService = backupService;
        _dialogService = dialogService;
        _configService = configService;

        _temaCor = configService.TemaCor;

        LoadedCommand = new RelayCommand(_ => _ = LoadAsync());
        BackupCommand = new RelayCommand(_ => _ = CriarBackupAsync());
        RestaurarBackupCommand = new RelayCommand(_ => _ = RestaurarBackupAsync());
        AbrirPastaBackupCommand = new RelayCommand(_ => _ = AbrirPastaBackupAsync());
        SelecionarCorCommand = new RelayCommand(param => TemaCor = param?.ToString() ?? "Azul");
        GerenciarUsuariosCommand = new RelayCommand(_ => AbrirUsuarios());
        GerenciarCamposCommand = new RelayCommand(_ => AbrirCamposPersonalizados());
    }

    private void AbrirUsuarios()
    {
        var dialog = new Views.UsuariosView();
        dialog.ShowDialog();
    }

    private void AbrirCamposPersonalizados()
    {
        var dialog = new Views.CamposPersonalizadosView();
        dialog.ShowDialog();
    }

    private void AplicarTemaAtual()
    {
        if (_configService.ModoEscuro) return;
        var dicts = WpfApp.Current.Resources.MergedDictionaries;
        var temaAtual = dicts.FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);
        if (temaAtual != null) dicts.Remove(temaAtual);
        var arquivo = _temaCor switch
        {
            "Verde" => "LightTheme_Verde",
            "Roxo" => "LightTheme_Roxo",
            "Laranja" => "LightTheme_Laranja",
            "Rosa" => "LightTheme_Rosa",
            "Vermelho" => "LightTheme_Vermelho",
            "Cinza" => "LightTheme_Cinza",
            _ => "LightTheme"
        };
        dicts.Add(new ResourceDictionary { Source = new Uri($"pack://application:,,,/Themes/{arquivo}.xaml") });
    }

    private async Task LoadAsync()
    {
        StatusMessage = "Configurações carregadas";
    }

    private async Task CriarBackupAsync()
    {
        var nomeArquivo = $"sistemarh_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
        var caminho = await _dialogService.SalvarArquivoAsync("Backup SistemaRH|*.zip", nomeArquivo);
        if (string.IsNullOrEmpty(caminho)) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Criando backup...";

            var resultado = await _backupService.CreateBackupAsync(caminho);
            if (resultado)
            {
                await _dialogService.ShowInfoAsync("Sucesso", "Backup criado com sucesso!");
                StatusMessage = "Backup criado com sucesso";
            }
            else
            {
                await _dialogService.ShowErrorAsync("Erro", "Não foi possível criar o backup");
                StatusMessage = "Erro ao criar backup";
            }
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

    private async Task RestaurarBackupAsync()
    {
        var confirmado = await _dialogService.ShowConfirmAsync(
            "Restaurar Backup",
            "Isso substituirá TODOS os dados atuais pelo conteúdo do backup selecionado.\n\n" +
            "Uma cópia de segurança do banco atual será criada automaticamente antes da restauração.\n\n" +
            "Deseja continuar?");

        if (!confirmado) return;

        var arquivo = await _dialogService.AbrirArquivoAsync("Backup SistemaRH|*.zip;*.db|Arquivo ZIP|*.zip|Banco de dados|*.db|Todos os arquivos|*.*");
        if (string.IsNullOrEmpty(arquivo)) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Restaurando backup...";

            var ok = await _backupService.RestoreBackupAsync(arquivo);
            if (!ok)
            {
                await _dialogService.ShowErrorAsync("Erro", "Não foi possível restaurar o backup. Verifique se o arquivo é válido.");
                StatusMessage = "Erro ao restaurar backup";
                return;
            }

            await _dialogService.ShowInfoAsync(
                "Backup Restaurado",
                "O backup foi restaurado com sucesso!\n\n" +
                "A aplicação será reiniciada agora para aplicar as alterações.");

            var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (exe != null) System.Diagnostics.Process.Start(exe);
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
            StatusMessage = $"Erro: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AbrirPastaBackupAsync()
    {
        try
        {
            var pastaBackup = await _backupService.GetBackupDirectoryAsync();
            Directory.CreateDirectory(pastaBackup);

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pastaBackup,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);

            StatusMessage = "Pasta de backup aberta";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }
}
