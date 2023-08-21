using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository;
using WebAPI_tutorial_recursos.Repository.Interfaces;

namespace WebAPI_tutorial_recursos.Controllers
{
    /// <summary>
    /// Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26946890#notes
    /// </summary>
    [ApiController]
    [Route("api/books/{bookId:int}/reviews")] // indica la dependencia 0..n de Reviews a Books (no existe review sin book). URL: primero el /book, después los /reviews. 
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ReviewsController : ControllerBase
    {
        private readonly ILogger<ReviewsController> _logger; // Logger para registrar eventos.
        private readonly IMapper _mapper;
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookRepository _bookRepository;
        private readonly UserManager<IdentityUser> _userManager;
        protected APIResponse _response;

        public ReviewsController(ILogger<ReviewsController> logger, IMapper mapper, IReviewRepository reviewRepository, IBookRepository bookRepository, UserManager<IdentityUser> userManager)
        {
            _response = new();
            _logger = logger;
            _mapper = mapper;
            _reviewRepository = reviewRepository;
            _bookRepository = bookRepository;
            _userManager = userManager;
        }

        [HttpGet] // url completa: https://localhost:7003/api/books/{bookId}/reviews/
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ReviewDTO>))]
        public async Task<ActionResult<List<APIResponse>>> GetReviews(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    _logger.LogError($"Error al obtener el libro ID = {bookId}");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var book = await _bookRepository.Get(v => v.Id == bookId);
                if (book == null)
                {
                    _logger.LogError($"El libro ID = {bookId} no existe.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                var reviewList = await _reviewRepository.GetAll(v => v.BookId == bookId);
                if (reviewList.Count == 0)
                {
                    _logger.LogError($"No hay reviews.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return NotFound(_response);
                }

                _logger.LogInformation("Obtener todas los reviews.");
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<IEnumerable<ReviewDTO>>(reviewList);
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

        [HttpGet("{id:int}", Name = "GetReviewById")] // url completa: https://localhost:7003/api/authors/1
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetReview(int bookId, int id)
        {
            try
            {
                if (bookId <= 0 || id <= 0)
                {
                    _logger.LogError($"Datos de entrada no válidos.");
                    _response.ErrorMessages = new List<string> { $"Datos de entrada no válidos." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var book = await _bookRepository.Get(v => v.Id == bookId);
                if (book == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {bookId}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {bookId}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                var review = await _reviewRepository.Get(v => v.Id == id && v.BookId == bookId);
                if (review == null)
                {
                    _logger.LogError($"Review no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Review no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<ReviewDTO>(review);
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<ActionResult<APIResponse>> CreateReview(int bookId, ReviewCreateDTO reviewCreateDTO)
        {
            try
            {
                if (bookId <= 0)
                {
                    _logger.LogError($"Ocurrió un error en el servidor.");
                    _response.ErrorMessages = new List<string> { $"Ocurrió un error en el servidor." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Ocurrió un error en el servidor.");
                    _response.ErrorMessages = new List<string> { $"Ocurrió un error en el servidor." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(ModelState);
                }
                if (await _bookRepository.Get(v => v.Id == bookId) == null)
                {
                    _logger.LogError($"Libro no encontrado = {bookId}");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {bookId}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    ModelState.AddModelError("BookNotFound", $"Libro no encontrado ID = {bookId}.");
                    return BadRequest(ModelState);
                }

                // -----------
                var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "Email").FirstOrDefault();
                var email = emailClaim.Value;
                var user = await _userManager.FindByEmailAsync(email);
                var userId = user.Id;

                var modelo = _mapper.Map<Review>(reviewCreateDTO);
                modelo.BookId = bookId;
                modelo.UserId = userId;

                await _reviewRepository.Create(modelo);
                _logger.LogInformation($"Se creó correctamente el review Id:{modelo.Id}.");

                _response.Result = _mapper.Map<ReviewDTO>(modelo); // Siempre retorna el DTO genérico: AuthorDTO
                _response.StatusCode = HttpStatusCode.Created;

                // Cuidado que exista un endpoint con la misma firma
                return CreatedAtRoute("GetReviewById", new { id = modelo.Id, bookId = bookId }, _response); // objeto que devuelve (el que creó). 
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

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<ActionResult<APIResponse>> DeleteReview(int bookId, int id)
        {
            try
            {
                if (bookId<= 0 || id <= 0)
                {
                    _logger.LogError($"Datos de entrada no válidos.");
                    _response.ErrorMessages = new List<string> { $"Datos de entrada no válidos." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var book = await _bookRepository.Get(v => v.Id == bookId);
                if (book == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {bookId}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {bookId}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                var review = await _reviewRepository.Get(v => v.Id == id && v.BookId == bookId);
                if (review == null)
                {
                    _logger.LogError($"Review no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Review no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                await _reviewRepository.Remove(review);
                _logger.LogInformation($"Se eliminó correctamente el comentario id= {id}, del libro id={bookId}.");
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
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateReview(int bookId, int id, ReviewCreateDTO reviewCreateDTO)
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

                var book = await _bookRepository.Get(v => v.Id == bookId);
                if (book == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {bookId}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {bookId}" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return BadRequest(_response);
                }

                var review = await _reviewRepository.Get(v => v.Id == id && v.BookId == bookId, tracked: false);
                if (review == null)
                {
                    _logger.LogError($"Review no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Review no encontrado ID = {id}" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return BadRequest(_response);
                }

                review = _mapper.Map<Review>(reviewCreateDTO);
                review.Id = id;
                review.BookId = bookId;
                var updatedReview = await _reviewRepository.Update(review);

                //review = _mapper.Map(reviewCreateDTO, review);
                //await _reviewRepository.Save();

                _logger.LogInformation($"Se actualizó correctamente el review = {id}.");
                _response.Result = _mapper.Map<ReviewDTO>(updatedReview);
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

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        public async Task<ActionResult<APIResponse>> UpdatePartialBook(int bookId, int id, JsonPatchDocument<ReviewCreateDTO> patchDto)
        {
            try
            {
                // Validar entrada
                if (patchDto == null || id <= 0)
                {
                    _logger.LogError($"Datos de entrada no válidos: {id}.");
                    _response.ErrorMessages = new List<string> { $"Datos de entrada no válidos: {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var book = await _bookRepository.Get(v => v.Id == bookId);
                if (book == null)
                {
                    _logger.LogError($"Libro no encontrado ID = {bookId}.");
                    _response.ErrorMessages = new List<string> { $"Libro no encontrado ID = {bookId}" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return BadRequest(_response);
                }

                ReviewCreateDTO reviewUpdateDTO = _mapper.Map<ReviewCreateDTO>(await _reviewRepository.Get(v => v.Id == id && v.BookId == bookId, tracked: false));
                if (reviewUpdateDTO == null)
                {
                    _logger.LogError($"Review no encontrado ID = {id}.");
                    _response.ErrorMessages = new List<string> { $"Review no encontrado ID = {id}." };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                // Aplicar el parche
                patchDto.ApplyTo(reviewUpdateDTO, error =>
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

                Review review = _mapper.Map<Review>(reviewUpdateDTO);
                var updatedReview = await _reviewRepository.Update(review);
                _logger.LogInformation($"Se actualizó correctamente el review = {id}.");

                _response.Result = _mapper.Map<ReviewDTO>(updatedReview);
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

    }
}