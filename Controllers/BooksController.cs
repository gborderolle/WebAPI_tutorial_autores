using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;

namespace WebAPI_tutorial_recursos.Controllers
{
    [ApiController]
    [Route("api/books")]
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookDTO>))]
        public async Task<ActionResult<List<APIResponse>>> GetBooks()
        {
            try
            {
                //var bookList = await _repositoryBook.GetAll(includes: b => b.Author); // incluye los autores de cada libro
                var bookList = await _bookRepository.GetAll(); // incluye los autores de cada libro
                if (bookList.Count == 0)
                {
                    _logger.LogError($"No hay libros.");
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
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return Ok(_response);
        }

        [HttpGet("{id:int}", Name = "GetBook")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetBook(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"Error al obtener el libro = {id}");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var book = await _bookRepository.Get(v => v.Id == id, includes: b => b.Reviews); // incluye los autores del libro
                //var book = await _bookRepository.Get(v => v.Id == id); // incluye los autores del libro
                if (book == null)
                {
                    _logger.LogError($"El libro ID = {id} no existe.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<BookDTO>(book);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<ActionResult<APIResponse>> CreateBook([FromBody] BookCreateDTO libroCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Ocurrió un error en el servidor.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(ModelState);
                }
                if (libroCreateDto.AuthorsIds == null)
                {
                    _logger.LogError("No se puede crear un libro sin autores.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    ModelState.AddModelError("NotExists", "No se puede crear un libro sin autores.\");.");
                    return BadRequest(ModelState);
                }

                var authorsList = await _authorRepository.GetAll(v => libroCreateDto.AuthorsIds.Contains(v.Id));
                if (authorsList.Count!= libroCreateDto.AuthorsIds.Count)
                {
                    _logger.LogError("No existe uno de los autores enviados.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    ModelState.AddModelError("NotExists", "No existe uno de los autores enviados.");
                    return BadRequest(ModelState);
                }
              
                Book modelo = _mapper.Map<Book>(libroCreateDto);

                await _bookRepository.Create(modelo);
                _logger.LogInformation($"Se creó correctamente el libro = {modelo.Id}.");

                _response.Result = _mapper.Map<BookDTO>(modelo);
                _response.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetBook", new { id = modelo.Id }, _response); // objeto que devuelve (el que creó)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"Datos de entrada no válidos: {id}.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var libro = await _bookRepository.Get(v => v.Id == id);
                if (libro == null)
                {
                    _logger.LogError($"Registro no encontrado: {id}.");
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
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return BadRequest(_response);
        }

        // Endpoint para actualizar una libro por ID.
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookUpdateDTO updatedBookDto)
        {
            try
            {
                if (updatedBookDto == null || id != updatedBookDto.Id)
                {
                    _logger.LogError($"Datos de entrada no válidos: {id}.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var updatedBook = await _bookRepository.Update(_mapper.Map<Book>(updatedBookDto));
                _logger.LogInformation($"Se actualizó correctamente el libro = {id}.");
                _response.Result = _mapper.Map<BookDTO>(updatedBook);
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString()
            };
            }
            return BadRequest(_response);
        }

        // Endpoint para hacer una actualización parcial de una libro por ID.
        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<IActionResult> UpdatePartialBook(int id, JsonPatchDocument<BookUpdateDTO> patchDto)
        {
            try
            {
                // Validar entrada
                if (patchDto == null || id <= 0)
                {
                    _logger.LogError($"Datos de entrada no válidos: {id}.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                // Obtener el DTO existente
                BookUpdateDTO bookDto = _mapper.Map<BookUpdateDTO>(await _bookRepository.Get(v => v.Id == id, tracked: false));

                // Verificar si el libroDto existe
                if (bookDto == null)
                {
                    _logger.LogError($"No se encontró el libro = {id}.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                // Aplicar el parche
                patchDto.ApplyTo(bookDto, error =>
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                });

                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Ocurrió un error en el servidor.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(ModelState);
                }

                Book libro = _mapper.Map<Book>(bookDto);
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
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return Ok(_response);
        }

    }
}

