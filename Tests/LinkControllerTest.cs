using Moq;
using ShortLinks.Models;
using ShortLinks.Services;
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

        SetupHttpContext("http", "localhost:5277");
        _linkServiceMock.Setup(serviceInstance => serviceInstance.CreateLink(createLinkDto)).ReturnsAsync(expectedLink);

        var controller = new LinkController(_httpContextAccessorMock.Object, _linkServiceMock.Object);

        var result = await controller.CreateShortLink(createLinkDto);
        Assert.IsType<Ok<ShortLinks.Models.PublicLinkView>>(result);
    }

    [Fact]
    public async void CreateShortLink_InvalidRequest_ReturnsOkResult()
    {
        var createLinkDto = new LinkDto { LinkPath = "https://dotnet.microsoft.com/en-us/apps/aspnet", PasswordToDel = "123098" };
        var expectedLink = new Link { LinkPath = createLinkDto.LinkPath, ShortLink = "abc123" };

        SetupHttpContext("http", "localhost:5277");
        _linkServiceMock.Setup(serviceInstance => serviceInstance.CreateLink(createLinkDto)).ReturnsAsync((Link)null!);

        var controller = new LinkController(_httpContextAccessorMock.Object, _linkServiceMock.Object);

        var result = await controller.CreateShortLink(createLinkDto);
        Assert.IsType<BadRequest>(result);
    }

    [Fact]
    public async void DeleteLink_ValidRequest_ReturnsOkResult()
    {
        LinkDto deleteRequest = new LinkDto();
        _linkServiceMock.Setup(serviceInstance => serviceInstance.Delete(deleteRequest)).ReturnsAsync(true);

        SetupHttpContext("http", "localhost:5277");
        var controller = new LinkController(_httpContextAccessorMock.Object, _linkServiceMock.Object);

        var result = await controller.DeleteLink(deleteRequest);
        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async void DeleteLink_InvalidRequest_ReturnsOkResult()
    {
        LinkDto deleteRequest = new LinkDto();
        _linkServiceMock.Setup(serviceInstance => serviceInstance.Delete(deleteRequest)).ReturnsAsync(false);

        SetupHttpContext("http", "localhost:5277");
        var controller = new LinkController(_httpContextAccessorMock.Object, _linkServiceMock.Object);

        var result = await controller.DeleteLink(deleteRequest);
        Assert.IsType<BadRequest>(result);
    }

    [Fact]
    public async void Redirect_ValidRequest_ReturnsOkResult()
    {
        string shortPath = "rjtk4";
        _linkServiceMock.Setup(serviceInstance => serviceInstance.Redirect(shortPath)).ReturnsAsync("https://yandex.ru");

        SetupHttpContext("http", "localhost:5277");
        var controller = new LinkController(_httpContextAccessorMock.Object, _linkServiceMock.Object);

        var result = await controller.RedirectToNativeLink(shortPath);
        Assert.IsType<RedirectHttpResult>(result);
    }    

    [Fact]
    public async void Redirect_InvalidRequest_ReturnsOkResult()
    {
        string shortPath = "rjtk4";
        _linkServiceMock.Setup(serviceInstance => serviceInstance.Redirect(shortPath)).ReturnsAsync(string.Empty);

        SetupHttpContext("http", "localhost:5277");
        var controller = new LinkController(_httpContextAccessorMock.Object, _linkServiceMock.Object);

        var result = await controller.RedirectToNativeLink(shortPath);
        Assert.IsType<BadRequest>(result);
    }   

    private void SetupHttpContext(string scheme, string host)
     {
        _requestMock.Setup(_ => _.Scheme).Returns(scheme);
        _requestMock.Setup(_ => _.Host).Returns(new HostString(host));

        _httpContextMock.Setup(_ => _.Request).Returns(_requestMock.Object);
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContextMock.Object);
     }
}
