using System.IO;
using System.Linq;
using System.Windows;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaRH.Application.Interfaces;
using SistemaRH.Application.Mappings;
using SistemaRH.Application.Services;
using SistemaRH.Data;
using SistemaRH.Desktop.Services;
using SistemaRH.Desktop.ViewModels;

namespace SistemaRH.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // Aplica restore pendente do banco (antes do EF Core abrir o arquivo)
                var appDataDir0 = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SistemaRH");
                var pendingFlag = Path.Combine(appDataDir0, "pending_restore.flag");
                var restoreFile = Path.Combine(appDataDir0, "sistemarh.db.restore");
                var dbPath0     = Path.Combine(appDataDir0, "sistemarh.db");
                if (File.Exists(pendingFlag) && File.Exists(restoreFile))
                {
                    File.Copy(restoreFile, dbPath0, overwrite: true);
                    File.Delete(restoreFile);
                    File.Delete(pendingFlag);
                }

                var services = new ServiceCollection();

                // DbContext
                services.AddDbContext<RhDbContext>(options =>
                {
                    var dbPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "SistemaRH", "sistemarh.db");

                    Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? "");
                    options.UseSqlite($"Data Source={dbPath}");
                });

                // AutoMapper
                var mapperConfig = new MapperConfiguration(cfg =>
                    cfg.AddProfile<MappingProfile>()
                );
                services.AddSingleton(mapperConfig.CreateMapper());

                // Config
                services.AddSingleton<ConfiguracaoService>();
                services.AddSingleton<TabelasFolhaService>();

                // Application Services
                services.AddScoped<IFolhaPagamentoService, FolhaPagamentoService>();
                services.AddScoped<IEmpresaService, EmpresaService>();
                services.AddScoped<IFuncionarioService, FuncionarioService>();
                services.AddScoped<IFeriasService, FeriasService>();
                services.AddScoped<IContratoPJService, ContratoPJService>();
                services.AddScoped<IRpaNfService, RpaNfService>();
                services.AddScoped<IRelatorioService, RelatorioService>();
                services.AddScoped<IBackupService, BackupService>();
                services.AddScoped<IGoogleDriveService, GoogleDriveService>();
                services.AddScoped<IExcelImportService, ExcelImportService>();
                services.AddScoped<IUsuarioService, UsuarioService>();
                services.AddScoped<IAuditoriaService, AuditoriaService>();
                services.AddScoped<IHistoricoSalarialService, HistoricoSalarialService>();

                // Desktop Services
                services.AddScoped<IDialogService, DialogService>();

                // ViewModels
                services.AddScoped<MainWindowViewModel>();
                services.AddScoped<EmpresasViewModel>();
                services.AddScoped<DashboardViewModel>();
                services.AddScoped<FuncionariosViewModel>();
                services.AddScoped<FeriasViewModel>();
                services.AddScoped<RpaNfViewModel>();
                services.AddScoped<RelatorioComparativoViewModel>();
                services.AddScoped<ConfiguracoesViewModel>();
                services.AddScoped<FuncionarioCLTDetailViewModel>();
                services.AddScoped<FuncionarioPJDetailViewModel>();
                services.AddScoped<FuncionarioEstagiarioDetailViewModel>();
                services.AddScoped<FolhaPagamentoViewModel>();
                services.AddTransient<TabelasCalculoViewModel>();
                services.AddScoped<LoginViewModel>();
                services.AddScoped<UsuariosViewModel>();
                services.AddScoped<UsuarioDetailViewModel>();
                services.AddScoped<AuditoriaViewModel>();
                services.AddScoped<HistoricoSalarialViewModel>();

                ServiceProvider = services.BuildServiceProvider();

                // Aplicar migrations
                using var scope = ServiceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<RhDbContext>();
                context.Database.Migrate();

                // Seed do usuário administrador inicial (só se não houver nenhum usuário cadastrado)
                var usuarioService = scope.ServiceProvider.GetRequiredService<IUsuarioService>();
                if (!await usuarioService.ExisteAlgumUsuarioAsync())
                    await usuarioService.AddAsync("admin", "admin123");

                // Backup automático a cada 30 dias
                var config = ServiceProvider.GetRequiredService<ConfiguracaoService>();
                var agora = DateTime.Now;
                var diasDesdeUltimo = config.UltimoBackupAutomatico.HasValue
                    ? (agora - config.UltimoBackupAutomatico.Value).TotalDays
                    : double.MaxValue;
                if (diasDesdeUltimo >= 30)
                {
                    var backupSvc = scope.ServiceProvider.GetRequiredService<IBackupService>();
                    var ok = await backupSvc.CreateAutoBackupAsync(agora);
                    if (ok) config.UltimoBackupAutomatico = agora;
                }

                // Restaurar tema salvo
                if (config.ModoEscuro || config.TemaCor != "Azul")
                {
                    var dicts = Current.Resources.MergedDictionaries;
                    var temaAtual = dicts.FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);
                    if (temaAtual != null) dicts.Remove(temaAtual);
                    string arquivo = config.ModoEscuro ? "DarkTheme" : config.TemaCor switch
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

                // Login: só continua para a janela principal se autenticar com sucesso
                var loginView = new Views.LoginView();
                var loginOk = loginView.ShowDialog() == true;
                if (!loginOk)
                {
                    Shutdown();
                    return;
                }

                var mainWindow = new MainWindow();
                mainWindow.Closed += (_, _) => Shutdown();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro durante inicialização:\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}",
                    "Erro de Inicialização",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Shutdown(1);
            }
        }
    }
}

