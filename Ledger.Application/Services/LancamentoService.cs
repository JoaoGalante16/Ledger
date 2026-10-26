using AutoMapper;
using FluentResults;
using Ledger.Application.Dtos.Lancamentos;
using Ledger.Application.Errors;
using Ledger.Domain.Entities;
using Ledger.Domain.Enums;
using Ledger.Domain.Repositories;
using Ledger.Domain.ValueObjects;

namespace Ledger.Application.Services;

public class LancamentoService : ILancamentoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LancamentoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReadLancamentoDto>>> CriarAsync(CreateLancamentoDto dto, string idUsuario, bool eAdmin)
    {
        var contaOrigem = await _unitOfWork.ContaRepository.BuscarPorPkAsync(c => c.Numero == dto.NumeroContaOrigem && (eAdmin || c.IdUsuario == idUsuario));
        var contaDestino = await _unitOfWork.ContaRepository.BuscarPorPkAsync(c => c.Numero.Equals(dto.NumeroContaDestino));
        if (contaOrigem is null) return Result.Fail(new NaoEncontradoError("Conta de origem não encontrada"));
        if (contaDestino is null) return Result.Fail(new NaoEncontradoError("Conta de destino não encontrada"));

        if (contaOrigem.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de origem foi encerrada"));
        if (contaDestino.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de destino foi encerrada"));
        
        var saldo = await _unitOfWork.LancamentoRepository.SomarSaldoAsync(dto.NumeroContaOrigem);
        if(saldo <  dto.Valor) return Result.Fail(new SaldoInsuficienteError("Saldo insuficiente"));

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

    public async Task<List<ReadLancamentoDto>> ListarAsync(string idUsuario, bool eAdmin)
    {
        List<Lancamento> lancamentos;
        if (eAdmin)
        {
            lancamentos = await _unitOfWork.LancamentoRepository.BuscarTodosAsync();
        }
        else
        {
            lancamentos = await _unitOfWork.LancamentoRepository.BuscarPorUsuarioAsync(idUsuario);
        }
         
        return _mapper.Map<List<ReadLancamentoDto>>(lancamentos);
    }

    public async Task<Result<ReadLancamentoDto>> BuscarAsync(int id, string idUsuario, bool eAdmin)
    {
        var lancamento = await BuscarLancamentoAcessivelAsync(id, idUsuario, eAdmin);
        if (lancamento is null) return Result.Fail(new NaoEncontradoError("Lancamento não encontrado"));
        var readDto = _mapper.Map<ReadLancamentoDto>(lancamento);
        return Result.Ok(readDto);
    }

    public async Task<Result<List<ReadLancamentoDto>>> BuscarParDeLancamentosAsync(int idTransacao)
    {
        var parLancamento = await _unitOfWork.LancamentoRepository.BuscarPorTransacaoAsync(idTransacao);
        if (parLancamento.Count != 2) return Result.Fail(new NaoEncontradoError("Par de lancamento não encontrado"));
        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(parLancamento);
        return Result.Ok(listaReadDto);
    }

    public async Task<Result<List<ReadLancamentoDto>>> CriarLancamentoCorrecaoAsync(CreateLancamentoCorrecaoDto dto)
    {
        var parLancamentosReferencias = await BuscarParDeLancamentosAsync(dto.IdLancamentoReferencia);
        if (parLancamentosReferencias.IsFailed) return Result.Fail(new NaoEncontradoError("Lancamentos de referência não encontrado"));

        var contaOrigem = await _unitOfWork.ContaRepository.BuscarPorPkAsync(c => c.Numero == parLancamentosReferencias.Value[0].NumeroConta);
        var contaDestino = await _unitOfWork.ContaRepository.BuscarPorPkAsync(c => c.Numero == parLancamentosReferencias.Value[1].NumeroConta);
        
        if (contaOrigem.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de origem foi encerrada"));
        if (contaDestino.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de destino foi encerrada"));

        var idTransacao = await _unitOfWork.LancamentoRepository.ProximoIdTransacaoAsync();

        var lancamentoOrigemCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoOrigemCorrecao.IdTransacao = idTransacao;
        lancamentoOrigemCorrecao.NumeroConta = parLancamentosReferencias.Value[0].NumeroConta;
        lancamentoOrigemCorrecao.TipoOperacao = TipoOperacao.Correcao;
        lancamentoOrigemCorrecao.IdLancamentoReferencia = parLancamentosReferencias.Value[0].Id;
        lancamentoOrigemCorrecao.DataTransacao = DateTime.UtcNow;
        lancamentoOrigemCorrecao.DataGravacao = DateTime.UtcNow;

        var lancamentoDestinoCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoDestinoCorrecao.IdTransacao = idTransacao;
        lancamentoDestinoCorrecao.NumeroConta = parLancamentosReferencias.Value[1].NumeroConta;
        lancamentoDestinoCorrecao.IdLancamentoReferencia = parLancamentosReferencias.Value[1].Id;
        lancamentoDestinoCorrecao.Valor = new Valor(-dto.Valor);
        lancamentoDestinoCorrecao.TipoOperacao = TipoOperacao.Correcao;
        lancamentoDestinoCorrecao.DataTransacao = DateTime.UtcNow;
        lancamentoDestinoCorrecao.DataGravacao = DateTime.UtcNow;

        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoOrigemCorrecao);
        await _unitOfWork.LancamentoRepository.AdicionarAsync(lancamentoDestinoCorrecao);
        await _unitOfWork.CommitAsync();
        
        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] {  lancamentoOrigemCorrecao, lancamentoDestinoCorrecao });
        return Result.Ok(listaReadDto);
    }
    
    private Task<Lancamento?> BuscarLancamentoAcessivelAsync(int id, string idUsuario, bool eAdmin)
    {
        return _unitOfWork.LancamentoRepository.BuscarPorIdAsync(l =>
            l.Id == id && (eAdmin || l.Conta.IdUsuario == idUsuario));
    }
}