using MassTransit;
using SytsBackendGen2.Infrastructure.Caching;

namespace SytsBackendGen2.Infrastructure.AsyncMessaging;

internal class SqlCachedKeysCachingHandler : IConsumer<CachedKeyDataMessage>
{
    private readonly ICachedKeysContext _context;

    public SqlCachedKeysCachingHandler(ICachedKeysContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<CachedKeyDataMessage> context)
    {
        await AsyncSqlCachedKeysProvider.TryCompleteFormationAsync(context.Message.CachedKey, _context);
    }
}
