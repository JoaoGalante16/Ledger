using AutoMapper;
using FluentResults;
using Ledger.Data;
using Ledger.Data.LancamentoDtos;
using Ledger.Models;
using Ledger.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ledger.Services;

public class LancamentoService : ILancamentoService
{
    private readonly LedgerContext _context;
    private readonly IMapper _mapper;

    public LancamentoService(LedgerContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Result<List<ReadLancamentoDto>> Criar(CreateLancamentoDto dto,  string idUsuario, bool eAdmin)
    {
        Conta? contaOrigem;
        if (eAdmin)
        {
            contaOrigem = _context.Contas.FirstOrDefault(c => c.Numero.Equals(dto.NumeroContaOrigem));
        }
        else
        {
            contaOrigem = _context.Contas.FirstOrDefault(c => c.Numero.Equals(dto.NumeroContaOrigem) && c.IdUsuario.Equals(idUsuario));
        }
        var contaDestino = _context.Contas.FirstOrDefault(c => c.Numero.Equals(dto.NumeroContaDestino));
        if (contaOrigem is null) return Result.Fail(new NaoEncontradoError("Conta de origem não encontrada"));
        if (contaDestino is null) return Result.Fail(new NaoEncontradoError("Conta de destino não encontrada"));
        
        var idTransacao = _context.Database
            .SqlQuery<int>($"SELECT nextval('\"TransacaoIdSeq\"') as \"Value\"")
            .First();

        var lancamentoOrigem = _mapper.Map<Lancamento>(dto);
        lancamentoOrigem.IdTransacao = idTransacao;
        lancamentoOrigem.NumeroConta = dto.NumeroContaOrigem;
        lancamentoOrigem.Valor = -dto.Valor;
        lancamentoOrigem.DataGravacao = DateTime.UtcNow;

        var lancamentoDestino = _mapper.Map<Lancamento>(dto);
        lancamentoDestino.IdTransacao = idTransacao;
        lancamentoDestino.NumeroConta = dto.NumeroContaDestino;
        lancamentoDestino.DataGravacao = DateTime.UtcNow;

        _context.Lancamentos.Add(lancamentoOrigem);
        _context.Lancamentos.Add(lancamentoDestino);
        _context.SaveChanges();

        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] { lancamentoOrigem, lancamentoDestino });
        return Result.Ok(listaReadDto);
    }

    public List<ReadLancamentoDto> Listar(string idUsuario, bool eAdmin)
    {
        if (eAdmin)
        {
            return _mapper.Map<List<ReadLancamentoDto>>(_context.Lancamentos.ToList());
        }
        return _mapper.Map<List<ReadLancamentoDto>>(_context.Lancamentos.Where(l => l.Conta.IdUsuario.Equals(idUsuario)));
    }

    public Result<ReadLancamentoDto> Buscar(int id, string idUsuario, bool eAdmin)
    {
        Lancamento? lancamento;
        if (eAdmin)
        {
            lancamento = _context.Lancamentos.FirstOrDefault(l => l.Id.Equals(id));
        }
        else
        {
            lancamento = _context.Lancamentos.FirstOrDefault(l => l.Id.Equals(id) && l.Conta.IdUsuario.Equals(idUsuario));
        }
        if (lancamento is null) return Result.Fail(new NaoEncontradoError("Lancamento não encontrado"));
        var readDto = _mapper.Map<ReadLancamentoDto>(lancamento);
        return Result.Ok(readDto);
    }

    public Result<List<ReadLancamentoDto>> BuscarParDeLancamentos(int idTransacao)
    {
        var parlancamento = _context.Lancamentos.Where(l => l.IdTransacao.Equals(idTransacao)).OrderBy(l => l.Valor).ToList();
        if (parlancamento.Count != 2) return Result.Fail(new NaoEncontradoError("Par de lancamento não encontrado"));
        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(parlancamento);
        return Result.Ok(listaReadDto);
    }

    public Result<List<ReadLancamentoDto>> CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto)
    {
        var parLancamentosReferencias = BuscarParDeLancamentos(dto.IdLancamentoReferencia);
        if (parLancamentosReferencias.IsFailed) return Result.Fail(new NaoEncontradoError("Lancamentos de referência não encontrado"));
        
        var idTransacao = _context.Database
            .SqlQuery<int>($"SELECT nextval('\"TransacaoIdSeq\"') as \"Value\"")
            .First();

        var lancamentoOrigemCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoOrigemCorrecao.IdTransacao = idTransacao;
        lancamentoOrigemCorrecao.NumeroConta = parLancamentosReferencias.Value[0].NumeroConta;
        lancamentoOrigemCorrecao.IdLancamentoReferencia = parLancamentosReferencias.Value[0].Id;
        lancamentoOrigemCorrecao.DataGravacao = DateTime.UtcNow;

        var lancamentoDestinoCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoDestinoCorrecao.IdTransacao = idTransacao;
        lancamentoDestinoCorrecao.NumeroConta = parLancamentosReferencias.Value[1].NumeroConta;
        lancamentoDestinoCorrecao.IdLancamentoReferencia = parLancamentosReferencias.Value[1].Id;
        lancamentoDestinoCorrecao.Valor = -dto.Valor;
        lancamentoDestinoCorrecao.DataGravacao = DateTime.UtcNow;

        _context.Lancamentos.Add(lancamentoOrigemCorrecao);
        _context.Lancamentos.Add(lancamentoDestinoCorrecao);
        _context.SaveChanges();
        
        var listaReadDto = _mapper.Map<List<ReadLancamentoDto>>(new[] {  lancamentoOrigemCorrecao, lancamentoDestinoCorrecao });
        return Result.Ok(listaReadDto);
    }
}