using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Desktop.ViewModels;

public class FeriasViewModel : BaseViewModel
{
    private readonly IFeriasService _feriasService;
    private readonly IFuncionarioService _funcionarioService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<FuncionarioDto> _funcionarios = new();
    private FuncionarioDto? _selectedFuncionario;
    private bool _elegivel;
    private bool _ehClt;

    private ObservableCollection<PeriodoAquisitivoDto> _periodos = new();
    private PeriodoAquisitivoDto? _selectedPeriodo;
    private ObservableCollection<PeriodoFeriasDto> _usos = new();

    private DateTime _novoUsoInicio = DateTime.Now;
    private DateTime _novoUsoFim = DateTime.Now.AddDays(13);
    private TipoUsoFerias _novoUsoTipo = TipoUsoFerias.Gozo;
    private bool _novoUsoRemunerada = true;
    private int _numeroDependentes;

    private CalculoLiquidoFeriasDto? _resultadoCalculo;

    private int _anoCalendario = DateTime.Now.Year;
    private int _mesCalendario = DateTime.Now.Month;
    private ObservableCollection<DiaCalendarioFerias> _diasCalendario = new();

    public ObservableCollection<FuncionarioDto> Funcionarios { get => _funcionarios; set => SetProperty(ref _funcionarios, value); }

    public FuncionarioDto? SelectedFuncionario
    {
        get => _selectedFuncionario;
        set
        {
            SetProperty(ref _selectedFuncionario, value);
            if (value != null)
                _ = CarregarPeriodosAsync();
        }
    }

    public bool Elegivel { get => _elegivel; set => SetProperty(ref _elegivel, value); }
    public bool EhClt { get => _ehClt; set => SetProperty(ref _ehClt, value); }

    public ObservableCollection<PeriodoAquisitivoDto> Periodos { get => _periodos; set => SetProperty(ref _periodos, value); }

    public PeriodoAquisitivoDto? SelectedPeriodo
    {
        get => _selectedPeriodo;
        set
        {
            SetProperty(ref _selectedPeriodo, value);
            OnPropertyChanged(nameof(TemPeriodoSelecionado));
            ResultadoCalculo = null;
            if (value != null)
                _ = CarregarUsosAsync();
            else
                Usos = new ObservableCollection<PeriodoFeriasDto>();
        }
    }

    public bool TemPeriodoSelecionado => SelectedPeriodo != null;

    public ObservableCollection<PeriodoFeriasDto> Usos { get => _usos; set => SetProperty(ref _usos, value); }

    public DateTime NovoUsoInicio { get => _novoUsoInicio; set => SetProperty(ref _novoUsoInicio, value); }
    public DateTime NovoUsoFim { get => _novoUsoFim; set => SetProperty(ref _novoUsoFim, value); }
    public TipoUsoFerias NovoUsoTipo { get => _novoUsoTipo; set => SetProperty(ref _novoUsoTipo, value); }
    public bool NovoUsoRemunerada { get => _novoUsoRemunerada; set => SetProperty(ref _novoUsoRemunerada, value); }
    public int NumeroDependentes { get => _numeroDependentes; set => SetProperty(ref _numeroDependentes, value); }

    public List<OpcaoTipoUsoFerias> OpcoesTipoUso { get; } = new()
    {
        new OpcaoTipoUsoFerias { Valor = TipoUsoFerias.Gozo, Rotulo = "Férias Gozadas" },
        new OpcaoTipoUsoFerias { Valor = TipoUsoFerias.AbonoPecuniario, Rotulo = "Abono Pecuniário" }
    };

    public CalculoLiquidoFeriasDto? ResultadoCalculo { get => _resultadoCalculo; set => SetProperty(ref _resultadoCalculo, value); }

    public int AnoCalendario { get => _anoCalendario; set => SetProperty(ref _anoCalendario, value); }
    public int MesCalendario { get => _mesCalendario; set => SetProperty(ref _mesCalendario, value); }

    public string TituloCalendario =>
        new DateTime(AnoCalendario, MesCalendario, 1).ToString("MMMM 'de' yyyy", new CultureInfo("pt-BR"));

    public ObservableCollection<DiaCalendarioFerias> DiasCalendario { get => _diasCalendario; set => SetProperty(ref _diasCalendario, value); }

    public ICommand AdicionarUsoCommand { get; }
    public ICommand RemoverUsoCommand { get; }
    public ICommand MesAnteriorCommand { get; }
    public ICommand MesProximoCommand { get; }

