using System.Net;
using Newtonsoft.Json;

namespace Hotel_UI.Middleware
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

        public async Task InvokeAsync(HttpContext context)
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // تعیین وضعیت HTTP بر اساس نوع استثنا
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Internal Server Error. Please try again later.";

            if (exception is ArgumentException) // مثال: برای استثنای خاص
            {
                statusCode = (int)HttpStatusCode.BadRequest; // یا هر وضعیت مناسب دیگر
                message = exception.Message; // پیام خاص استثنا
            }

            // ثبت خطا
            _logger.LogError(exception, "An unhandled exception occurred.");

            // تنظیم پاسخ
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                statusCode = statusCode,
                message = message,
                detailed = context.RequestServices.GetService<IHostEnvironment>().IsDevelopment() ? exception.ToString() : null // جزئیات در محیط توسعه
            };

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
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
