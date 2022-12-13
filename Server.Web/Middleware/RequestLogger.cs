using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Server.Web.Middleware;

public class RequestLogger
{
    private readonly RequestDelegate _next;

    public RequestLogger(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, ILogger<RequestLogger> logger)
    {
        var method = context.Request.Method;

        method = method switch
        {
            "DELETE" => "DEL",
            "OPTIONS" => "OPT",
            "PATCH" => "PAT",
            "TRACE" => "TRC",
            "CONNECT" => "CON",
            "HEAD" => "HED",
            "POST" => "POS",
            _ => method
        };

        try
        {
            var postData = string.Empty;

            if (context.Request.HasFormContentType)
            {
                postData = $" | Post Data: {string.Join(", ", context.Request.Form.Select(x => $"{x.Key}:{x.Value}"))}";
                var split = postData.Split('\n');

                if (split.Length > 1)
                    postData = $"{string.Join('\n', split.Take(1))}\nMore data found, but was concatenated...";
            }

            logger.LogTrace("INC {Method} {Path}{Query}{Post} | {IP}", method,
                context.Request.Path.Value, context.Request.QueryString, postData, GetIp(context, logger));

            await _next(context);

            if (context.Response.StatusCode != 404)
                logger.LogTrace("{StatusCode} {Method} {Path}", context.Response.StatusCode, method,
                    context.Request.Path.Value);
            else
                logger.LogWarning("{StatusCode} {Method} {Path}", context.Response.StatusCode, method,
                    context.Request.Path.Value);
        }
        catch (Exception ex)
        {
            logger.LogError("Error 500 {Method} {Path}", method, context.Request.Path.Value);
            logger.LogError(ex, "Unable to run web request");
            throw;
        }
    }

    private static string GetIp(HttpContext context, ILogger logger)
    {
        try
        {
            string ip = context.Request.Headers["X-Forwarded-For"]!;

            if (string.IsNullOrEmpty(ip))
                ip = context.Request.Headers["REMOTE_ADDR"]!;
            else
                // Using X-Forwarded-For last address
                ip = ip.Split(',')
                    .Last()
                    .Trim();

            if (context.Connection.RemoteIpAddress != null)
                return string.IsNullOrEmpty(ip) ? context.Connection.RemoteIpAddress.ToString() : ip;
        }
        catch (Exception ex)
        {
            logger.LogError("Error getting IP Address");
            logger.LogError(ex, "Unable to find IP");
        }

        return string.Empty;
    }
}
