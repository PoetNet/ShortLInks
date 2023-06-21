using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShortLinks.Models;
using ShortLinks.Services;
using System.Threading.Tasks;
using ShortLinks.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ShortLinks.Tests;

public class LinkControllerTest
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpRequest> _requestMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<ILinkService> _linkServiceMock;
    public LinkControllerTest()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _requestMock = new Mock<HttpRequest>();
        _httpContextMock = new Mock<HttpContext>();
        _linkServiceMock = new Mock<ILinkService>();
    }

    [Fact]
    public async void CreateShortLink_ValidRequest_ReturnsOkResult()
    {
        var createLinkDto = new LinkDto { LinkPath = "https://dotnet.microsoft.com/en-us/apps/aspnet", PasswordToDel = "123098" };
        var expectedLink = new Link { LinkPath = createLinkDto.LinkPath, ShortLink = "abc123" };

        _requestMock.Setup(_ => _.Scheme).Returns("http");
        _requestMock.Setup(_ => _.Host).Returns(new HostString("localhost:5277"));

        _httpContextMock.Setup(_ => _.Request).Returns(_requestMock.Object);
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContextMock.Object);

        _linkServiceMock.Setup(serviceInstance => serviceInstance.CreateLink(createLinkDto)).ReturnsAsync(expectedLink);

        var controller = new LinkController(_httpContextAccessorMock.Object, _linkServiceMock.Object);

        var result = await controller.CreateShortLink(createLinkDto);
        Assert.IsType<Ok<ShortLinks.Models.PublicLinkView>>(result);
    }
}
