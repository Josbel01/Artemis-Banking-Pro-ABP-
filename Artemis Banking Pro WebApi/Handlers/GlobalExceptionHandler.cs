using ABP.Core.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingApi.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            string exceptionTitle = "An unexpected error occurred";
            string details = exception.Message;

            switch (exception)
            {
                case ApiException apiException:
                    //custom exception handling
                    switch (apiException.StatusCode)
                    {
                        case (int)HttpStatusCode.BadRequest:
                            exceptionTitle = "Bad Request";
                            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            break;
                        case (int)HttpStatusCode.InternalServerError:
                            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            break;
                        case (int)HttpStatusCode.NotFound:
                            exceptionTitle = "Not found";
                            httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            break;
                        case 0:
                            // ApiException without explicit status code defaults to 400
                            exceptionTitle = "Bad Request";
                            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            break;
                        default:
                            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            break;
                    }
                    break;
                case KeyNotFoundException:
                    exceptionTitle = "Not found";
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case ArgumentException:
                    exceptionTitle = "Bad Request";
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ABP.Core.Application.Exceptions.ValidationException:
                    exceptionTitle = "Bad Request";
                    details = ((ABP.Core.Application.Exceptions.ValidationException)exception).Errors.Aggregate((a,b) => a + ", " + b);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var problemDetails = new
            {
                Title = exceptionTitle,
                Status = httpContext.Response.StatusCode,
                Detail = details,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

            return true;
        }
    }
}
