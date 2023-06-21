using Microsoft.AspNetCore.Mvc;
using ShortLinks.Models;
using ShortLinks.Services;
using FileShare.Services;
using MongoDB.Driver;
using Visus.Cuid;

namespace ShortLinks.Controllers
{
    [Route("/")]
    [ApiController]
    public class LinkController : Controller
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<Link> _collection;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly string _serverUrl;
        private readonly RedisCacheService _redisCacheService;
        public LinkController(IMongoClient client,
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

        [HttpPost("api/v1/short")]
        public async Task<IResult> CreateShortLink(LinkDto createLinkDto)
        {
            byte[] salt = SodiumLibrary.CreateSalt();
            var hashedPassword = SodiumLibrary.HashPassword(createLinkDto.PasswordToDel, salt);

            var filter = Builders<Link>.Filter.Eq("LinkPath", createLinkDto.LinkPath);

            var isLinkExsist = await _collection.Find(filter).AnyAsync();
            if (isLinkExsist) return Results.BadRequest();

            Link newLink = new Link
            {
                LinkPath = createLinkDto.LinkPath,
                ShortLink = new Cuid2(5).ToString(),
                Salt = salt,
                PasswordToDel = hashedPassword
            };

            _collection.InsertOne(newLink);
            return Results.Ok(new PublicLinkView
            {
                LinkPath = newLink.LinkPath,
                ShortLink = _serverUrl + '/' + newLink.ShortLink
            });
        }

        [HttpPost("api/v1/short/deleteLink")]
        public async Task<IResult> DeleteLink(LinkDto deleteRequest)
        {
            var filter = Builders<Link>.Filter.Eq("LinkPath", deleteRequest.LinkPath);

            var linkToDelete = await _collection.Find(filter).SingleOrDefaultAsync();
            if (linkToDelete == null) return Results.NotFound();

            var isVerified = SodiumLibrary.VerifyPassword(deleteRequest.PasswordToDel, linkToDelete.Salt, linkToDelete.PasswordToDel);
            if (!isVerified) return Results.BadRequest();

            await _collection.DeleteOneAsync(filter);
            return Results.Ok();
        }

        [HttpGet("{shortPath}")]
        public async Task<IResult> RedirectToNativeLink(string shortPath)
        {
            var redisKey = "link_" + shortPath;
            var cachedLink = _redisCacheService.Get(redisKey);

            if (!string.IsNullOrEmpty(cachedLink)) return Results.Redirect(cachedLink);

            var filter = Builders<Link>.Filter.Eq("ShortLink", shortPath);
            var link = await _collection.Find(filter).SingleOrDefaultAsync();

            if (link == null) return Results.NotFound();
            
            _redisCacheService.Set(redisKey, link.LinkPath, TimeSpan.FromMinutes(10));
            return Results.Redirect(link.LinkPath);
        }        
    }
}
