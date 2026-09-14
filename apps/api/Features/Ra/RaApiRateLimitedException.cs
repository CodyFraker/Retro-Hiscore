namespace RetroHiscore.Api.Features.Ra;

public sealed class RaApiRateLimitedException : HttpRequestException
{
    public RaApiRateLimitedException(string message)
        : base(message, inner: null, statusCode: System.Net.HttpStatusCode.TooManyRequests)
    {
    }
}
