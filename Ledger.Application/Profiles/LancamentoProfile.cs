using AutoMapper;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Domain.Entities;

namespace Ledger.Application.Profiles;

public class LancamentoProfile : Profile
{
    public LancamentoProfile()
    {
        CreateMap<Lancamento, ReadLancamentoDto>();
        CreateMap<CreateLancamentoDto, Lancamento>();
        CreateMap<CreateLancamentoCorrecaoDto, Lancamento>();
    }
}
