using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OrdersMicroservice.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Log the exception here (e.g., using a logging framework like Serilog, NLog, etc.)
                _logger.LogError($"{ex.GetType()}:{ex.Message}");

                if (ex.InnerException is not null)
                {
                    // Log the inner exception as well
                    _logger.LogError($"{ex.InnerException.GetType()}:{ex.InnerException.Message}");
                }

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                //Internal Server Error
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    StatusCode = httpContext.Response.StatusCode,
                    Message = ex.Message,
                    Type = ex.GetType()
                });
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
