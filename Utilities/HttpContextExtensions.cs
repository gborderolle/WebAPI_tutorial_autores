using Microsoft.EntityFrameworkCore;

namespace WebAPI_tutorial_recursos.Utilities
{
    /// <summary>
    /// Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148924#notes
    /// </summary>
    public static class HttpContextExtensions
    {
        public async static Task InsertParamPaginationHeader<T>(this HttpContext httpContext, IQueryable<T> queryable)
        {
            if (httpContext == null) { throw new ArgumentNullException(nameof(httpContext)); }
            double count = await queryable.CountAsync();
            httpContext.Response.Headers.Add("totalSizeRecords", count.ToString());
        }

    }
}
