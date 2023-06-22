using Microsoft.AspNetCore.Mvc;
using ShortLinks.Models;
using ShortLinks.Services;
using FileShare.Services;
using MongoDB.Driver;
using Visus.Cuid;

namespace ShortLinks.Services;

public interface ILinkService
{
    Task<Link> CreateLink(LinkDto createLinkDto);
    Task<bool> Delete(LinkDto deleteRequest);
    Task<string> Redirect(string shortPath);
}

public class LinkService : ILinkService
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<Link> _collection;
    private readonly IHttpContextAccessor _httpAccessor;
    private readonly string _serverUrl;
    private readonly RedisCacheService _redisCacheService;
    public LinkService(IMongoClient client,
                       IHttpContextAccessor httpAccessor,
                       RedisCacheService redisCacheService)
    {
        _client = client;
        _db = _client.GetDatabase("Shorts");
        _collection = _db.GetCollection<Link>("Links");
        _httpAccessor = httpAccessor;
        _serverUrl = $"{_httpAccessor.HttpContext!.Request.Scheme}://{_httpAccessor.HttpContext.Request.Host.Value}";
        _redisCacheService = redisCacheService;
    }

    public async Task<Link> CreateLink(LinkDto createLinkDto)
    {
        byte[] salt = SodiumLibrary.CreateSalt();
        var hashedPassword = SodiumLibrary.HashPassword(createLinkDto.PasswordToDel, salt);

        var filter = Builders<Link>.Filter.Eq("LinkPath", createLinkDto.LinkPath);

        var isLinkExsist = await _collection.Find(filter).AnyAsync();
        if (isLinkExsist) return null!;

        Link newLink = new Link
        {
            LinkPath = createLinkDto.LinkPath,
            ShortLink = new Cuid2(5).ToString(),
            Salt = salt,
            PasswordToDel = hashedPassword
        };

        _collection.InsertOne(newLink);
        return newLink;
    }

    public async Task<bool> Delete(LinkDto deleteRequest)
    {
        var filter = Builders<Link>.Filter.Eq("LinkPath", deleteRequest.LinkPath);

        var linkToDelete = await _collection.Find(filter).SingleOrDefaultAsync();
        if (linkToDelete == null) return false;

        var isVerified = SodiumLibrary.VerifyPassword(deleteRequest.PasswordToDel, linkToDelete.Salt, linkToDelete.PasswordToDel);
        if (!isVerified) return false;

        await _collection.DeleteOneAsync(filter);
        return true;
    }

    public async Task<string> Redirect(string shortPath)
    {
        var redisKey = "link_" + shortPath;
        var cachedLink = _redisCacheService.Get(redisKey);

        if (!string.IsNullOrEmpty(cachedLink)) return cachedLink;

        var filter = Builders<Link>.Filter.Eq("ShortLink", shortPath);
        var link = await _collection.Find(filter).SingleOrDefaultAsync();

        if (link == null) return string.Empty;

        _redisCacheService.Set(redisKey, link.LinkPath, TimeSpan.FromMinutes(10));
        return link.LinkPath;
    }
}
