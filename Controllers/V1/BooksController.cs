﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;
using WebAPI_tutorial_recursos.Utilities;

namespace WebAPI_tutorial_recursos.Controllers.V1
{
    [ApiController]
    [Route("api/books")]
    [HasHeader("x-version", "1")] // "x-version": nombre inventado. "1": versión nro 1. Versionado headers, Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148898#notes
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
            _response = new();
            _logger = logger;
            _mapper = mapper;
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }

        #region Endpoints

        [HttpGet(Name = "GetBooksv1")]
        public async Task<ActionResult<List<APIResponse>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            try
            {
                var bookList = await _bookRepository.GetAllIncluding(httpContext: HttpContext, paginationDTO: paginationDTO);

                _logger.LogInformation("Obtener todos los libros.");
                _response.Result = _mapper.Map<IEnumerable<BookDTO>>(bookList);
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

        [HttpGet("{id:int}", Name = "GetBookv1")]
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
                    ModelState.AddModelError("NotExists", "No se puede crear un libro sin autores.");
                    return BadRequest(ModelState);
                }

                var authorsList = await _authorRepository.GetAll(v => bookCreateDTO.AuthorsIds.Contains(v.Id));
                if (authorsList.Count != bookCreateDTO.AuthorsIds.Count)
                {
                    _logger.LogError("No existe uno de los autores recibidos.");
                    _response.ErrorMessages = new List<string> { $"No existe uno de los autores recibidos." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    ModelState.AddModelError("NotExists", "No existe uno de los autores recibidos.");
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

        [HttpPut("{id:int}", Name = "UpdateBookv1")]
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

        [HttpPatch("{id:int}", Name = "UpdatePartialBookByIdv1")]
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