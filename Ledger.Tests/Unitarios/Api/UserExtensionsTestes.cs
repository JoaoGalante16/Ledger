using System.Security.Claims;
using Ledger.Models;

namespace Ledger.Tests.Unitarios.Api;

public class UserExtensionsTestes
{
    private static ClaimsPrincipal Usuario(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Teste"));

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
