using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Runtime.Versioning;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;
using WebAPI_tutorial_recursos.Utilities.HATEOAS;

namespace WebAPI_tutorial_recursos.Controllers
{
    [ApiController]
    [Route("api/authors")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public class AuthorsController : ControllerBase
    {
        private readonly ILogger<AuthorsController> _logger; // Logger para registrar eventos.
        private readonly IMapper _mapper;
        private readonly IAuthorRepository _authorRepository; // Servicio que contiene la lógica principal de negocio para authors.
        private readonly IAuthorizationService _authorizationService;
        protected APIResponse _response;

        public AuthorsController(ILogger<AuthorsController> logger, IMapper mapper, IAuthorRepository authorRepository, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _mapper = mapper;
            _authorRepository = authorRepository;
            _authorizationService = authorizationService;
            _response = new();
        }

        #region Endpoints

        /// <summary>
        /// Tipos de retorno:
        /// 1. Tipo de dato puro síncrono: List<AutorDto>: no sirve
        /// 2. Tipo de dato puro síncrono: ActionResult<List<AutorDto>>: permite retornar objetos controlados: ResultOk() etc
        /// 3. Tipo de dato puro asíncrono: Task<ActionResult<List<AutorDto>>>: es asíncrono: no espera al método para seguir la ejecución
        /// 4. IActionResult: está depracated, no se usa más.
        /// 
        /// Programación síncrona
        /// Task: retorna void
        /// Task<T>: retorna un tipo de dato T
        /// Sólo usar síncrona cuando el método se conecta con otra API o con la BD: Task, async y await.
        /// 
        /// APIResponse: estandariza las respuestas con mensajes tipo http además de lo solicitado
        /// 
        /// Model binding: para recibir parámetros puede ser:
        /// Como parte de la url con "/" (sin nada): no especifica cómo llega. ej: GetAuthor(int id) --> "https://localhost:7003/api/authors/12"
        /// FromBody: ej: GetAuthor([FromBody] int id) --> "id va oculto en el body"
        /// FromHeader: ej: GetAuthor([FromHeader] int id) --> "id va oculto en el header"
        /// FromQuery: desde el QueryString. ej: GetAuthor([FromQuery] int id) --> ".com?id=12"
        /// "combinación": combina varios tipos de params. ej: GetAuthor([FromHeader] int id, [FromQuery] string nombre) --> ".com?nombre=felipe"
        /// FromForm: [FromForm] para recibir archivos
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetAuthors")] // url completa: https://localhost:7003/api/authors/
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AuthorDTO>))]
        [AllowAnonymous] // Permitido sin login
        [ServiceFilter(typeof(HATEOASAuthorFilterAttribute))]
        public async Task<ActionResult<List<APIResponse>>> Get()
        {
            try
            {
                var authorList = await _authorRepository.GetAllIncluding(null);
                if (authorList.Count == 0)
                {
                    _logger.LogError($"No hay autores.");
                    _response.ErrorMessages = new List<string> { $"No hay autores." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return NotFound(_response);
                }

                _logger.LogInformation("Obtener todas los autores.");
                _response.StatusCode = HttpStatusCode.OK;

                // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148814#notes
                // Navegación de los elementos (HATEOAS)
                // Uso de HATEOAS para listas, Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148838#notes
                _response.Result = _mapper.Map<List<AuthorDTO>>(authorList); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return Ok(_response);
        }

        [HttpGet("{id:int}", Name = "GetAuthorById")] // url completa: https://localhost:7003/api/authors/1
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDTOWithBooks))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAuthorFilterAttribute))]
        public async Task<ActionResult<APIResponse>> Get(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"Error al obtener el autor ID = {id}");
                    _response.ErrorMessages = new List<string> { $"Error al obtener el autor ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var thenIncludeConfig = new ThenIncludePropertyConfiguration<Author>
                {
                    IncludeExpression = b => b.AuthorsBooks,
                    ThenIncludeExpression = ab => ((AuthorBook)ab).Book
                };

                var author = await _authorRepository.Get(
                    v => v.Id == id,
                    thenIncludes: new[] { thenIncludeConfig }
                );

                if (author == null)
                {
                    _logger.LogError($"Autor no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Autor no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                var dto = _mapper.Map<AuthorDTOWithBooks>(author);

                _response.Result = dto;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("searchFirstAuthorByName/{name}", Name = "SearchFirstAuthorByName")] // url completa: https://localhost:7003/api/authors/gonzalo
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetFirst([FromRoute] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogError($"Error al obtener el autor nombre = {name}");
                    _response.ErrorMessages = new List<string> { $"Error al obtener el autor nombre = {name}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var author = await _authorRepository.Get(v => v.Name.ToLower().Contains(name.ToLower()));
                if (author == null)
                {
                    _logger.LogError($"Autor no encontrado Nombre = {name}.");
                    _response.ErrorMessages = new List<string> { $"Autor no encontrado Nombre = {name}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<AuthorDTO>(author);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("searchAllAuthorsByName/{name}", Name = "SearchAllAuthorsByName")] // url completa: https://localhost:7003/api/authors/gonzalo
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AuthorDTO>))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetByName([FromRoute] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogError($"Error al obtener autores con nombre = {name}");
                    _response.ErrorMessages = new List<string> { $"Error al obtener autores con nombre = {name}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var authorList = await _authorRepository.GetAll(v => v.Name.ToLower().Contains(name.ToLower()));
                if (authorList == null || authorList.Count == 0)
                {
                    _logger.LogError($"Autor no encontrado Nombre = {name}.");
                    _response.ErrorMessages = new List<string> { $"Autor no encontrado Nombre = {name}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<List<AuthorDTO>>(authorList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost(Name = "CreateAuthor")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthorDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<ActionResult<APIResponse>> Post([FromBody] AuthorCreateDTO authorCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Ocurrió un error en el servidor.");
                    _response.ErrorMessages = new List<string> { $"Ocurrió un error en el servidor." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(ModelState);
                }
                if (await _authorRepository.Get(v => v.Name.ToLower() == authorCreateDto.Name.ToLower()) != null)
                {
                    _logger.LogError($"El nombre {authorCreateDto.Name} ya existe en el sistema");
                    _response.ErrorMessages = new List<string> { $"El nombre {authorCreateDto.Name} ya existe en el sistema." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    ModelState.AddModelError("NameAlreadyExists", $"El nombre {authorCreateDto.Name} ya existe en el sistema.");
                    return BadRequest(ModelState);
                }

                Author modelo = _mapper.Map<Author>(authorCreateDto);
                modelo.Creation = DateTime.Now;
                modelo.Update = DateTime.Now;

                await _authorRepository.Create(modelo);
                _logger.LogInformation($"Se creó correctamente el autor Id:{modelo.Id}.");

                _response.Result = _mapper.Map<AuthorDTO>(modelo); // Siempre retorna el DTO genérico: AuthorDTO
                _response.StatusCode = HttpStatusCode.Created;

                // CreatedAtRoute -> Nombre de la ruta (del método): GetAuthorById
                // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/13816172#notes
                return CreatedAtRoute("GetAuthorById", new { id = modelo.Id }, _response); // objeto que devuelve (el que creó). 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteAuthor")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"El Id {id} es inválido.");
                    _response.ErrorMessages = new List<string> { $"El Id {id} es inválido." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var author = await _authorRepository.Get(v => v.Id == id);
                if (author == null)
                {
                    _logger.LogError($"Autor no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Autor no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                await _authorRepository.Remove(author);
                _logger.LogInformation($"Se eliminó correctamente el autor Id:{id}.");
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return BadRequest(_response);
        }

        // Endpoint para actualizar una author por ID.
        /// <summary>
        /// Clase PUT: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/13816178#notes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorCreateDTO"></param>
        /// <returns></returns>
        [HttpPut("{id:int}", Name = "UpdateAuthor")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> Put(int id, AuthorCreateDTO authorCreateDTO)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"Datos de entrada inválidos.");
                    _response.ErrorMessages = new List<string> { $"Datos de entrada inválidos." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var author = await _authorRepository.Get(v => v.Id == id, tracked: false);
                if (author == null)
                {
                    _logger.LogError($"Autor no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Autor no encontrado ID = {id}" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return BadRequest(_response);
                }

                author = _mapper.Map<Author>(authorCreateDTO);
                author.Id = id;
                var updatedAuthor = await _authorRepository.Update(author);

                //author = _mapper.Map(authorCreateDTO, author);
                //await _authorRepository.Save();

                _logger.LogInformation($"Se actualizó correctamente el autor Id:{id}.");
                _response.Result = _mapper.Map<AuthorDTO>(updatedAuthor);
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return BadRequest(_response);
        }

        // Endpoint para hacer una actualización parcial de una author por ID.
        /// <summary>
        /// Clase PATCH: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26946940#notes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchDto"></param>
        /// <returns></returns>
        [HttpPatch("{id:int}", Name = "UpdatePartialAuthor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<ActionResult<APIResponse>> Patch(int id, JsonPatchDocument<AuthorCreateDTO> patchDto)
        {
            try
            {
                // Validar entrada
                if (patchDto == null || id <= 0)
                {
                    _logger.LogError($"El Id {id} es inválido.");
                    _response.ErrorMessages = new List<string> { $"El Id {id} es inválido." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                // Obtener el DTO existente
                AuthorCreateDTO authorCreateDTO = _mapper.Map<AuthorCreateDTO>(await _authorRepository.Get(v => v.Id == id, tracked: false));
                if (authorCreateDTO == null)
                {
                    _logger.LogError($"Autor no encontrado ID = {id}.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                // Aplicar el parche
                patchDto.ApplyTo(authorCreateDTO, error =>
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                });

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Ocurrió un error en el servidor.");
                    _response.ErrorMessages = new List<string> { $"Ocurrió un error en el servidor." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(ModelState);
                }

                Author author = _mapper.Map<Author>(authorCreateDTO);
                var updatedAuthor = await _authorRepository.Update(author);
                _logger.LogInformation($"Se actualizó correctamente el autor Id:{id}.");

                _response.Result = _mapper.Map<AuthorDTO>(updatedAuthor);
                _response.StatusCode = HttpStatusCode.NoContent;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return Ok(_response);
        }

        #endregion

        #region Private methods

        #endregion

    }
}