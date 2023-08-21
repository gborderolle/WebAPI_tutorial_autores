using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Services;

namespace WebAPI_tutorial_recursos.Utilities.HATEOAS
{
    public class HATEOASAuthorFilterAttribute : HATEOASFilterAttribute
    {
        private readonly GenerateLinks _generateLinks;

        public HATEOASAuthorFilterAttribute(GenerateLinks generateLinks)
        {
            _generateLinks = generateLinks;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var mustInclude = MustIncludeHATEOAS(context);
            if (!mustInclude)
            {
                await next();
                return;
            }
            var result = context.Result as ObjectResult;
            if (result != null && result.Value != null)
            {
                var response = (APIResponse)result.Value;
                var authorDTO = response.Result as AuthorDTO;
                if (authorDTO == null)
                {
                    var authorListDTO = response.Result as List<AuthorDTO>;
                    if (authorListDTO == null)
                    {
                        throw new ArgumentNullException("Se esperaba una instancia de AuthorDTO.");
                    }
                    authorListDTO.ForEach(async author => await _generateLinks.AddLinksToAuthor(author));
                    result.Value = authorListDTO;
                }
                else
                {
                    await _generateLinks.AddLinksToAuthor(authorDTO);
                }
                await next();
            }
            else
            {
                throw new ArgumentNullException("Se esperaba una instancia de AuthorDTO.");
            }
        }

    }
}