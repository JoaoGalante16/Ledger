using Ledger.Data;
using Ledger.Data.LancamentoDtos;
using Ledger.Models;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Services;

public class LancamentoService : ILancamentoService
{
    private readonly LedgerContext _context;

    public LancamentoService(LedgerContext context)
    {
        _context = context;
    }

    public List<ReadLancamentoDto>? Criar(CreateLancamentoDto dto)
    {
        var contaOrigem = _context.Contas.FirstOrDefault(c => c.Numero.Equals(dto.NumeroContaOrigem));
        var contaDestino = _context.Contas.FirstOrDefault(c => c.Numero.Equals(dto.NumeroContaDestino));
        if(contaOrigem is null || contaDestino is null) return  null;
        
        var idTransacao = _context.Database
            .SqlQuery<int>($"SELECT nextval('\"TransacaoIdSeq\"') as \"Value\"")
            .First();

        var lancamentoOrigem = new Lancamento
        {
            IdTransacao = idTransacao,
            NumeroConta = dto.NumeroContaOrigem,
            Valor = -dto.Valor,
            DataTransacao = dto.DataTransacao,
            DataGravacao = DateTime.UtcNow,
            Descricao = dto.Descricao
        };

        var lancamentoDestino = new Lancamento
        {
            IdTransacao = idTransacao,
            NumeroConta = dto.NumeroContaDestino,
            Valor = dto.Valor,
            DataTransacao = dto.DataTransacao,
            DataGravacao = DateTime.UtcNow,
            Descricao = dto.Descricao
        };

        _context.Lancamentos.Add(lancamentoOrigem);
        _context.Lancamentos.Add(lancamentoDestino);
        _context.SaveChanges();

        return new List<ReadLancamentoDto>
        {
            new()
            {
                Id = lancamentoOrigem.Id, IdTransacao = lancamentoOrigem.IdTransacao,
                NumeroConta = lancamentoOrigem.NumeroConta, Valor = lancamentoOrigem.Valor,
                DataTransacao = lancamentoOrigem.DataTransacao, DataGravacao = lancamentoOrigem.DataGravacao,
                Descricao = lancamentoOrigem.Descricao
            },
            new()
            {
                Id = lancamentoDestino.Id, IdTransacao = lancamentoDestino.IdTransacao,
                NumeroConta = lancamentoDestino.NumeroConta, Valor = lancamentoDestino.Valor,
                DataTransacao = lancamentoDestino.DataTransacao, DataGravacao = lancamentoDestino.DataGravacao,
                Descricao = lancamentoDestino.Descricao
            }
        };
    }

    public List<ReadLancamentoDto> Listar()
    {
        return _context.Lancamentos.Select(l => new ReadLancamentoDto()
        {
            Id = l.Id,
            IdTransacao = l.IdTransacao,
            NumeroConta = l.NumeroConta,
            Valor = l.Valor,
            DataTransacao = l.DataTransacao,
            DataGravacao = l.DataGravacao,
            Descricao = l.Descricao,
            LancamentoReferenciaId = l.IdLancamentoReferencia
        }).ToList();
    }

    public ReadLancamentoDto? Buscar(int id)
    {
        var lancamento = _context.Lancamentos.FirstOrDefault(l => l.Id.Equals(id));
        if (lancamento is null) return null;
        return new ReadLancamentoDto()
        {
            Id = lancamento.Id,
            IdTransacao = lancamento.IdTransacao,
            NumeroConta = lancamento.NumeroConta,
            Valor = lancamento.Valor,
            DataTransacao = lancamento.DataTransacao,
            DataGravacao = lancamento.DataGravacao,
            Descricao = lancamento.Descricao,
            LancamentoReferenciaId = lancamento.IdLancamentoReferencia
        };
    }

