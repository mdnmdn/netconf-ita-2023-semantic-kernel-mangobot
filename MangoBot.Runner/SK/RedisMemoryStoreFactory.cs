using Microsoft.SemanticKernel.Connectors.Memory.Redis;
using Microsoft.SemanticKernel.Memory;
using Microsoft.VisualBasic;
using StackExchange.Redis;

namespace MangoBot.Runner.SK;

public class RedisMemoryStoreFactory
{
    public static async Task<IMemoryStore> CreateSampleRedisMemoryStoreAsync()
    {
        var configuration = Config.Instance.RedisConnectionString;
        var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configuration);
        IDatabase database = connectionMultiplexer.GetDatabase();
        IMemoryStore store = new RedisMemoryStore(database, vectorSize: 1536);
        return store;
    }
}