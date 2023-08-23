using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;
using WebAPI_tutorial_recursos.Utilities;
using WebAPI_tutorial_recursos.Utilities.HATEOAS;

namespace WebAPI_tutorial_recursos.Controllers.V1
{
    [ApiController]
    [Route("api/authors")] // Versionado URL, Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148870#notes
    [HasHeader("x-version", "1")] // "x-version": nombre inventado. "1": versión nro 1. Versionado headers, Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148898#notes
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")] // Requiere estar logueado con usuario ADMIN (salvo los Annonymous)
    public class AuthorsController : ControllerBase
    {
        private readonly ILogger<AuthorsController> _logger; // Logger para registrar eventos.
        private readonly IMapper _mapper;
        private readonly IAuthorRepository _authorRepository; // Servicio que contiene la lógica principal de negocio para authors.
        protected APIResponse _response;

        public AuthorsController(ILogger<AuthorsController> logger, IMapper mapper, IAuthorRepository authorRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _authorRepository = authorRepository;
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
        [HttpGet(Name = "GetAuthorsv1")] // url completa: https://localhost:7003/api/authors/
        [AllowAnonymous] // Permitido sin login
        [ServiceFilter(typeof(HATEOASAuthorFilterAttribute))]
        public async Task<ActionResult<List<APIResponse>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            try
            {
                var authorList = await _authorRepository.GetAllIncluding(orderBy: x => x.Name, ascendingOrder:false, httpContext: HttpContext, paginationDTO: paginationDTO);

                // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148814#notes
                // Navegación de los elementos (HATEOAS)
                // Uso de HATEOAS para listas, Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148838#notes
                _response.Result = _mapper.Map<List<AuthorDTO>>(authorList); ;
                _response.StatusCode = HttpStatusCode.OK;
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

        /// <summary>
        /// Obtener autor por ID
        /// </summary>
        /// <param name="id">ID del autor</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "GetAuthorByIdv1")] // url completa: https://localhost:7003/api/authors/1
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
                    IncludeExpression = b => b.AuthorBookList,
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

        /// <summary>
        /// Obtener el primer autor por nombre
        /// </summary>
        /// <param name="name">Nombre del autor</param>
        /// <returns></returns>
        [HttpGet("searchFirstAuthorByName/{name}", Name = "SearchFirstAuthorByNamev1")] // url completa: https://localhost:7003/api/authors/gonzalo
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

        /// <summary>
        /// Obtener todos los autores por nombre
        /// </summary>
        /// <param name="name">Nombre del autor</param>
        /// <returns></returns>
        [HttpGet("searchAllAuthorsByName/{name}", Name = "SearchAllAuthorsByNamev1")] // url completa: https://localhost:7003/api/authors/gonzalo
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

        /// <summary>
        /// Crear un autor
        /// </summary>
        /// <param name="authorCreateDto"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateAuthorv1")]
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
                return CreatedAtRoute("GetAuthorByIdv1", new { id = modelo.Id }, _response); // objeto que devuelve (el que creó). 
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

        /// <summary>
        /// Borrar un autor
        /// </summary>
        /// <param name="id">ID del autor</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "DeleteAuthorv1")]
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
        [HttpPut("{id:int}", Name = "UpdateAuthorv1")]
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
        [HttpPatch("{id:int}", Name = "UpdatePartialAuthorv1")]
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