using System.Security.Cryptography;

namespace SistemaRH.Application.Services;

public static class SenhaHasher
{
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        var combinado = new byte[TamanhoSalt + TamanhoHash];
        Buffer.BlockCopy(salt, 0, combinado, 0, TamanhoSalt);
        Buffer.BlockCopy(hash, 0, combinado, TamanhoSalt, TamanhoHash);
        return Convert.ToBase64String(combinado);
    }

    public static bool Verificar(string senha, string senhaHash)
    {
        var combinado = Convert.FromBase64String(senhaHash);
        if (combinado.Length != TamanhoSalt + TamanhoHash) return false;

        var salt = new byte[TamanhoSalt];
        var hashEsperado = new byte[TamanhoHash];
        Buffer.BlockCopy(combinado, 0, salt, 0, TamanhoSalt);
        Buffer.BlockCopy(combinado, TamanhoSalt, hashEsperado, 0, TamanhoHash);

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