    public FeriasViewModel(
        IFeriasService feriasService,
        IFuncionarioService funcionarioService,
        IDialogService dialogService)
    {
        _feriasService = feriasService;
        _funcionarioService = funcionarioService;
        _dialogService = dialogService;

        AdicionarUsoCommand = new RelayCommand(_ => _ = AdicionarUsoAsync(), _ => SelectedPeriodo != null);
        RemoverUsoCommand = new RelayCommand(param => _ = RemoverUsoAsync(param as PeriodoFeriasDto));
        MesAnteriorCommand = new RelayCommand(_ => _ = MudarMesAsync(-1));
        MesProximoCommand = new RelayCommand(_ => _ = MudarMesAsync(1));
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando funcionários...";

            var todos = await _funcionarioService.GetAllAsync();
            Funcionarios = new ObservableCollection<FuncionarioDto>(todos.Where(f => f.IsAtivo));

            await CarregarCalendarioAsync();

            StatusMessage = $"Carregados {Funcionarios.Count} funcionários";
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

    private async Task CarregarPeriodosAsync()
    {
        if (SelectedFuncionario == null) return;

        try
        {
            IsLoading = true;
            SelectedPeriodo = null;
            ResultadoCalculo = null;
            EhClt = SelectedFuncionario.Tipo == TipoFuncionario.CLT;

            Elegivel = await _feriasService.EhElegivelAsync(SelectedFuncionario.Id);
            if (!Elegivel)
            {
                Periodos = new ObservableCollection<PeriodoAquisitivoDto>();
                StatusMessage = $"{SelectedFuncionario.Nome} não possui controle de período aquisitivo (estagiário ou PJ sem direito a férias).";
                return;
            }

            await _feriasService.GarantirPeriodosAsync(SelectedFuncionario.Id);
            var periodos = await _feriasService.GetPeriodosAquisitivosAsync(SelectedFuncionario.Id);
            Periodos = new ObservableCollection<PeriodoAquisitivoDto>(periodos);
            StatusMessage = $"Períodos carregados para {SelectedFuncionario.Nome}";
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

    private async Task CarregarUsosAsync()
    {
        if (SelectedPeriodo == null) return;

        try
        {
            var usos = await _feriasService.GetUsosDoPeriodoAsync(SelectedPeriodo.Id);
            Usos = new ObservableCollection<PeriodoFeriasDto>(usos);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }

    private async Task AdicionarUsoAsync()
    {
        if (SelectedPeriodo == null || SelectedFuncionario == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Registrando uso de férias...";

            var uso = await _feriasService.AdicionarUsoAsync(SelectedPeriodo.Id, NovoUsoInicio, NovoUsoFim, NovoUsoTipo, NovoUsoRemunerada);

            if (EhClt && NovoUsoTipo == TipoUsoFerias.Gozo)
            {
                ResultadoCalculo = await _feriasService.CalcularLiquidoFeriasAsync(SelectedFuncionario.Id, uso.Dias, 0, NumeroDependentes);
                await _dialogService.ShowInfoAsync("Cálculo do período",
                    $"Bruto de férias: {ResultadoCalculo.BrutoFerias:C2}\n" +
                    $"Desconto INSS: {ResultadoCalculo.DescontoINSS:C2}\n" +
                    $"Desconto IRRF: {ResultadoCalculo.DescontoIRRF:C2}\n" +
                    $"Líquido a receber: {ResultadoCalculo.Liquido:C2}");
            }
            else if (EhClt && NovoUsoTipo == TipoUsoFerias.AbonoPecuniario)
            {
                ResultadoCalculo = await _feriasService.CalcularLiquidoFeriasAsync(SelectedFuncionario.Id, 0, uso.Dias, NumeroDependentes);
                await _dialogService.ShowInfoAsync("Cálculo do abono",
                    $"Valor do abono (isento de INSS/IRRF): {ResultadoCalculo.BrutoAbono:C2}");
            }
            else
            {
                await _dialogService.ShowInfoAsync("Sucesso", "Uso de férias registrado.");
            }

            await CarregarUsosAsync();
            await CarregarPeriodosAsync();
            await CarregarCalendarioAsync();
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

    private async Task RemoverUsoAsync(PeriodoFeriasDto? uso)
    {
        if (uso == null) return;

        var confirmar = await _dialogService.ShowConfirmAsync("Remover uso", "Tem certeza que deseja remover este registro?");
        if (!confirmar) return;

        try
        {
            await _feriasService.RemoverUsoAsync(uso.Id);
            await CarregarUsosAsync();
            await CarregarPeriodosAsync();
            await CarregarCalendarioAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }

    private async Task MudarMesAsync(int delta)
    {
        var novaData = new DateTime(AnoCalendario, MesCalendario, 1).AddMonths(delta);
        AnoCalendario = novaData.Year;
        MesCalendario = novaData.Month;
        OnPropertyChanged(nameof(TituloCalendario));
        await CarregarCalendarioAsync();
    }

    private async Task CarregarCalendarioAsync()
    {
        try
        {
            var funcionariosDeFerias = await _feriasService.GetFuncionariosDeFeriasNoMesAsync(AnoCalendario, MesCalendario);

            var primeiroDiaMes = new DateTime(AnoCalendario, MesCalendario, 1);
            var diasNoMes = DateTime.DaysInMonth(AnoCalendario, MesCalendario);
            var offsetInicial = (int)primeiroDiaMes.DayOfWeek;

            var dias = new ObservableCollection<DiaCalendarioFerias>();

            for (var i = 0; i < offsetInicial; i++)
                dias.Add(new DiaCalendarioFerias { ForaDoMes = true });

            for (var dia = 1; dia <= diasNoMes; dia++)
            {
                var data = new DateTime(AnoCalendario, MesCalendario, dia);
                var nomes = funcionariosDeFerias
                    .Where(f => data >= f.DataInicio.Date && data <= f.DataFim.Date)
                    .Select(f => f.FuncionarioNome)
                    .Distinct()
                    .ToList();

                dias.Add(new DiaCalendarioFerias
                {
                    Dia = dia,
                    TemFerias = nomes.Count > 0,
                    Funcionarios = nomes.Count <= 2 ? string.Join(", ", nomes) : $"{nomes[0]} +{nomes.Count - 1}"
                });
            }

            DiasCalendario = dias;
            OnPropertyChanged(nameof(TituloCalendario));
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }
}
