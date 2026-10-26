using AutoMapper;
using Ledger.Application.Dtos.Contas;
using Ledger.Domain.Entities;
using Ledger.Domain.ValueObjects;

namespace Ledger.Application.Profiles;

public class ContaProfile : Profile
{
    public ContaProfile() 
    {
       CreateMap<string, Cpf>().ConvertUsing(cpf => new Cpf(cpf));
       CreateMap<Cpf, string>().ConvertUsing(cpf => cpf.Value);
        
       CreateMap<CreateContaDto, Conta>();
       CreateMap<Conta, ReadContaDto>();
       CreateMap<UpdateContaDto, Conta>();
    }
}
