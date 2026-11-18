using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Application.Extensions;
using Ledger.Domain.Entities;
using Ledger.Domain.Enums;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Ledger.Application.UseCases.Lancamentos;

public class Sacar
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<Sacar> _logger;

    public Sacar(IUnitOfWork unitOfWork, IMapper mapper, ILogger<Sacar> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ReadLancamentoDto>>> ExecutarAsync(CreateLancamentoDepositoOuSaque dto, string idUsuario, bool eAdmin)
    {
        await _unitOfWork.IniciarTransacaoAsync();
        var numeroContaCaixa = await _unitOfWork.ContaRepository.BuscarNumeroContaCaixaAsync();
        var contas = await _unitOfWork.ContaRepository.BloquearEBuscarAsync(dto.NumeroConta, numeroContaCaixa);

        var contaCaixa = contas.Single(c => c.Numero == numeroContaCaixa);
        var contaCliente = contas.SingleOrDefault(c => c.Numero == dto.NumeroConta && c.PodeSerAcessadaPor(idUsuario, eAdmin));

        if (contaCliente is null) return Result.Fail(new NaoEncontradoError("Conta não encontrada"));

        if (contaCliente.Numero == contaCaixa.Numero)
            return Result.Fail(new ConflitoError("A conta Caixa é do banco e não pode ser movimentada diretamente"));

        var valor = new Valor(dto.Valor);
        contaCliente.Debitar(valor);
        contaCaixa.Creditar(valor);

        var idTransacao = await _unitOfWork.LancamentoRepository.ProximoIdTransacaoAsync();

        var lancamentoOrigem = _mapper.Map<Lancamento>(dto);
        lancamentoOrigem.IdTransacao = idTransacao;
        lancamentoOrigem.NumeroConta = contaCliente.Numero;
        lancamentoOrigem.Valor = new Valor(-dto.Valor);
        lancamentoOrigem.TipoOperacao = TipoOperacao.Saque;
        lancamentoOrigem.DataTransacao = DateTime.UtcNow;
        lancamentoOrigem.DataGravacao = DateTime.UtcNow;

        var lancamentoDestino = _mapper.Map<Lancamento>(dto);
        lancamentoDestino.IdTransacao = idTransacao;
        lancamentoDestino.NumeroConta = contaCaixa.Numero;
        lancamentoDestino.TipoOperacao = TipoOperacao.Saque;
        lancamentoDestino.DataTransacao = DateTime.UtcNow;
        lancamentoDestino.DataGravacao = DateTime.UtcNow;

        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoOrigem);
        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoDestino);
        await _unitOfWork.CommitAsync();
        _logger.LogInformation("Saque {IdTransacao} de {Valor} na conta {NumeroConta}", idTransacao, dto.Valor, contaCliente.Numero);

        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] { lancamentoOrigem, lancamentoDestino });
        return Result.Ok(listaReadDto);
    }
}
