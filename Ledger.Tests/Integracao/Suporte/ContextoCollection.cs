namespace Ledger.Tests.Integracao.Suporte;

// todas classes da colecao compartilham o mesmo container
[CollectionDefinition(nameof(ContextoCollection))]
public class ContextoCollection : ICollectionFixture<ContextoFixture>
{
}
