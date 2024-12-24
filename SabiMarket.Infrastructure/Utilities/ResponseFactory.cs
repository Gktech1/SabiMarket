using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Domain.Models;

public static class ResponseFactory
{
    public static BaseResponse<T> Success<T>(T data, string message = "Success")
    {
        return new BaseResponse<T>
        {
            Data = data,
            Message = message,
            Status = true
        };
    }

    public static BaseResponse<T> Fail<T>(Exception exception, string message)
    {
        return new BaseResponse<T>
        {
            Data = default,
            Message = message,
            Status = false,
            Error = new ErrorResponse
            {
                Type = exception.GetType().Name,
                Message = exception.Message,
                StatusCode = GetStatusCode(exception)
            }
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbidException => StatusCodes.Status403Forbidden,
            BadRequestException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}


