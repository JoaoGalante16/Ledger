using System.Security.Claims;
using Ledger.Models;

namespace Ledger.Tests.Unitarios.Api;

public class UserExtensionsTestes
{
    
    //monta o user na mao podendo passar as claims
    private static ClaimsPrincipal Usuario(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Teste"));

    // ---- ObterId ----

    [Fact]
    public void ObterIdDevolveAClaimNameIdentifier()
    {
        var usuario = Usuario(new Claim(ClaimTypes.NameIdentifier, "abc-123"));

        Assert.Equal("abc-123", usuario.ObterId());
    }

    [Fact]
    public void ObterIdLancaExcecaoQuandoNaoTemAClaim()
    {
        var usuario = Usuario(new Claim(ClaimTypes.Email, "joao@exemplo.com"));

        Assert.Throws<InvalidOperationException>(() => usuario.ObterId());
    }

    // ---- EhAdmin ----

    [Fact]
    public void EhAdminVerdadeiroComARoleAdmin()
    {
        Assert.True(Usuario(new Claim(ClaimTypes.Role, "Admin")).EhAdmin());
    }

    [Theory]
    [InlineData("Usuario")]
    [InlineData("admin")]
    public void EhAdminFalsoSemARoleAdmin(string role)
    {
        Assert.False(Usuario(new Claim(ClaimTypes.Role, role)).EhAdmin());
    }
}
