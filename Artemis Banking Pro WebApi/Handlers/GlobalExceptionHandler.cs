using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ABP.Core.Application.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingApi.Handlers
{
public class GlobalExceptionHandler : IExceptionHandler
{
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            var status = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                ApiException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = status == StatusCodes.Status500InternalServerError ? "Error interno del servidor" : "La solicitud no pudo procesarse",
                Detail = status == StatusCodes.Status500InternalServerError ? "Ocurrió un error inesperado." : exception.Message,
                Type = $"https://httpstatuses.com/{status}"
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
