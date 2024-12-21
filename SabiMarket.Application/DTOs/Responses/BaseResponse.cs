namespace SabiMarket.Application.DTOs.Responses
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object Metadata { get; set; }
    }

    public class BaseResponse<T> : BaseResponse
    {
        public T Data { get; set; }

        public BaseResponse()
        {
        }

        public BaseResponse(T data)
        {
            Data = data;
            Success = true;
        }

        public BaseResponse(string message)
        {
            Success = false;
            Message = message;
        }

        public BaseResponse(T data, string message)
        {
            Data = data;
            Success = true;
            Message = message;
        }
    }
}
