using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_tutorial_recursos.Utilities.HATEOAS;

namespace WebAPI_tutorial_recursos.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class RootController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        #region Endpoints

        [HttpGet(Name = "GetRoot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DataHATEOAS>>> Get()
        {
            var isAdmin = await _authorizationService.AuthorizeAsync(User, "IsAdmin");
            var dataHateoas = new List<DataHATEOAS>();
            dataHateoas.Add(new DataHATEOAS(link: Url.Link("GetRoot", new { }), description: "self", method: "GET"));
            dataHateoas.Add(new DataHATEOAS(link: Url.Link("GetAuthors", new { }), description: "Autores", method: "GET"));

            if (isAdmin.Succeeded)
            {
                dataHateoas.Add(new DataHATEOAS(link: Url.Link("CreateAuthor", new { }), description: "Crear autor", method: "POST"));
                dataHateoas.Add(new DataHATEOAS(link: Url.Link("CreateBook", new { }), description: "Crear libro", method: "POST"));
            }
            return dataHateoas;
        }

        #endregion

        #region Private methods

        #endregion

    }
}