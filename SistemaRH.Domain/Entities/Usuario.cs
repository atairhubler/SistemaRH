namespace SistemaRH.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NomeUsuario { get; set; }
    public string SenhaHash { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}
