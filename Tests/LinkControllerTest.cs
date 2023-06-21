using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShortLinks.Models;
using ShortLinks.Services;
using System.Threading.Tasks;
using ShortLinks.Controllers;

namespace ShortLinks.Tests;

public class LinkControllerTest
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILinkService> _mockLinkService;
    public LinkControllerTest(Mock<IHttpContextAccessor> mockHttpContextAccessor,
                              Mock<ILinkService> mockLinkService)
    {
        _mockHttpContextAccessor = mockHttpContextAccessor;
        _mockLinkService = mockLinkService;
    }

    [Fact]
    public async void CreateShortLink_ValidRequest_ReturnsOkResult()
    {
        var createLinkDto = new LinkDto { LinkPath = "https://dotnet.microsoft.com/en-us/apps/aspnet", PasswordToDel = "123098" };
        var expectedLink = new Link { LinkPath = createLinkDto.LinkPath, ShortLink = "abc123" };

        var context = new DefaultHttpContext();
        _mockLinkService.Setup(serviceInstance => serviceInstance.CreateLink(createLinkDto)).ReturnsAsync(expectedLink);
        var controller = new LinkController(_mockHttpContextAccessor.Object, _mockLinkService.Object);

        var result = await controller.CreateShortLink(createLinkDto);

        Assert.IsType<OkObjectResult>(result);
    }
}
