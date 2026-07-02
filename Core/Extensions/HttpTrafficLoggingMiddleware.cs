using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;
using System.Text;

namespace Core.Extensions;

public class HttpTrafficLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public HttpTrafficLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Enrich User Details
        var userId = context.User?.Identity?.Name ?? "";
        var traceId = context.TraceIdentifier;
        var ip = context.Connection?.RemoteIpAddress?.ToString() ?? "";
        var application = "KIP";

        LogContext.PushProperty("CreatedUserId", userId);
        LogContext.PushProperty("CreatedSurrogateId", "");
        LogContext.PushProperty("TraceIdentifier", traceId);
        LogContext.PushProperty("RemoteIpAddress", ip);
        LogContext.PushProperty("Application", application);
        LogContext.PushProperty("HttpMethod", context.Request.Method);
        LogContext.PushProperty("Path", context.Request.Path);

        // Capture Request Body
        context.Request.EnableBuffering();
        var requestBody = await ReadStreamInChunks(context.Request.Body);
        context.Request.Body.Position = 0;
        LogContext.PushProperty("RequestBody", string.IsNullOrEmpty(requestBody) ? null : requestBody);

        // Intercept Response Body
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        var stopwatch = Stopwatch.StartNew();

        Exception? caughtException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            
            context.Response.Body.Position = 0;
            var responseBody = await ReadStreamInChunks(context.Response.Body);
            context.Response.Body.Position = 0;
            await responseBodyStream.CopyToAsync(originalBodyStream);

            LogContext.PushProperty("ResponseBody", string.IsNullOrEmpty(responseBody) ? null : responseBody);
            LogContext.PushProperty("StatusCode", context.Response.StatusCode);
            LogContext.PushProperty("Elapsed", stopwatch.ElapsedMilliseconds);

            if (caughtException != null)
            {
                LogContext.PushProperty("Exception", caughtException.ToString());
                Serilog.Log.Error(caughtException, "HTTP Request Error");
            }
            else
            {
                Serilog.Log.Information("HTTP Request Completed");
            }
        }
    }

    private async Task<string> ReadStreamInChunks(Stream stream)
    {
        const int readChunkBufferLength = 4096;
        stream.Seek(0, SeekOrigin.Begin);
        using var textWriter = new StringWriter();
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true); // leaveOpen: true
        var readChunk = new char[readChunkBufferLength];
        int readChunkLength;
        do
        {
            readChunkLength = await reader.ReadBlockAsync(readChunk, 0, readChunkBufferLength);
            textWriter.Write(readChunk, 0, readChunkLength);
        } while (readChunkLength > 0);
        return textWriter.ToString();
    }
}
