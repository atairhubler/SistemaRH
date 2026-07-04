using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class FuncionarioPerfilViewModel : BaseViewModel
{
    private readonly IEmpresaService _empresaService;
    private readonly IHistoricoSalarialService _historicoService;
    private readonly IFeriasService _feriasService;
    private readonly IAtestadoService _atestadoService;
    private readonly IDialogService _dialogService;

    private string _nome = "";
    private string _tipoTexto = "";
    private string _statusTexto = "";
    private ObservableCollection<CampoPerfilItem> _dadosCadastrais = new();
    private ObservableCollection<SalarioHistoricoDto> _historicoSalarial = new();
    private bool _temHistoricoSalarial;
    private string _rotuloValorHistorico = "Valor";
    private ObservableCollection<PeriodoFeriasComUsos> _periodosFerias = new();
    private bool _temFerias;
    private bool _elegivelFerias;
    private ObservableCollection<AtestadoDto> _atestados = new();
    private bool _temAtestados;
    private byte[]? _foto;

    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string TipoTexto { get => _tipoTexto; set => SetProperty(ref _tipoTexto, value); }
    public string StatusTexto { get => _statusTexto; set => SetProperty(ref _statusTexto, value); }
    public byte[]? Foto { get => _foto; set => SetProperty(ref _foto, value); }

    public ObservableCollection<CampoPerfilItem> DadosCadastrais
    {
        get => _dadosCadastrais;
        set => SetProperty(ref _dadosCadastrais, value);
    }

    public ObservableCollection<SalarioHistoricoDto> HistoricoSalarial
    {
        get => _historicoSalarial;
        set => SetProperty(ref _historicoSalarial, value);
    }

    public bool TemHistoricoSalarial { get => _temHistoricoSalarial; set => SetProperty(ref _temHistoricoSalarial, value); }
    public string RotuloValorHistorico { get => _rotuloValorHistorico; set => SetProperty(ref _rotuloValorHistorico, value); }

    public ObservableCollection<PeriodoFeriasComUsos> PeriodosFerias
    {
        get => _periodosFerias;
        set => SetProperty(ref _periodosFerias, value);
    }

    public bool TemFerias { get => _temFerias; set => SetProperty(ref _temFerias, value); }
    public bool ElegivelFerias { get => _elegivelFerias; set => SetProperty(ref _elegivelFerias, value); }

    public ObservableCollection<AtestadoDto> Atestados
    {
        get => _atestados;
        set => SetProperty(ref _atestados, value);
    }

    public bool TemAtestados { get => _temAtestados; set => SetProperty(ref _temAtestados, value); }

    public ICommand FecharCommand { get; }
    public ICommand BaixarAtestadoCommand { get; }
    public Action? FecharJanela { get; set; }

    public FuncionarioPerfilViewModel(
        IEmpresaService empresaService,
        IHistoricoSalarialService historicoService,
        IFeriasService feriasService,
        IAtestadoService atestadoService,
        IDialogService dialogService)
    {
        _empresaService = empresaService;
        _historicoService = historicoService;
        _feriasService = feriasService;
        _atestadoService = atestadoService;
        _dialogService = dialogService;

        FecharCommand = new RelayCommand(_ => FecharJanela?.Invoke());
        BaixarAtestadoCommand = new RelayCommand(param => _ = BaixarAtestadoAsync(param as AtestadoDto));
    }

    private async Task BaixarAtestadoAsync(AtestadoDto? atestado)
    {
        if (atestado == null) return;

        try
        {
            var (arquivo, nomeArquivo) = await _atestadoService.ExportarArquivoAsync(atestado.Id);
            var extensao = Path.GetExtension(nomeArquivo);
            var filtro = string.IsNullOrEmpty(extensao) ? "Todos os arquivos|*.*" : $"Arquivo|*{extensao}";
            var caminho = await _dialogService.SalvarArquivoAsync(filtro, Path.GetFileNameWithoutExtension(nomeArquivo));
            if (string.IsNullOrEmpty(caminho)) return;

            await File.WriteAllBytesAsync(caminho, arquivo);
            await _dialogService.ShowInfoAsync("Sucesso", "Arquivo salvo com sucesso!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }

    public void Preparar(FuncionarioDto dto)
    {
        _ = CarregarAsync(dto);
    }

    private async Task CarregarAsync(FuncionarioDto dto)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando perfil...";

            Nome = dto.Nome;
            StatusTexto = dto.IsAtivo ? "Ativo" : "Inativo";
            Foto = dto.Foto;

            string nomeEmpresa;
            try
            {
                var empresa = await _empresaService.GetByIdAsync(dto.EmpresaId);
                nomeEmpresa = empresa?.RazaoSocial ?? "—";
            }
            catch
            {
                nomeEmpresa = "—";
            }

            var campos = new List<CampoPerfilItem>
            {
                new() { Rotulo = "Empresa", Valor = nomeEmpresa },
                new() { Rotulo = "E-mail", Valor = dto.Email },
                new() { Rotulo = "E-mail Corporativo", Valor = dto.EmailEmpresa },
                new() { Rotulo = "Telefone", Valor = dto.Telefone }
            };

            switch (dto)
            {
                case FuncionarioCLTDto clt:
                    TipoTexto = "CLT";
                    RotuloValorHistorico = "Salário";
                    campos.Add(new() { Rotulo = "Cargo", Valor = clt.Cargo });
                    campos.Add(new() { Rotulo = "Departamento", Valor = clt.Departamento });
                    campos.Add(new() { Rotulo = "CPF", Valor = clt.Cpf });
                    campos.Add(new() { Rotulo = "RG", Valor = clt.Rg });
                    campos.Add(new() { Rotulo = "CTPS", Valor = clt.Ctps });
                    campos.Add(new() { Rotulo = "PIS/PASEP", Valor = clt.PisPassep });
                    campos.Add(new() { Rotulo = "Data de Nascimento", Valor = clt.DataNascimento.ToString("dd/MM/yyyy") });
                    campos.Add(new() { Rotulo = "Data de Admissão", Valor = clt.DataAdmissao.ToString("dd/MM/yyyy") });
                    campos.Add(new() { Rotulo = "Data de Demissão", Valor = clt.DataDemissao?.ToString("dd/MM/yyyy") ?? "—" });
                    campos.Add(new() { Rotulo = "Salário Bruto", Valor = clt.SalarioBruto.ToString("C2") });
                    campos.Add(new() { Rotulo = "Total Atual", Valor = clt.TotalAtual.ToString("C2") });
                    break;

                case FuncionarioPJDto pj:
                    TipoTexto = "PJ";
                    RotuloValorHistorico = "Valor de Serviço";
                    campos.Add(new() { Rotulo = "Razão Social", Valor = pj.RazaoSocial });
                    campos.Add(new() { Rotulo = "Departamento", Valor = pj.Departamento });
                    campos.Add(new() { Rotulo = "CNPJ", Valor = pj.Cnpj });
                    campos.Add(new() { Rotulo = "Data de Nascimento", Valor = pj.DataNascimento.ToString("dd/MM/yyyy") });
                    campos.Add(new() { Rotulo = "Início de Contrato", Valor = pj.DataInicio?.ToString("dd/MM/yyyy") ?? "—" });
                    campos.Add(new() { Rotulo = "Fim de Contrato", Valor = pj.DataFim?.ToString("dd/MM/yyyy") ?? "—" });
                    campos.Add(new() { Rotulo = "Validade do Contrato", Valor = pj.ValidadeContrato });
                    campos.Add(new() { Rotulo = "Valor de Serviço", Valor = pj.ValorServico.ToString("C2") });
                    campos.Add(new() { Rotulo = "Valor Contratado", Valor = pj.ValorContratado.ToString("C2") });
                    break;

                case FuncionarioEstagiarioDto estagiario:
                    TipoTexto = "Estagiário";
                    RotuloValorHistorico = "Bolsa";
                    campos.Add(new() { Rotulo = "Cargo", Valor = estagiario.Cargo });
                    campos.Add(new() { Rotulo = "Departamento", Valor = estagiario.Departamento });
                    campos.Add(new() { Rotulo = "CPF", Valor = estagiario.Cpf });
                    campos.Add(new() { Rotulo = "RG", Valor = estagiario.Rg });
                    campos.Add(new() { Rotulo = "Data de Nascimento", Valor = estagiario.DataNascimento.ToString("dd/MM/yyyy") });
                    campos.Add(new() { Rotulo = "Data de Admissão", Valor = estagiario.DataAdmissao.ToString("dd/MM/yyyy") });
                    campos.Add(new() { Rotulo = "Data de Demissão", Valor = estagiario.DataDemissao?.ToString("dd/MM/yyyy") ?? "—" });
                    campos.Add(new() { Rotulo = "Bolsa", Valor = estagiario.Bolsa.ToString("C2") });
                    campos.Add(new() { Rotulo = "Total Atual", Valor = estagiario.TotalAtual.ToString("C2") });
                    break;
            }

            DadosCadastrais = new ObservableCollection<CampoPerfilItem>(campos);

            var historico = await _historicoService.GetByFuncionarioAsync(dto.Id);
            HistoricoSalarial = new ObservableCollection<SalarioHistoricoDto>(historico.OrderByDescending(h => h.DataVigencia));
            TemHistoricoSalarial = HistoricoSalarial.Count > 0;

            ElegivelFerias = await _feriasService.EhElegivelAsync(dto.Id);
            if (ElegivelFerias)
            {
                var periodos = await _feriasService.GetPeriodosAquisitivosAsync(dto.Id);
                var lista = new List<PeriodoFeriasComUsos>();
                foreach (var periodo in periodos.OrderBy(p => p.NumeroPeriodo))
                {
                    var usos = await _feriasService.GetUsosDoPeriodoAsync(periodo.Id);
                    lista.Add(new PeriodoFeriasComUsos
                    {
                        Periodo = periodo,
                        Usos = new ObservableCollection<PeriodoFeriasDto>(usos)
                    });
                }
                PeriodosFerias = new ObservableCollection<PeriodoFeriasComUsos>(lista);
                TemFerias = PeriodosFerias.Count > 0;
            }
            else
            {
                PeriodosFerias = new ObservableCollection<PeriodoFeriasComUsos>();
                TemFerias = false;
            }

            var atestados = await _atestadoService.GetByFuncionarioAsync(dto.Id);
            Atestados = new ObservableCollection<AtestadoDto>(atestados);
            TemAtestados = Atestados.Count > 0;

            StatusMessage = "Perfil carregado";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar perfil: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