    public List<ReadLancamentoDto>? BuscarParDeLancamentos(int idTransacao)
    {
        var parDtosLancamentos = new List<ReadLancamentoDto>();
        var parlancamento = _context.Lancamentos.Where(l => l.IdTransacao.Equals(idTransacao)).OrderBy(l => l.Valor).ToList();
        if (parlancamento.Count != 2) return null;
        var lancamentoOrigem = new ReadLancamentoDto()
        {
            Id = parlancamento[0].Id,
            IdTransacao = parlancamento[0].IdTransacao,
            NumeroConta = parlancamento[0].NumeroConta,
            Valor = parlancamento[0].Valor,
            DataTransacao = parlancamento[0].DataTransacao,
            DataGravacao = parlancamento[0].DataGravacao,
            Descricao = parlancamento[0].Descricao,
            LancamentoReferenciaId = parlancamento[0].IdLancamentoReferencia
        };

        var lancamentoDestino = new ReadLancamentoDto()
        {
            Id = parlancamento[1].Id,
            IdTransacao = parlancamento[1].IdTransacao,
            NumeroConta = parlancamento[1].NumeroConta,
            Valor = parlancamento[1].Valor,
            DataTransacao = parlancamento[1].DataTransacao,
            DataGravacao = parlancamento[1].DataGravacao,
            Descricao = parlancamento[1].Descricao,
            LancamentoReferenciaId = parlancamento[1].IdLancamentoReferencia
        };
        parDtosLancamentos.AddRange(lancamentoOrigem, lancamentoDestino);
        return parDtosLancamentos;
    }

    public List<ReadLancamentoDto>? CriarLancamentoCorrecao(CreateLancamentoCorrecaoDto dto)
    {
        var lancamentosReferencias = BuscarParDeLancamentos(dto.IdLancamentoReferencia);
        if (lancamentosReferencias is null) return null;
        
        var idTransacao = _context.Database
            .SqlQuery<int>($"SELECT nextval('\"TransacaoIdSeq\"') as \"Value\"")
            .First();
        
        var lancamentoOrigemCorrecao = new Lancamento()
        {
            IdTransacao = idTransacao,
            NumeroConta = lancamentosReferencias[0].NumeroConta,
            Valor = dto.Valor,
            DataTransacao = dto.DataTransacao,
            DataGravacao = DateTime.UtcNow,
            Descricao = dto.Descricao,
            IdLancamentoReferencia = dto.IdLancamentoReferencia
        };

        var lancamentoDestinoCorrecao = new Lancamento()
        {
            IdTransacao = idTransacao,
            NumeroConta =  lancamentosReferencias[1].NumeroConta,
            Valor = -dto.Valor,
            DataTransacao = dto.DataTransacao,
            DataGravacao = DateTime.UtcNow,
            Descricao = dto.Descricao,
            IdLancamentoReferencia = dto.IdLancamentoReferencia
        };

        _context.Lancamentos.Add(lancamentoOrigemCorrecao);
        _context.Lancamentos.Add(lancamentoDestinoCorrecao);
        _context.SaveChanges();
        
        return new List<ReadLancamentoDto>
        {
            new()
            {
                Id = lancamentoOrigemCorrecao.Id, IdTransacao = lancamentoOrigemCorrecao.IdTransacao,
                NumeroConta = lancamentoOrigemCorrecao.NumeroConta, Valor = lancamentoOrigemCorrecao.Valor,
                DataTransacao = lancamentoOrigemCorrecao.DataTransacao, DataGravacao = lancamentoOrigemCorrecao.DataGravacao,
                Descricao = lancamentoOrigemCorrecao.Descricao, LancamentoReferenciaId = lancamentoOrigemCorrecao.IdLancamentoReferencia
            },
            new()
            {
                Id = lancamentoDestinoCorrecao.Id, IdTransacao = lancamentoDestinoCorrecao.IdTransacao,
                NumeroConta = lancamentoDestinoCorrecao.NumeroConta, Valor = lancamentoDestinoCorrecao.Valor,
                DataTransacao = lancamentoDestinoCorrecao.DataTransacao, DataGravacao = lancamentoDestinoCorrecao.DataGravacao,
                Descricao = lancamentoDestinoCorrecao.Descricao, LancamentoReferenciaId = lancamentoDestinoCorrecao.IdLancamentoReferencia
            }
        };
    }
}