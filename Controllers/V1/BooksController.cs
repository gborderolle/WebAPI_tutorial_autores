using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;

namespace WebAPI_tutorial_recursos.Controllers.V1
{
    [ApiController]
    [Route("api/v1/books")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public class BooksController : ControllerBase
    {
        private readonly ILogger<BooksController> _logger; // Logger para registrar eventos.
        private readonly IMapper _mapper;
        private readonly IBookRepository _bookRepository; // Servicio que contiene la lógica principal de negocio para libros.
        private readonly IAuthorRepository _authorRepository;
        protected APIResponse _response;

        public BooksController(ILogger<BooksController> logger, IMapper mapper, IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _bookRepository = bookRepository;
            _response = new();
            _authorRepository = authorRepository;
        }

        #region Endpoints

        [HttpGet(Name = "GetBooksv1")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookDTO>))]
        public async Task<ActionResult<List<APIResponse>>> Get()
        {
            try
            {
                //var bookList = await _repositoryBook.GetAll(includes: b => b.Author); // incluye los autores de cada libro
                var bookList = await _bookRepository.GetAll(); // incluye los autores de cada libro
                if (bookList.Count == 0)
                {
                    _logger.LogError($"No hay libros.");
                    _response.ErrorMessages = new List<string> { $"No hay libros." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return NotFound(_response);
                }

                _logger.LogInformation("Obtener todos los libros.");
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<IEnumerable<BookDTO>>(bookList);
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

        [HttpGet("{id:int}", Name = "GetBookv1")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTOWithAuthors))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> Get(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"Error al obtener el libro = {id}");
                    _response.ErrorMessages = new List<string> { $"Error al obtener el libro = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var includeConfigs = new List<IncludePropertyConfiguration<Book>>{
                    new IncludePropertyConfiguration<Book> { IncludeExpression = b => b.ReviewList }
                };

                var thenIncludeConfig = new ThenIncludePropertyConfiguration<Book>
                {
                    IncludeExpression = b => b.AuthorBookList,
                    ThenIncludeExpression = ab => ((AuthorBook)ab).Author
                };

                var book = await _bookRepository.Get(
                    v => v.Id == id,
                    includes: includeConfigs,
                    thenIncludes: new[] { thenIncludeConfig }
                );

                if (book == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                book.AuthorBookList = book.AuthorBookList.OrderBy(x => x.Order).ToList();

                _response.Result = _mapper.Map<BookDTOWithAuthors>(book);
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

        [HttpPost(Name = "CreateBookv1")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<ActionResult<APIResponse>> Post([FromBody] BookCreateDTO bookCreateDTO)
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
                if (bookCreateDTO.AuthorsIds == null)
                {
                    _logger.LogError("No se puede crear un libro sin autores.");
                    _response.ErrorMessages = new List<string> { $"No se puede crear un libro sin autores." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    ModelState.AddModelError("NotExists", "No se puede crear un libro sin autores.\");.");
                    return BadRequest(ModelState);
                }

                var authorsList = await _authorRepository.GetAll(v => bookCreateDTO.AuthorsIds.Contains(v.Id));
                if (authorsList.Count != bookCreateDTO.AuthorsIds.Count)
                {
                    _logger.LogError("No existe uno de los autores enviados.");
                    _response.ErrorMessages = new List<string> { $"No existe uno de los autores enviados." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    ModelState.AddModelError("NotExists", "No existe uno de los autores enviados.");
                    return BadRequest(ModelState);
                }

                Book model = _mapper.Map<Book>(bookCreateDTO);
                model = SetAuthorsOrder(model);

                await _bookRepository.Create(model);
                _logger.LogInformation($"Se creó correctamente el libro = {model.Id}.");

                _response.Result = _mapper.Map<BookDTO>(model);
                _response.StatusCode = HttpStatusCode.Created;


                // CreatedAtRoute -> Nombre de la ruta (del método): GetBook
                // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/13816172#notes
                return CreatedAtRoute("GetBookv1", new { id = model.Id }, _response); // objeto que devuelve (el que creó)
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

        [HttpDelete("{id:int}", Name = "DeleteBookv1")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<ActionResult<APIResponse>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"Datos de entrada no válidos: {id}.");
                    _response.ErrorMessages = new List<string> { $"Datos de entrada no válidos: {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var libro = await _bookRepository.Get(v => v.Id == id);
                if (libro == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                await _bookRepository.Remove(libro);
                _logger.LogInformation($"Se eliminó correctamente el libro = {id}.");
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

        // Endpoint para actualizar una libro por ID.
        [HttpPut("{id:int}", Name = "UpdateBookv1")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> Put(int id, BookCreateDTO bookCreateDTO)
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

                var thenIncludeConfig = new ThenIncludePropertyConfiguration<Book>
                {
                    IncludeExpression = b => b.AuthorBookList,
                    ThenIncludeExpression = ab => ((AuthorBook)ab).Author
                };

                var book = await _bookRepository.Get(
                    v => v.Id == id,
                    //tracked: false,
                    thenIncludes: new[] { thenIncludeConfig }
                );

                if (book == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {id}" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return BadRequest(_response);
                }

                book = SetAuthorsOrder(book);
                book = _mapper.Map(bookCreateDTO, book);
                await _bookRepository.Save();

                _logger.LogInformation($"Se actualizó correctamente el libro ID = {id}.");
                _response.Result = _mapper.Map<BookDTO>(book);
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { ex.ToString()
            };
            }
            return BadRequest(_response);
        }

        // Endpoint para hacer una actualización parcial de una libro por ID.
        [HttpPatch("{id:int}", Name = "UpdatePartialBookByIdv1")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<ActionResult<APIResponse>> Patch(int id, JsonPatchDocument<BookCreateDTO> patchDTO)
        {
            try
            {
                // Validar entrada
                if (patchDTO == null || id <= 0)
                {
                    _logger.LogError($"Datos de entrada no válidos: {id}.");
                    _response.ErrorMessages = new List<string> { $"Datos de entrada no válidos: {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                // Obtener el DTO existente
                BookCreateDTO bookCreateDTO = _mapper.Map<BookCreateDTO>(await _bookRepository.Get(v => v.Id == id, tracked: false));
                if (bookCreateDTO == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                // Aplicar el parche
                patchDTO.ApplyTo(bookCreateDTO, error =>
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

                Book libro = _mapper.Map<Book>(bookCreateDTO);
                var updatedBook = await _bookRepository.Update(libro);
                _logger.LogInformation($"Se actualizó correctamente el libro = {id}.");

                _response.Result = _mapper.Map<BookDTO>(updatedBook);
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

        private Book SetAuthorsOrder(Book model)
        {
            if (model.AuthorBookList != null)
            {
                for (int i = 0; i < model.AuthorBookList.Count; i++)
                {
                    model.AuthorBookList[i].Order = i;
                }
            }
            return model;
        }

        #endregion

    }
}