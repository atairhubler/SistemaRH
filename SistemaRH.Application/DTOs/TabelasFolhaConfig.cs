namespace SistemaRH.Application.DTOs;

public class TabelasFolhaConfig
{
    public List<FaixaINSSConfig> FaixasINSS { get; set; } = new();
    public List<FaixaIRRFConfig> FaixasIRRF { get; set; } = new();
    public decimal DeducaoPorDependente { get; set; } = 189.59m;

    public static TabelasFolhaConfig Padrao2024() => new()
    {
        DeducaoPorDependente = 189.59m,
        FaixasINSS =
        [
            new() { LimiteSuperior = 1412.00m,  Aliquota = 0.075m },
            new() { LimiteSuperior = 2666.68m,  Aliquota = 0.09m  },
            new() { LimiteSuperior = 4000.01m,  Aliquota = 0.12m  },
            new() { LimiteSuperior = 7786.02m,  Aliquota = 0.14m  }
        ],
        FaixasIRRF =
        [
            new() { LimiteSuperior = 2259.20m,   Aliquota = 0m,     Deducao = 0m      },
            new() { LimiteSuperior = 2826.65m,   Aliquota = 0.075m, Deducao = 169.44m },
            new() { LimiteSuperior = 3751.05m,   Aliquota = 0.15m,  Deducao = 381.44m },
            new() { LimiteSuperior = 4664.68m,   Aliquota = 0.225m, Deducao = 662.77m },
            new() { LimiteSuperior = 999999.99m, Aliquota = 0.275m, Deducao = 896.00m }
        ]
    };
}

public class FaixaINSSConfig
{
    public decimal LimiteSuperior { get; set; }
    public decimal Aliquota { get; set; }
}

public class FaixaIRRFConfig
{
    public decimal LimiteSuperior { get; set; }
    public decimal Aliquota { get; set; }
    public decimal Deducao { get; set; }
}
