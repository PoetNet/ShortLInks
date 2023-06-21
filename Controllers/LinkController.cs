using Microsoft.AspNetCore.Mvc;
using ShortLinks.Models;
using ShortLinks.Services;

namespace ShortLinks.Controllers
{
    [Route("/")]
    [ApiController]
    public class LinkController : Controller
    {
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly string _serverUrl;
        private readonly ILinkService _linkService;
        public LinkController(IHttpContextAccessor httpAccessor,
                              ILinkService linkService)
        {
            _httpAccessor = httpAccessor;
            _serverUrl = $"{_httpAccessor.HttpContext!.Request.Scheme}://{_httpAccessor.HttpContext.Request.Host.Value}";
            _linkService = linkService;
        }

        [HttpPost("api/v1/short")]
        public async Task<IResult> CreateShortLink(LinkDto createLinkDto)
        {
            Link newLink = await _linkService.CreateLink(createLinkDto);
            if(newLink == null) return Results.BadRequest();

            return Results.Ok(new PublicLinkView
            {
                LinkPath = newLink.LinkPath,
                ShortLink = _serverUrl + '/' + newLink.ShortLink
            });
        }

        [HttpPost("api/v1/short/deleteLink")]
        public async Task<IResult> DeleteLink(LinkDto deleteRequest)
        {
            bool isDeleted = await _linkService.Delete(deleteRequest);
            if (!isDeleted) return Results.BadRequest();

            return Results.Ok();
        }

        [HttpGet("{shortPath}")]
        public async Task<IResult> RedirectToNativeLink(string shortPath)
        {
            return await _linkService.Redirect(shortPath);
        }        
    }
}
