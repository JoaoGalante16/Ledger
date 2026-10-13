using System.Globalization;
using Ledger.Data.Dtos.ContaDtos;
using Ledger.Data.Dtos.LancamentoDtos;
using Ledger.Tests.Unitarios.Suporte;

namespace Ledger.Tests.Unitarios.Validacao;

public class DtosValidacao
{
    private static decimal Dec(string valor) => decimal.Parse(valor, CultureInfo.InvariantCulture);

    private static CreateLancamentoDto LancamentoValido() => new()
    {
        NumeroContaOrigem = 1,
        NumeroContaDestino = 2,
        Valor = 10.50m,
        Descricao = "Pix"
    };

    [Fact]
    public void CreateLancamentoDtoValidoNaoTemErros()
    {
        Assert.Empty(Validador.Validar(LancamentoValido()));
    }

    [Fact]
    public void CreateLancamentoDtoRejeitaOrigemIgualAoDestino()
    {
        var dto = LancamentoValido();
        dto.NumeroContaDestino = dto.NumeroContaOrigem;

        var erros = Validador.Validar(dto);

        Assert.Contains(erros, e => e.MemberNames.Contains(nameof(CreateLancamentoDto.NumeroContaDestino)));
    }

    [Theory]
    [InlineData("0.009", false)]
    [InlineData("0", false)]
    [InlineData("-5", false)]
    [InlineData("0.01", true)]
    [InlineData("1.005", false)]
    [InlineData("1999.99", true)]
    public void CreateLancamentoDtoValidaValor(string valor, bool valido)
    {
        var dto = LancamentoValido();
        dto.Valor = Dec(valor);

        Assert.Equal(!valido, Validador.TemErroEm(dto, nameof(CreateLancamentoDto.Valor)));
    }

    [Theory]
    [InlineData(250, true)]
    [InlineData(251, false)]
    public void CreateLancamentoDtoLimitaDescricaoA250Caracteres(int tamanho, bool valido)
    {
        var dto = LancamentoValido();
        dto.Descricao = new string('a', tamanho);

        Assert.Equal(!valido, Validador.TemErroEm(dto, nameof(CreateLancamentoDto.Descricao)));
    }

    [Theory]
    [InlineData(0, 2, nameof(CreateLancamentoDto.NumeroContaOrigem))]
    [InlineData(-1, 2, nameof(CreateLancamentoDto.NumeroContaOrigem))]
    [InlineData(1, 0, nameof(CreateLancamentoDto.NumeroContaDestino))]
    public void CreateLancamentoDtoRejeitaNumeroDeContaMenorQueUm(int origem, int destino, string campoComErro)
    {
        var dto = LancamentoValido();
        dto.NumeroContaOrigem = origem;
        dto.NumeroContaDestino = destino;

        Assert.True(Validador.TemErroEm(dto, campoComErro));
    }

    // ---- CreateLancamentoCorrecaoDto ----

    private static CreateLancamentoCorrecaoDto CorrecaoValida() => new()
    {
        Valor = 10m,
        Descricao = "Estorno",
        IdLancamentoReferencia = 1
    };

    [Fact]
    public void CreateLancamentoCorrecaoDtoValidoNaoTemErros()
    {
        Assert.Empty(Validador.Validar(CorrecaoValida()));
    }

    [Theory]
    [InlineData("0", false)]
    [InlineData("0.01", true)]
    [InlineData("10.555", false)]
    public void CreateLancamentoCorrecaoDtoValidaValor(string valor, bool valido)
    {
        var dto = CorrecaoValida();
        dto.Valor = Dec(valor);

        Assert.Equal(!valido, Validador.TemErroEm(dto, nameof(CreateLancamentoCorrecaoDto.Valor)));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void CreateLancamentoCorrecaoDtoExigeReferenciaMaiorQueZero(int idReferencia, bool valido)
    {
        var dto = CorrecaoValida();
        dto.IdLancamentoReferencia = idReferencia;

        Assert.Equal(!valido, Validador.TemErroEm(dto, nameof(CreateLancamentoCorrecaoDto.IdLancamentoReferencia)));
    }

    [Fact]
    public void CreateLancamentoCorrecaoDtoLimitaDescricaoA250Caracteres()
    {
        var dto = CorrecaoValida();
        dto.Descricao = new string('a', 251);

        Assert.True(Validador.TemErroEm(dto, nameof(CreateLancamentoCorrecaoDto.Descricao)));
    }

    // ---- CreateContaDto / UpdateContaDto ----

    [Fact]
    public void CreateContaDtoValidoNaoTemErros()
    {
        Assert.Empty(Validador.Validar(new CreateContaDto { Nome = "Conta", Cpf = "52998224725" }));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(250, true)]
    [InlineData(251, false)]
    public void CreateContaDtoValidaNome(object? nomeOuTamanho, bool valido)
    {
        var nome = nomeOuTamanho is int tamanho ? new string('a', tamanho) : (string?)nomeOuTamanho;
        var dto = new CreateContaDto { Nome = nome!, Cpf = "52998224725" };

        Assert.Equal(!valido, Validador.TemErroEm(dto, nameof(CreateContaDto.Nome)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345678900")] // dígito verificador errado
    [InlineData("529.982.247-25")]
    public void CreateContaDtoRejeitaCpfAusenteOuInvalido(string? cpf)
    {
        var dto = new CreateContaDto { Nome = "Conta", Cpf = cpf! };

        Assert.True(Validador.TemErroEm(dto, nameof(CreateContaDto.Cpf)));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(250, true)]
    [InlineData(251, false)]
    public void UpdateContaDtoValidaNome(int? tamanho, bool valido)
    {
        var dto = new UpdateContaDto { Nome = tamanho is null ? null! : new string('a', tamanho.Value) };

        Assert.Equal(!valido, Validador.TemErroEm(dto, nameof(UpdateContaDto.Nome)));
    }
}
