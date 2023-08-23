using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Utilities.HATEOAS;

namespace WebAPI_tutorial_recursos.Services
{
    public class GenerateLinks
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActionContextAccessor _actionContextAccessor;

        public GenerateLinks(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor, IActionContextAccessor actionContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _actionContextAccessor = actionContextAccessor;
        }

        private IUrlHelper SetupURLHelper()
        {
            var factory = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(_actionContextAccessor.ActionContext);
        }

        private async Task<bool> IsAdmin()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var result = await _authorizationService.AuthorizeAsync(httpContext.User, "IsAdmin");
            return result.Succeeded;
        }

        public async Task AddLinksToAuthor(AuthorDTO authorDTO)
        {
            var isAdmin = await IsAdmin();
            var url = SetupURLHelper();

            authorDTO.Links.Add(new DataHATEOAS(link: url.Link("GetAuthorById", new { id = authorDTO.Id }), description: "self", method: "GET"));
            if (isAdmin)
            {
                authorDTO.Links.Add(new DataHATEOAS(link: url.Link("UpdateAuthor", new { id = authorDTO.Id }), description: "Actualizar autor", method: "PUT"));
                authorDTO.Links.Add(new DataHATEOAS(link: url.Link("DeleteAuthor", new { id = authorDTO.Id }), description: "Borrar actor", method: "DELETE"));
            }
        }

    }
}
