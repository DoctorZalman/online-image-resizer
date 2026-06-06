namespace ImageResizer.Middleware;

public class SessionMiddleware
{
    private const string CookieName = "session_id";
    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // - runs on every request - ensures session cookie exists
    public async Task InvokeAsync(HttpContext context)
    {
        if (
            !context.Request.Cookies.TryGetValue(CookieName, out var sessionId)
            || string.IsNullOrEmpty(sessionId)
        )
        {
            // - generate new session GUID for anonymous user
            sessionId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append(
                CookieName,
                sessionId,
                new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(24),
                }
            );
        }

        // - store sessionId in HttpContext for controllers and services to read
        context.Items["SessionId"] = sessionId;

        await _next(context);
    }
}
