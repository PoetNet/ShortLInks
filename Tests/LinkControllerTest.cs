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
    // private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    // // private readonly Mock<HttpRequest> _mockHttpRequest;
    // // private readonly Mock<HttpContext> _mockHttpContext;
    // private readonly Mock<ILinkService> _mockLinkService;
    // public LinkControllerTest(Mock<IHttpContextAccessor> mockHttpContextAccessor,
    //                         //   Mock<HttpRequest> mockHttpRequest,
    //                         //   Mock<HttpContext> mockHttpContext,
    //                           Mock<ILinkService> mockLinkService)
    // {
    //     _mockHttpContextAccessor = mockHttpContextAccessor;
    //     // _mockHttpRequest = mockHttpRequest;
    //     // _mockHttpContext = mockHttpContext;
    //     _mockLinkService = mockLinkService;
    // }

    [Fact]
    public async void CreateShortLink_ValidRequest_ReturnsOkResult()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var linkServiceMock = new Mock<ILinkService>();
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();

        var createLinkDto = new LinkDto { LinkPath = "https://dotnet.microsoft.com/en-us/apps/aspnet", PasswordToDel = "123098" };
        var expectedLink = new Link { LinkPath = createLinkDto.LinkPath, ShortLink = "abc123" };

        requestMock.Setup(_ => _.Scheme).Returns("http");
        requestMock.Setup(_ => _.Host).Returns(new HostString("localhost:5277"));

        httpContextMock.Setup(_ => _.Request).Returns(requestMock.Object);
        httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        linkServiceMock.Setup(serviceInstance => serviceInstance.CreateLink(createLinkDto)).ReturnsAsync(expectedLink);

        var controller = new LinkController(httpContextAccessorMock.Object, linkServiceMock.Object);

        var result = await controller.CreateShortLink(createLinkDto);
        Assert.IsType<Ok<ShortLinks.Models.PublicLinkView>>(result);
    }
}
