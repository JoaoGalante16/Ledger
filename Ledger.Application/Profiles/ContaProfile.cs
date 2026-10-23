using AutoMapper;
using Ledger.Application.Dtos.Contas;
using Ledger.Domain.Entities;

namespace Ledger.Application.Profiles;

public class ContaProfile : Profile
{
    public ContaProfile() 
    {
        CreateMap<CreateContaDto, Conta>();
        CreateMap<Conta, ReadContaDto>();
        CreateMap<UpdateContaDto, Conta>();
    }
}
