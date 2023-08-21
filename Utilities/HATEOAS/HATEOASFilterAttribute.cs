using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI_tutorial_recursos.Utilities.HATEOAS
{
    public class HATEOASFilterAttribute : ResultFilterAttribute
    {
        public bool MustIncludeHATEOAS(ResultExecutingContext context)
        {
            var result = context.Result as ObjectResult;
            if (!IsResponseSuccess(result))
            {
                return false;
            }
            var header = context.HttpContext.Request.Headers["includeHATEOAS"];
            if (header.Count == 0)
            {
                return false;
            }
            var value = header[0];
            if (!value.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }

        private bool IsResponseSuccess(ObjectResult result)
        {
            if (result == null || result.Value == null)
            {
                return false;
            }
            if (result.StatusCode.HasValue && !result.StatusCode.Value.ToString().StartsWith("2")) // Código 200... son los exitosos.
            {
                return false;
            }
            return true;
        }

    }
}