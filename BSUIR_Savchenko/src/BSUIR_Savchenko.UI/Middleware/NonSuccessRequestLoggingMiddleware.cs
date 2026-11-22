namespace BSUIR_Savchenko.UI.Middleware;

public class NonSuccessRequestLoggingMiddleware(RequestDelegate next, Serilog.ILogger log)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        finally
        {
            var status = context.Response.StatusCode;
            if (status < 200 || status >= 300)
            {
                log.Warning("HTTP {Method} {Path} => {StatusCode}",
                    context.Request.Method, context.Request.Path.Value, status);
            }
        }
    }
}
