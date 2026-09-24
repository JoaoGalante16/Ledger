using AutoMapper;
using Ledger.Data;
using Ledger.Data.LancamentoDtos;
using Ledger.Models;
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

    public List<ReadLancamentoDto>? Criar(CreateLancamentoDto dto)
    {
        var contaOrigem = _context.Contas.FirstOrDefault(c => c.Numero.Equals(dto.NumeroContaOrigem));
        var contaDestino = _context.Contas.FirstOrDefault(c => c.Numero.Equals(dto.NumeroContaDestino));
        if(contaOrigem is null || contaDestino is null) return  null;
        
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

        return _mapper.Map<List<ReadLancamentoDto>>(new[] { lancamentoOrigem, lancamentoDestino });
    }

    public List<ReadLancamentoDto> Listar()
    {
        return _mapper.Map<List<ReadLancamentoDto>>(_context.Lancamentos.ToList());
    }

    public ReadLancamentoDto? Buscar(int id)
    {
        var lancamento = _context.Lancamentos.FirstOrDefault(l => l.Id.Equals(id));
        if (lancamento is null) return null;
        return _mapper.Map<ReadLancamentoDto>(lancamento);
    }

    public List<ReadLancamentoDto>? BuscarParDeLancamentos(int idTransacao)
    {
        var parlancamento = _context.Lancamentos.Where(l => l.IdTransacao.Equals(idTransacao)).OrderBy(l => l.Valor).ToList();
        if (parlancamento.Count != 2) return null;
        return _mapper.Map<List<ReadLancamentoDto>>(parlancamento);

    }

    public List<ReadLancamentoDto>? CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto)
    {
        var parLancamentosReferencias = BuscarParDeLancamentos(dto.IdLancamentoReferencia);
        if (parLancamentosReferencias is null) return null;
        
        var idTransacao = _context.Database
            .SqlQuery<int>($"SELECT nextval('\"TransacaoIdSeq\"') as \"Value\"")
            .First();

        var lancamentoOrigemCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoOrigemCorrecao.IdTransacao = idTransacao;
        lancamentoOrigemCorrecao.NumeroConta = parLancamentosReferencias[0].NumeroConta;
        lancamentoOrigemCorrecao.IdLancamentoReferencia = parLancamentosReferencias[0].Id;
        lancamentoOrigemCorrecao.DataGravacao = DateTime.UtcNow;

        var lancamentoDestinoCorrecao = _mapper.Map<Lancamento>(dto);
        lancamentoDestinoCorrecao.IdTransacao = idTransacao;
        lancamentoDestinoCorrecao.NumeroConta = parLancamentosReferencias[1].NumeroConta;
        lancamentoDestinoCorrecao.IdLancamentoReferencia = parLancamentosReferencias[1].Id;
        lancamentoDestinoCorrecao.Valor = -dto.Valor;
        lancamentoDestinoCorrecao.DataGravacao = DateTime.UtcNow;

        _context.Lancamentos.Add(lancamentoOrigemCorrecao);
        _context.Lancamentos.Add(lancamentoDestinoCorrecao);
        _context.SaveChanges();
        
        return _mapper.Map<List<ReadLancamentoDto>>(new[] {  lancamentoOrigemCorrecao, lancamentoDestinoCorrecao });
    }
}