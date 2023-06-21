using StackExchange.Redis;

namespace ShortLinks.Services;

public class RedisCacheService
{
    private readonly ConnectionMultiplexer _redisConnection;

    public RedisCacheService(string connectionString)
    {
        _redisConnection = ConnectionMultiplexer.Connect(connectionString);
    }

    public string? Get(string key)
    {
        var db = _redisConnection.GetDatabase();
        return db.StringGet(key);
    }

    public void Set(string key, string value, TimeSpan expiry)
    {
        var db = _redisConnection.GetDatabase();
        db.StringSet(key, value, expiry);
    }

    public void Remove(string key)
    {
        var db = _redisConnection.GetDatabase();
        db.KeyDelete(key);
    }

}