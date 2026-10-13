using AutoMapper;
using Ledger.Data.Profiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ledger.Tests.Unitarios.Suporte;

public static class MapperDeTeste
{
    public static IMapper Criar()
    {
        var configuracao = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ContaProfile>();
            cfg.AddProfile<LancamentoProfile>();
        }, NullLoggerFactory.Instance);

        return configuracao.CreateMapper();
    }
}
