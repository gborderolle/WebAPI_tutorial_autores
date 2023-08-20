using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Repository.Interfaces;

namespace WebAPI_tutorial_recursos.Controllers
{
    /// <summary>
    /// Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26946890#notes
    /// </summary>
    [ApiController]
    [Route("api/books/{bookId:int}/reviews")] // indica la dependencia 0..n de Reviews a Books (no existe review sin book). URL: primero el /book, después los /reviews. 
    public class ReviewsController : ControllerBase
    {
        private readonly ILogger<ReviewsController> _logger; // Logger para registrar eventos.
        private readonly IMapper _mapper;
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookRepository _bookRepository;
        protected APIResponse _response;

        public ReviewsController(ILogger<ReviewsController> logger, IMapper mapper, IReviewRepository reviewRepository, IBookRepository bookRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _reviewRepository = reviewRepository;
            _bookRepository = bookRepository;
            _response = new();
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
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return Ok(_response);
        }

        [HttpGet("{id:int}", Name = "GetReviewById")] // url completa: https://localhost:7003/api/authors/1
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDTO))] // tipo de dato del objeto de la respuesta, siempre devolver DTO
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetReview(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogError($"Error al obtener el review ID = {id}");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var review = await _reviewRepository.Get(v => v.Id == id);
                if (review == null)
                {
                    _logger.LogError($"El review ID = {id} no existe.");
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
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogError($"Ocurrió un error en el servidor.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(ModelState);
                }
                if (await _bookRepository.Get(v => v.Id == bookId) == null)
                {
                    _logger.LogError($"El libro ID = {bookId} no existe en el sistema");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    ModelState.AddModelError("BookNotFound", $"El libro ID = {bookId} no existe en el sistema.");
                    return BadRequest(ModelState);
                }

                var modelo = _mapper.Map<Review>(reviewCreateDTO);
                modelo.BookId = bookId;

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
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }
            return _response;
        }

    }
}
