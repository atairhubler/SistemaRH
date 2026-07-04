using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.DTOs;

public abstract class FuncionarioDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public TipoFuncionario Tipo { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string EmailEmpresa { get; set; }
    public string Contratante { get; set; }
    public string Telefone { get; set; }
    public string Endereco { get; set; }
    public string Complemento { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    public string Cep { get; set; }
    public StatusFuncionario Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public string Genero { get; set; }
    public string Ramal { get; set; }
    public string DadosBancarios { get; set; }
    public bool Comissionado { get; set; }
    public string Observacoes { get; set; }
    public decimal AjudaDeCusto { get; set; }
    public string CamposPersonalizadosJson { get; set; } = "{}";
    public byte[]? Foto { get; set; }

    public bool IsAtivo => Status == StatusFuncionario.Ativo;
}
