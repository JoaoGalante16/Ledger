using AutoMapper;
using Ledger.Application.Dtos.Contas;
using Ledger.Domain.Entities;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Ledger.Application.UseCases.Contas;

public class CriarConta
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CriarConta> _logger;

    public CriarConta(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CriarConta> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReadContaDto> ExecutarAsync(CreateContaDto dto, string idUsuario)
    {
        var conta = new Conta(dto.Nome, new Cpf(dto.Cpf), idUsuario);
        await _unitOfWork.ContaRepository.AdicionarAsync(conta);
        await _unitOfWork.CommitAsync();
        _logger.LogInformation("Conta {NumeroConta} criada", conta.Numero);
        return _mapper.Map<ReadContaDto>(conta);
    }
}
