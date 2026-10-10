using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Bogus.Extensions.Brazil;
using Ledger.Validation;

namespace Ledger.Tests.Unitarios.Validacao;

public class AtributosValidacao
{
    private static bool Valido(ValidationAttribute atributo, object? valor) =>
        atributo.GetValidationResult(valor, new ValidationContext(new object())) == ValidationResult.Success;

    // ---- CpfAttribute ----

    [Theory]
    [InlineData("52998224725")]
    [InlineData("11144477735")]
    public void CpfAceitaCpfValido(string cpf)
    {
        Assert.True(Valido(new CpfAttribute(), cpf));
    }

    [Fact]
    public void CpfAceitaCpfsGeradosPeloBogus()
    {
        for (var i = 0; i < 50; i++)
        {
            var cpf = new Bogus.Faker("pt_BR").Person.Cpf(includeFormatSymbols: false);
            Assert.True(Valido(new CpfAttribute(), cpf), $"CPF gerado foi rejeitado: {cpf}");
        }
    }

    [Theory]
    [InlineData("52998224735")]    // primeiro dígito verificador errado
    [InlineData("52998224726")]    // segundo dígito verificador errado
    [InlineData("5299822472")]     // 10 dígitos
    [InlineData("529982247250")]   // 12 dígitos
    [InlineData("11111111111")]    // todos iguais (passaria na conta dos dígitos)
    [InlineData("00000000000")]
    [InlineData("529.982.247-25")]
    [InlineData("5299822472a")]
    public void CpfRejeitaCpfInvalido(string cpf)
    {
        Assert.False(Valido(new CpfAttribute(), cpf));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CpfAceitaNuloEVazioPorqueAObrigatoriedadeVemDoRequired(string? cpf)
    {
        Assert.True(Valido(new CpfAttribute(), cpf));
    }

    // ---- MaximoDuasCasasDecimaisAttribute ----

    [Theory]
    [InlineData("10", true)]
    [InlineData("10.5", true)]
    [InlineData("10.55", true)]
    [InlineData("10.555", false)]
    [InlineData("0.001", false)]
    [InlineData("10.500", true)]
    public void MaximoDuasCasasDecimais(string valor, bool valido)
    {
        var numero = decimal.Parse(valor, CultureInfo.InvariantCulture);

        Assert.Equal(valido, Valido(new MaximoDuasCasasDecimaisAttribute(), numero));
    }
}
