using WebAPI_tutorial_recursos.Utilities;

namespace WebAPI_tutorial_recursos.Middlewares
{
    public static class LogResponseMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogResponseHTTP(this IApplicationBuilder app)
        {
            // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26839760#notes
            return app.UseMiddleware<LogResponseMiddleware>();
        }
    }

    /// <summary>
    // Middleware para registrar todos los response del sistema
    // Clase 1: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/13816098#notes
    // Clase 2: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26839760#notes
    /// </summary>
    public class LogResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogResponseMiddleware> _logger;

        public LogResponseMiddleware(RequestDelegate next, ILogger<LogResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Invoke or InvokeAsync
        public async Task InvokeAsync(HttpContext context)
        {
            using (var memoryStream = new MemoryStream())
            {
                var responseBody = context.Response.Body;
                context.Response.Body = memoryStream;

                await _next(context);

                memoryStream.Seek(0, SeekOrigin.Begin);
                string response = new StreamReader(memoryStream).ReadToEnd(); // guarda la respuesta
                memoryStream.Seek(0, SeekOrigin.Begin);

                await memoryStream.CopyToAsync(responseBody);
                context.Response.Body = responseBody;

                _logger.LogInformation($"{GlobalServices.GetDatetimeUruguayString()}: " + response);
            }
        }

    }
}