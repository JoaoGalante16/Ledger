using Ledger.Domain.ValueObjects;

namespace Ledger.Tests.Unitarios.ValueObjects;

public class CpfTestes
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("11144477735")]
    public void CriaCpfQuandoONumeroEhValido(string numero)
    {
        var cpf = new Cpf(numero);

        Assert.Equal(numero, cpf.Value);
    }

    [Theory]
    [InlineData("52998224735")]
    [InlineData("52998224726")]
    [InlineData("5299822472")]
    [InlineData("11111111111")]
    [InlineData("529.982.247-25")]
    [InlineData("5299822472a")]
    [InlineData("")]
    public void LancaExcecaoQuandoONumeroEhInvalido(string numero)
    {
        Assert.Throws<ArgumentException>(() => new Cpf(numero));
    }

    [Fact]
    public void DoisCpfsComOMesmoNumeroSaoIguais()
    {
        Assert.Equal(new Cpf("52998224725"), new Cpf("52998224725"));
    }
}
