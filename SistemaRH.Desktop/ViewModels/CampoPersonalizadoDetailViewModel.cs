using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Desktop.ViewModels;

public class CampoPersonalizadoDetailViewModel : BaseViewModel
{
    private readonly ICampoPersonalizadoService _campoService;
    private readonly IDialogService _dialogService;

    private int _id;
    private bool _isEdit;

    private string _rotulo = "";
    private string _tipoTexto = nameof(TipoCampoPersonalizado.Texto);
    private string _opcoes = "";
    private bool _obrigatorio;
    private int _ordem;
    private string _aplicavelATexto = "Todos";
    private bool _ativo = true;

    public string Titulo => _isEdit ? "Editar Campo Personalizado" : "Novo Campo Personalizado";

    public string Rotulo { get => _rotulo; set => SetProperty(ref _rotulo, value); }

    public string TipoTexto
    {
        get => _tipoTexto;
        set
        {
            if (SetProperty(ref _tipoTexto, value))
                OnPropertyChanged(nameof(MostrarOpcoes));
        }
    }

    public bool MostrarOpcoes => _tipoTexto == nameof(TipoCampoPersonalizado.Lista);

    public string Opcoes { get => _opcoes; set => SetProperty(ref _opcoes, value); }
    public bool Obrigatorio { get => _obrigatorio; set => SetProperty(ref _obrigatorio, value); }
    public int Ordem { get => _ordem; set => SetProperty(ref _ordem, value); }
    public string AplicavelATexto { get => _aplicavelATexto; set => SetProperty(ref _aplicavelATexto, value); }
    public bool Ativo { get => _ativo; set => SetProperty(ref _ativo, value); }

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }

    public CampoPersonalizadoDetailViewModel(ICampoPersonalizadoService campoService, IDialogService dialogService)
    {
        _campoService = campoService;
        _dialogService = dialogService;

        SalvarCommand = new RelayCommand(_ => _ = SalvarAsync());
        CancelarCommand = new RelayCommand(_ => Cancelar());
    }

    public void PrepararNovo()
    {
        _id = 0;
        _isEdit = false;
        Rotulo = "";
        TipoTexto = nameof(TipoCampoPersonalizado.Texto);
        Opcoes = "";
        Obrigatorio = false;
        Ordem = 0;
        AplicavelATexto = "Todos";
        Ativo = true;
        OnPropertyChanged(nameof(Titulo));
    }

    public void PrepararEdicao(CampoPersonalizadoDto dto)
    {
        _id = dto.Id;
        _isEdit = true;
        Rotulo = dto.Rotulo;
        TipoTexto = dto.Tipo.ToString();
        Opcoes = dto.Opcoes;
        Obrigatorio = dto.Obrigatorio;
        Ordem = dto.Ordem;
        AplicavelATexto = dto.AplicavelA?.ToString() ?? "Todos";
        Ativo = dto.Ativo;
        OnPropertyChanged(nameof(Titulo));
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(Rotulo)) return false;
        if (MostrarOpcoes && string.IsNullOrWhiteSpace(Opcoes)) return false;
        return true;
    }

    private async Task SalvarAsync()
    {
        if (!ValidarCampos())
        {
            await _dialogService.ShowErrorAsync("Validação",
                "Informe o rótulo do campo. Para campos do tipo Lista, informe as opções (separadas por vírgula).");
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Salvando campo personalizado...";

            var dto = new CampoPersonalizadoDto
            {
                Rotulo = Rotulo,
                Tipo = Enum.Parse<TipoCampoPersonalizado>(TipoTexto),
                Opcoes = MostrarOpcoes ? Opcoes : "",
                Obrigatorio = Obrigatorio,
                Ordem = Ordem,
                AplicavelA = AplicavelATexto == "Todos" ? null : Enum.Parse<TipoFuncionario>(AplicavelATexto),
                Ativo = Ativo
            };

            if (_isEdit)
                await _campoService.UpdateAsync(_id, dto);
            else
                await _campoService.AddAsync(dto);

            var acao = _isEdit ? "atualizado" : "cadastrado";
            await _dialogService.ShowInfoAsync("Sucesso", $"Campo '{Rotulo}' {acao} com sucesso!");
            FecharJanela?.Invoke();
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

    private void Cancelar() => FecharJanela?.Invoke();

    public Action? FecharJanela { get; set; }
}
