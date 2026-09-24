using AutoMapper;
using Ledger.Data.ContaDtos;
using Ledger.Models;

namespace Ledger.Data.Profiles;

public class ContaProfile : Profile
{
    public ContaProfile() 
    {
        CreateMap<CreateContaDto, Conta>();
        CreateMap<Conta, ReadContaDto>();
        CreateMap<UpdateContaDto, Conta>();
    }
}
