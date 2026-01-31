using System.Diagnostics;
using System.Text;

namespace UserManagementAPI.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        await LogRequest(context);
        
        // Capture original response body stream
        var originalResponseBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        try
        {
            // Continue processing
            await _next(context);
            
            stopwatch.Stop();
            
            // Log response
            await LogResponse(context, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            // Restore original response stream before copying
            context.Response.Body = originalResponseBodyStream;

            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBodyStream);
        }
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();
        
        var requestBody = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Seek(0, SeekOrigin.Begin);
        }

        _logger.LogInformation(
            "HTTP Request: {Method} {Path} {QueryString} | Content-Type: {ContentType} | Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            context.Request.ContentType ?? "N/A",
            string.IsNullOrWhiteSpace(requestBody) ? "N/A" : requestBody);
    }

    private async Task LogResponse(HttpContext context, long elapsedMilliseconds)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation(
            "HTTP Response: {Method} {Path} | Status: {StatusCode} | Elapsed: {ElapsedMs}ms | Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMilliseconds,
            string.IsNullOrWhiteSpace(responseBody) ? "N/A" : responseBody);
    }
}
