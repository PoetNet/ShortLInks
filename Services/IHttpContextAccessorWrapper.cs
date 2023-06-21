namespace ShortLinks.Services;

public interface IHttpContextAccessorWrapper
{
    HttpContext HttpContext { get; }
}