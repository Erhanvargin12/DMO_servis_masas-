using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Core.Extensions
{
    public class ExceptionMiddleware
    {
        private RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unhandled exception has occurred: {Message}", e.Message);
                await HandleExceptionAsync(httpContext, e);
            }
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception e)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            string message = "Internal Server Error";
            int statusCode = (int)HttpStatusCode.InternalServerError;

            IEnumerable<string> validationErrors = null;

            if (e.GetType() == typeof(ValidationException))
            {
                message = e.Message;
                statusCode = (int)HttpStatusCode.BadRequest;
                validationErrors = ((ValidationException)e).Errors.Select(x => x.ErrorMessage);
            }
            else if (e.GetType() == typeof(UnauthorizedAccessException))
            {
                message = e.Message;
                statusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                message = e.Message;
                
                // TODO: In production, do not return raw exception messages
                // if (env.IsProduction()) message = "An unexpected error occurred.";
            }

            httpContext.Response.StatusCode = statusCode;

            if (validationErrors != null)
            {
                return httpContext.Response.WriteAsync(new ValidationErrorDetails
                {
                    StatusCode = statusCode,
                    Message = message,
                    Errors = validationErrors
                }.ToString());
            }

            return httpContext.Response.WriteAsync(new ErrorDetails
            {
                StatusCode = statusCode,
                Message = message
            }.ToString());
        }
    }
}
