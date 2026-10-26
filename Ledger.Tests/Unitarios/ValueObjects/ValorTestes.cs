using System.Globalization;
using Ledger.Domain.ValueObjects;

namespace Ledger.Tests.Unitarios.ValueObjects;

public class ValorTestes
{
    [Theory]
    [InlineData("10")]
    [InlineData("10.5")]
    [InlineData("10.55")]
    [InlineData("-100")]
    [InlineData("-120.50")]
    public void CriaValorComAteDuasCasasDecimais(string texto)
    {
        var numero = decimal.Parse(texto, CultureInfo.InvariantCulture);

        var valor = new Valor(numero);

        Assert.Equal(numero, valor.Value);
    }

    [Theory]
    [InlineData("10.555")]
    [InlineData("-0.001")]
    public void LancaExcecaoQuandoOValorTemMaisDeDuasCasasDecimais(string texto)
    {
        var numero = decimal.Parse(texto, CultureInfo.InvariantCulture);

        Assert.Throws<ArgumentException>(() => new Valor(numero));
    }

    [Fact]
    public void DoisValoresComOMesmoNumeroSaoIguais()
    {
        Assert.Equal(new Valor(120.50m), new Valor(120.50m));
    }
}
