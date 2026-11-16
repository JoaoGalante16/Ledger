using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Application.Extensions;
using Ledger.Domain.Entities;
using Ledger.Domain.Enums;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;

namespace Ledger.Application.UseCases.Lancamentos;

public class Transferir
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public Transferir(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReadLancamentoDto>>> ExecutarAsync(CreateLancamentoDto dto, string idUsuario, bool eAdmin)
    {
        await _unitOfWork.IniciarTransacaoAsync();
        var contas = await _unitOfWork.ContaRepository.BloquearEBuscarAsync(dto.NumeroContaOrigem, dto.NumeroContaDestino);

        var contaOrigem = contas.SingleOrDefault(c => c.Numero == dto.NumeroContaOrigem && c.PodeSerAcessadaPor(idUsuario, eAdmin));
        var contaDestino = contas.SingleOrDefault(c => c.Numero == dto.NumeroContaDestino);
        if (contaOrigem is null) return Result.Fail(new NaoEncontradoError("Conta de origem não encontrada"));
        if (contaDestino is null) return Result.Fail(new NaoEncontradoError("Conta de destino não encontrada"));

        var numeroContaCaixa = await _unitOfWork.ContaRepository.BuscarNumeroContaCaixaAsync();
        if (contaOrigem.Numero == numeroContaCaixa || contaDestino.Numero == numeroContaCaixa)
            return Result.Fail(new ConflitoError("A conta Caixa é do banco e não pode ser movimentada diretamente"));

        var valor = new Valor(dto.Valor);
        contaOrigem.Debitar(valor);
        contaDestino.Creditar(valor);

        var idTransacao = await _unitOfWork.LancamentoRepository.ProximoIdTransacaoAsync();

        var lancamentoOrigem = _mapper.Map<Lancamento>(dto);
        lancamentoOrigem.IdTransacao = idTransacao;
        lancamentoOrigem.NumeroConta = dto.NumeroContaOrigem;
        lancamentoOrigem.Valor = new Valor(-dto.Valor);
        lancamentoOrigem.TipoOperacao = TipoOperacao.Transferencia;
        lancamentoOrigem.DataTransacao = DateTime.UtcNow;
        lancamentoOrigem.DataGravacao = DateTime.UtcNow;

        var lancamentoDestino = _mapper.Map<Lancamento>(dto);
        lancamentoDestino.IdTransacao = idTransacao;
        lancamentoDestino.NumeroConta = dto.NumeroContaDestino;
        lancamentoDestino.TipoOperacao = TipoOperacao.Transferencia;
        lancamentoDestino.DataTransacao = DateTime.UtcNow;
        lancamentoDestino.DataGravacao = DateTime.UtcNow;

        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoOrigem);
        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoDestino);
        await _unitOfWork.CommitAsync();

        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] { lancamentoOrigem, lancamentoDestino });
        return Result.Ok(listaReadDto);
    }
}
