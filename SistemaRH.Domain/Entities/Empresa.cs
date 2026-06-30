namespace SistemaRH.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; }
    public string Cnpj { get; set; }
    public string InscricaoEstadual { get; set; }
    public string Endereco { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    public string Cep { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativa { get; set; }

    // Navegação
    public ICollection<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();
}
