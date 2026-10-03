using AutoMapper;
using FluentResults;
using Ledger.Data;
using Ledger.Data.Dtos.LancamentoDtos;
using Ledger.Data.UnitOfWork;
using Ledger.Models;
using Ledger.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ledger.Services;

public class LancamentoService : ILancamentoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LancamentoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ReadLancamentoDto>>> Criar(CreateLancamentoDto dto, string idUsuario, bool eAdmin)
    {
        var contaOrigem = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == dto.NumeroContaOrigem && (eAdmin || c.IdUsuario == idUsuario));
        var contaDestino = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero.Equals(dto.NumeroContaDestino));
        if (contaOrigem is null) return Result.Fail(new NaoEncontradoError("Conta de origem não encontrada"));
        if (contaDestino is null) return Result.Fail(new NaoEncontradoError("Conta de destino não encontrada"));

        if (contaOrigem.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de origem foi encerrada"));
        if (contaDestino.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de destino foi encerrada"));
        
        var saldo = await BuscarSaldo(dto.NumeroContaOrigem);
        if(saldo <  dto.Valor) return Result.Fail(new SaldoInsuficienteError("Saldo insuficiente"));

        var idTransacao = await _unitOfWork.LancamentoRepository.ProximoIdTransacao();

        var lancamentoOrigem = _mapper.Map<Lancamento>(dto);
        lancamentoOrigem.IdTransacao = idTransacao;
        lancamentoOrigem.NumeroConta = dto.NumeroContaOrigem;
        lancamentoOrigem.Valor = -dto.Valor;
        lancamentoOrigem.DataTransacao = DateTime.UtcNow;
        lancamentoOrigem.DataGravacao = DateTime.UtcNow;

        var lancamentoDestino = _mapper.Map<Lancamento>(dto);
        lancamentoDestino.IdTransacao = idTransacao;
        lancamentoDestino.NumeroConta = dto.NumeroContaDestino;
        lancamentoDestino.DataTransacao = DateTime.UtcNow;
        lancamentoDestino.DataGravacao = DateTime.UtcNow;

        await _unitOfWork.LancamentoRepository.Adicionar(lancamentoOrigem);
        await _unitOfWork.LancamentoRepository.Adicionar(lancamentoDestino);
        await _unitOfWork.Commit();

        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] { lancamentoOrigem, lancamentoDestino });
        return Result.Ok(listaReadDto);
    }

    public async Task<List<ReadLancamentoDto>> Listar(string idUsuario, bool eAdmin)
    {
        var query = (await _unitOfWork.LancamentoRepository.BuscarTodos());
        if (!eAdmin)
        {
            query = query.Where(l => l.Conta.IdUsuario.Equals(idUsuario));
        }
        var lancamentos = await query.ToListAsync();
        return _mapper.Map<List<ReadLancamentoDto>>(lancamentos);
    }

    public async Task<Result<ReadLancamentoDto>> Buscar(int id, string idUsuario, bool eAdmin)
    {
        var lancamento = await BuscarLancamentoAcessivel(id, idUsuario, eAdmin);
        if (lancamento is null) return Result.Fail(new NaoEncontradoError("Lancamento não encontrado"));
        var readDto = _mapper.Map<ReadLancamentoDto>(lancamento);
        return Result.Ok(readDto);
    }

    public async Task<Result<List<ReadLancamentoDto>>> BuscarParDeLancamentos(int idTransacao)
    {
        var query = await _unitOfWork.LancamentoRepository.BuscarTodos();
        var parLancamento = await query.Where(l => l.IdTransacao.Equals(idTransacao)).OrderBy(l => l.Valor).ToListAsync();
        if (parLancamento.Count != 2) return Result.Fail(new NaoEncontradoError("Par de lancamento não encontrado"));
        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(parLancamento);
        return Result.Ok(listaReadDto);
    }

    public async Task<Result<List<ReadLancamentoDto>>> CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto)
    {
        var parLancamentosReferencias = await BuscarParDeLancamentos(dto.IdLancamentoReferencia);
        if (parLancamentosReferencias.IsFailed) return Result.Fail(new NaoEncontradoError("Lancamentos de referência não encontrado"));

        var contaOrigem = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == parLancamentosReferencias.Value[0].NumeroConta);
        var contaDestino = await _unitOfWork.ContaRepository.BuscarPorPk(c => c.Numero == parLancamentosReferencias.Value[1].NumeroConta);
        
        if (contaOrigem.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de origem foi encerrada"));
        if (contaDestino.DataEncerramento is not null)
            return Result.Fail(new ConflitoError("A conta de destino foi encerrada"));

        var idTransacao = await _unitOfWork.LancamentoRepository.ProximoIdTransacao();

        var lancamentoOrigemCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoOrigemCorrecao.IdTransacao = idTransacao;
        lancamentoOrigemCorrecao.NumeroConta = parLancamentosReferencias.Value[0].NumeroConta;
        lancamentoOrigemCorrecao.IdLancamentoReferencia = parLancamentosReferencias.Value[0].Id;
        lancamentoOrigemCorrecao.DataTransacao = DateTime.UtcNow;
        lancamentoOrigemCorrecao.DataGravacao = DateTime.UtcNow;

        var lancamentoDestinoCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoDestinoCorrecao.IdTransacao = idTransacao;
        lancamentoDestinoCorrecao.NumeroConta = parLancamentosReferencias.Value[1].NumeroConta;
        lancamentoDestinoCorrecao.IdLancamentoReferencia = parLancamentosReferencias.Value[1].Id;
        lancamentoDestinoCorrecao.Valor = -dto.Valor;
        lancamentoDestinoCorrecao.DataTransacao = DateTime.UtcNow;
        lancamentoDestinoCorrecao.DataGravacao = DateTime.UtcNow;

        await _unitOfWork.LancamentoRepository.Adicionar(lancamentoOrigemCorrecao);
        await _unitOfWork.LancamentoRepository.Adicionar(lancamentoDestinoCorrecao);
        await _unitOfWork.Commit();
        
        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] {  lancamentoOrigemCorrecao, lancamentoDestinoCorrecao });
        return Result.Ok(listaReadDto);
    }
    
    private Task<Lancamento?> BuscarLancamentoAcessivel(int id, string idUsuario, bool eAdmin)
    {
        return _unitOfWork.LancamentoRepository.BuscarPorId(l =>
            l.Id == id && (eAdmin || l.Conta.IdUsuario == idUsuario));
    }
    
    private async Task<decimal> BuscarSaldo(int id)
    {
        var query = await _unitOfWork.LancamentoRepository.BuscarTodos();
        var saldo = await query.Where(l => l.NumeroConta == id).SumAsync(l => l.Valor);
        return saldo;
    }
}