using CMCCyberSecurity.Helpers;
using Newtonsoft.Json;
using System.Net;
using static CMCCyberSecurity.Helpers.EnumHelper;

namespace CMCCyberSecurity.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _log;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
        {
            _next = next;
            _log = log;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = 500;
            var message = "Đã xảy ra lỗi trong quá trình xử lý";
            var code = ECode.InternalServerError;
            switch (ex)
            {
                case UnauthorizedAccessException _:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    code = ECode.Unauthorized;
                    message = ex.Message;
                    break;
                case HttpStatusException exception:
                    statusCode = exception.StatusCode;
                    message = exception.Message;
                    code = exception.Code;
                    break;
            }

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = statusCode;
            _log.LogError(ex, ex.Message);
            var err = JsonConvert.SerializeObject(new ErrorModel()
            {
                ErrorCode = code,
                Message = message
            });
            return context.Response.WriteAsync(err);
        }
    }
}
