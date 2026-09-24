using AutoMapper;
using Ledger.Data.LancamentoDtos;
using Ledger.Models;

namespace Ledger.Data.Profiles;

public class LancamentoProfile : Profile
{
    public LancamentoProfile()
    {
        CreateMap<Lancamento, ReadLancamentoDto>();
        CreateMap<CreateLancamentoDto, Lancamento>();
        CreateMap<CreateLancamentoCorrecaoDto, Lancamento>();
    }
}
