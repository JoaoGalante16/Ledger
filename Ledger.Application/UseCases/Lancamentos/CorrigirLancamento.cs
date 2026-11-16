using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Domain.Entities;
using Ledger.Domain.Enums;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;

namespace Ledger.Application.UseCases.Lancamentos;

public class CorrigirLancamento
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CorrigirLancamento(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReadLancamentoDto>>> ExecutarAsync(CreateLancamentoCorrecaoDto dto)
    {
        var parReferencia = await _unitOfWork.LancamentoRepository.BuscarPorTransacaoAsync(dto.IdLancamentoReferencia);
        if (parReferencia.Count != 2) return Result.Fail(new NaoEncontradoError("Lancamentos de referência não encontrado"));

        if (parReferencia[0].TipoOperacao is TipoOperacao.Deposito or TipoOperacao.Saque)
            return Result.Fail(new ConflitoError("Tipo de operação não aceita correção"));

        var contaOrigem = await _unitOfWork.ContaRepository.BuscarPorPkAsync(c => c.Numero == parReferencia[0].NumeroConta);
        var contaDestino = await _unitOfWork.ContaRepository.BuscarPorPkAsync(c => c.Numero == parReferencia[1].NumeroConta);

        var valor = new Valor(dto.Valor);
        contaOrigem!.Creditar(valor);
        contaDestino!.Debitar(valor);

        var idTransacao = await _unitOfWork.LancamentoRepository.ProximoIdTransacaoAsync();

        var lancamentoOrigemCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoOrigemCorrecao.IdTransacao = idTransacao;
        lancamentoOrigemCorrecao.NumeroConta = parReferencia[0].NumeroConta;
        lancamentoOrigemCorrecao.TipoOperacao = TipoOperacao.Correcao;
        lancamentoOrigemCorrecao.IdLancamentoReferencia = parReferencia[0].Id;
        lancamentoOrigemCorrecao.DataTransacao = DateTime.UtcNow;
        lancamentoOrigemCorrecao.DataGravacao = DateTime.UtcNow;

        var lancamentoDestinoCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoDestinoCorrecao.IdTransacao = idTransacao;
        lancamentoDestinoCorrecao.NumeroConta = parReferencia[1].NumeroConta;
        lancamentoDestinoCorrecao.IdLancamentoReferencia = parReferencia[1].Id;
        lancamentoDestinoCorrecao.Valor = new Valor(-dto.Valor);
        lancamentoDestinoCorrecao.TipoOperacao = TipoOperacao.Correcao;
        lancamentoDestinoCorrecao.DataTransacao = DateTime.UtcNow;
        lancamentoDestinoCorrecao.DataGravacao = DateTime.UtcNow;

        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoOrigemCorrecao);
        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoDestinoCorrecao);
        await _unitOfWork.CommitAsync();

        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] { lancamentoOrigemCorrecao, lancamentoDestinoCorrecao });
        return Result.Ok(listaReadDto);
    }
}
