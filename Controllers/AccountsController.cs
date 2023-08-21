using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WebAPI_tutorial_recursos.DTOs;
using WebAPI_tutorial_recursos.Models;
using WebAPI_tutorial_recursos.Utilities;

namespace WebAPI_tutorial_recursos.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountsController> _logger; // Logger para registrar eventos.
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        protected APIResponse _response;

        public AccountsController(ILogger<AccountsController> logger, UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _response = new();
        }

        #region Endpoints

        [HttpPost("register", Name = "Register")] //api/accounts/register
        public async Task<ActionResult<APIResponse>> Register(UserCredential userCredential)
        {
            try
            {
                var user = new IdentityUser { UserName = userCredential.Email, Email = userCredential.Email };
                var result = await _userManager.CreateAsync(user, userCredential.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Registración correcta.");
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = await TokenSetup(userCredential);
                }
                else
                {
                    _logger.LogError($"Registración incorrecta.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(result.Errors);
                }
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

        [HttpPost("login",Name = "Login")]
        public async Task<ActionResult<APIResponse>> Login(UserCredential userCredential)
        {
            try
            {
                // lockoutOnFailure: bloquea al usuario si tiene muchos intentos de logueo
                var result = await _signInManager.PasswordSignInAsync(userCredential.Email, userCredential.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login correcto.");
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = await TokenSetup(userCredential);
                }
                else
                {
                    _logger.LogError($"Login incorrecto.");
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest("Login incorrecto"); // respuesta genérica para no revelar información
                }
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
        /// Renueva el token automáticamente
        /// Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27047668#notes
        /// </summary>
        /// <returns></returns>
        [HttpGet("RenewToken", Name = "RenewToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<APIResponse>> RenewToken()
        {
            try
            {
                var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "Email").FirstOrDefault();
                var email = emailClaim.Value;
                var userCredential = new UserCredential()
                {
                    Email = email
                };
                _response.Result = await TokenSetup(userCredential);
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

        [HttpPost("MakeAdmin", Name = "MakeAdmin")]
        public async Task<ActionResult<APIResponse>> MakeAdmin(EditAdminDTO editAdminDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(editAdminDTO.Email);
                await _userManager.AddClaimAsync(user, new Claim("IsAdmin", "1"));
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = NoContent();
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

        [HttpPost("DeleteAdmin", Name = "DeleteAdmin")]
        public async Task<ActionResult<APIResponse>> DeleteAdmin(EditAdminDTO editAdminDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(editAdminDTO.Email);
                await _userManager.RemoveClaimAsync(user, new Claim("IsAdmin", "1"));
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = NoContent();
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

        private async Task<AuthenticationResponse> TokenSetup(UserCredential userCredential) // APIResponse sólo va cuando es un método http expuesto (no un método local)
        {
            // Claim = información encriptada pero que no sea sensible (no usar tarjetas de crédito, por ej.)
            var claims = new List<Claim>()
            {
                new Claim("Email" , userCredential.Email),

            };

            // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27047714#notes
            var user = await _userManager.FindByEmailAsync(userCredential.Email);
            var claimsDB = await _userManager.GetClaimsAsync(user);
            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["KeyJwt"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiration, signingCredentials: credentials);
            return new AuthenticationResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiration = expiration
            };
        }

        #endregion

    }
}