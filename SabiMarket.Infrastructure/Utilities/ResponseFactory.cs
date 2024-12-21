using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Infrastructure.Utilities
{
    public static class ResponseFactory
    {
        public static BaseResponse<T> Success<T>(T data, string message = null)
        {
            return new BaseResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Operation completed successfully"
            };
        }

        public static BaseResponse<T> Fail<T>(string message, T data = default)
        {
            return new BaseResponse<T>
            {
                Success = false,
                Data = data,
                Message = message
            };
        }

        public static BaseResponse Success(string message = null)
        {
            return new BaseResponse
            {
                Success = true,
                Message = message ?? "Operation completed successfully"
            };
        }

        public static BaseResponse Fail(string message)
        {
            return new BaseResponse
            {
                Success = false,
                Message = message
            };
        }
    }
}
