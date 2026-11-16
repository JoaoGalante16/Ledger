using AutoMapper;
using Ledger.Application.Dtos.Contas;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;

namespace Ledger.Application.UseCases.Contas;

public class CriarConta
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CriarConta(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReadContaDto> ExecutarAsync(CreateContaDto dto, string idUsuario)
    {
        var conta = new Conta(dto.Nome, new Cpf(dto.Cpf), idUsuario);
        await _unitOfWork.ContaRepository.AdicionarAsync(conta);
        await _unitOfWork.CommitAsync();
        return _mapper.Map<ReadContaDto>(conta);
    }
}
