using Microsoft.AspNetCore.Mvc;
using ShortLinks.Models;
using FileShare.Services;
using MongoDB.Driver;
using Visus.Cuid;

namespace ShortLinks.Controllers
{
    [Route("api/v1/short")]
    [ApiController]
    public class LinkController
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<Link> _collection;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly string _serverUrl;
        public LinkController(IMongoClient client,
                              IHttpContextAccessor httpAccessor)
        {
            _client = client;
            _db = _client.GetDatabase("Shorts");
            _collection = _db.GetCollection<Link>("Links");
            _httpAccessor = httpAccessor;
            _serverUrl = $"{_httpAccessor.HttpContext!.Request.Scheme}://{_httpAccessor.HttpContext.Request.Host.Value}";
        }

        [HttpPost]
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
                ShortLink = _serverUrl + '/' + (new Cuid2(5)).ToString(),
                Salt = salt,
                PasswordToDel = hashedPassword
            };

            _collection.InsertOne(newLink);
            return Results.Ok(new PublicLinkView
            {
                LinkPath = newLink.LinkPath,
                ShortLink = newLink.ShortLink
            });
        }

        // [HttpGet("{filename}")]
        // public IResult GetFile(string fileName)
        // {
        //     var fileInfo = _fileProvider.GetFileInfo(fileName);
        //     if (!fileInfo.Exists) return Results.NotFound();

        //     return Results.File(fileInfo.PhysicalPath!);
        // }

        // [HttpGet]
        // public IResult GetAllFiles()
        // {
        //     return Results.Ok(new
        //     {
        //         Data = _context.Files.ToList(),
        //         StatusCode = StatusCodes.Status200OK,
        //         Success = true
        //     });
        // }

        [HttpPost("deleteLink")]
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

        // public async Task DeleteFileAsync(CustomFile fileToDelete)
        // {
        //     using (var transaction = await _context.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             File.Delete(fileToDelete.Path);
        //             _context.Files.Remove(fileToDelete);
        //             await _context.SaveChangesAsync();

        //             await transaction.CommitAsync();
        //         }
        //         catch (Exception)
        //         {
        //             await transaction.RollbackAsync();
        //         }
        //     }
        // }

        // public async Task DeleteFileByTimer(CustomFile fileToDelete)
        // {
        //     if (fileToDelete.DelTime <= DateTime.UtcNow)
        //     {
        //         if (File.Exists(fileToDelete.Path))
        //             await DeleteFileAsync(fileToDelete);

        //     }
        // }
    }
}
