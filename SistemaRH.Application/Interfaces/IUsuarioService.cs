using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto> AddAsync(string nomeUsuario, string senha);
    Task<UsuarioDto> UpdateAsync(int id, string nomeUsuario, string? novaSenha, bool ativo);
    Task<UsuarioDto?> ValidarCredenciaisAsync(string nomeUsuario, string senha);
    Task<bool> ExisteAlgumUsuarioAsync();
}
