using System.Globalization;
using SistemaRH.Application.DTOs;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Desktop.ViewModels;

public class CampoPersonalizadoValorViewModel : BaseViewModel
{
    public CampoPersonalizadoDto Definicao { get; }

    public int CampoPersonalizadoId => Definicao.Id;
    public string Rotulo => Definicao.Rotulo;
    public string RotuloExibicao => Obrigatorio ? $"{Rotulo} *" : Rotulo;
    public bool Obrigatorio => Definicao.Obrigatorio;
    public string[] OpcoesLista => Definicao.OpcoesLista;

    public bool EhTexto => Definicao.Tipo == TipoCampoPersonalizado.Texto;
    public bool EhNumero => Definicao.Tipo == TipoCampoPersonalizado.Numero;
    public bool EhMonetario => Definicao.Tipo == TipoCampoPersonalizado.Monetario;
    public bool EhData => Definicao.Tipo == TipoCampoPersonalizado.Data;
    public bool EhCheckbox => Definicao.Tipo == TipoCampoPersonalizado.Checkbox;
    public bool EhLista => Definicao.Tipo == TipoCampoPersonalizado.Lista;

    private string _valor;

    public string Valor { get => _valor; set => SetProperty(ref _valor, value); }

    public string ValorNumero
    {
        get => _valor;
        set => Valor = new string((value ?? "").Where(char.IsDigit).ToArray());
    }

    public decimal ValorDecimal
    {
        get => decimal.TryParse(_valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
        set => Valor = value.ToString(CultureInfo.InvariantCulture);
    }

    public DateTime? ValorData
    {
        get => DateTime.TryParse(_valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
        set => Valor = value?.ToString("O", CultureInfo.InvariantCulture) ?? "";
    }

    public bool ValorBool
    {
        get => _valor == "true";
        set => Valor = value ? "true" : "false";
    }

    public bool PreenchidoValido => !Obrigatorio || !string.IsNullOrWhiteSpace(Valor);

    public CampoPersonalizadoValorViewModel(CampoPersonalizadoDto definicao, string? valorInicial)
    {
        Definicao = definicao;
        _valor = valorInicial ?? "";
    }
}
