using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public abstract class Funcionario
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public TipoFuncionario Tipo { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
    public string Endereco { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    public string Cep { get; set; }
    public StatusFuncionario Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    // Navegação
    public Empresa Empresa { get; set; }
    public Ferias Ferias { get; set; }
    public ICollection<Atestado> Atestados { get; set; } = new List<Atestado>();
}
