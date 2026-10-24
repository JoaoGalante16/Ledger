using AutoMapper;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Domain.Entities;
using Ledger.Domain.ValueObjects;

namespace Ledger.Application.Profiles;

public class LancamentoProfile : Profile
{
    public LancamentoProfile()
    {
        CreateMap<decimal, Valor>().ConvertUsing(valor => new Valor(valor));
        CreateMap<Valor, decimal>().ConvertUsing(valor => valor.Value);
        
        CreateMap<Lancamento, ReadLancamentoDto>();
        CreateMap<CreateLancamentoDto, Lancamento>();
        CreateMap<CreateLancamentoCorrecaoDto, Lancamento>();
    }
}
