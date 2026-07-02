using AutoMapper;
using SistemaRH.Application.DTOs;
using SistemaRH.Domain.Entities;

namespace SistemaRH.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Funcionário
        CreateMap<Funcionario, FuncionarioDto>().ReverseMap();
        CreateMap<FuncionarioCLT, FuncionarioCLTDto>().ReverseMap();
        CreateMap<FuncionarioPJ, FuncionarioPJDto>().ReverseMap();
        CreateMap<FuncionarioEstagiario, FuncionarioEstagiarioDto>().ReverseMap();

        // Férias
        CreateMap<Ferias, FeriasDto>().ReverseMap();

        // Contrato PJ
        CreateMap<ContratoPJ, ContratoPJDto>().ReverseMap();

        // RPA/NF PJ
        CreateMap<RpaNfPJ, RpaNfDto>().ReverseMap();

        // Relatório
        CreateMap<RelatorioComparativoDto, RelatorioComparativoDto>().ReverseMap();
    }
}
