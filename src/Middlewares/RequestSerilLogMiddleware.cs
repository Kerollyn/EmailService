using Serilog.Context;

namespace EmailService.Middlewares
{
  public class RequestSerilLogMiddleware
  {
    private readonly RequestDelegate _next;

    public RequestSerilLogMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public Task Invoke(HttpContext context)
    {
      using (LogContext.PushProperty("UserName", context?.User?.Identity?.Name ?? "anônimo"))
      {
        return _next.Invoke(context);
      }
    }
  }
}
