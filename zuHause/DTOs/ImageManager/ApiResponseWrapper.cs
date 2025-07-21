using System.ComponentModel.DataAnnotations;

namespace zuHause.DTOs.ImageManager
{
    public class ApiResponseWrapper<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public int? ErrorCode { get; set; }

        public static ApiResponseWrapper<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponseWrapper<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponseWrapper<T> ErrorResponse(string message, int? errorCode = null, List<string>? errors = null)
        {
            return new ApiResponseWrapper<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                Errors = errors
            };
        }

        public static ApiResponseWrapper<T> ValidationErrorResponse(List<string> errors)
        {
            return new ApiResponseWrapper<T>
            {
                Success = false,
                Message = "驗證失敗",
                ErrorCode = 400,
                Errors = errors
            };
        }
    }

    public class MvcResponseWrapper
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static MvcResponseWrapper SuccessResponse(object? data = null, string? message = null)
        {
            return new MvcResponseWrapper
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static MvcResponseWrapper ErrorResponse(string message, List<string>? errors = null)
        {
            return new MvcResponseWrapper
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}