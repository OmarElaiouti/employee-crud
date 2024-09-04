
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using crud.Core.Enums;
using crud.Infrastructure.Dtos.AuthDtos;
using crud.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Delivery.Api.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthController(AuthService authService, ILogger<AuthController> logger, IHttpContextAccessor httpContext)
        {
            _authService = authService;
            _logger = logger;
            _httpContextAccessor = httpContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var registrationResult = await _authService.Register(model);

                return registrationResult.Status switch
                {
                    RegistrationStatus.Success => Ok(new { message = "User registered successfully." }),
                    RegistrationStatus.UserAlreadyExists => Conflict(new { error = "User already exists." }),
                    RegistrationStatus.PasswordValidationFailed => BadRequest(new { error = "Password validation failed." }),
                    RegistrationStatus.OtherError => StatusCode(500, new { error = "An error occurred during registration." }),
                    _ => StatusCode(500, new { error = "An unexpected error occurred." })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during registration.");
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Invalid email or password." });
                }

                LoginResultDto? token = await _authService.Login(model, _httpContextAccessor.HttpContext);
                if (token == null)
                {
                    _logger.LogWarning("Invalid login attempt: Invalid token.");
                    return BadRequest(new { error = "Invalid email or password." });
                }
  
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login.");
                return StatusCode(500, new { error = "An unexpected error occurred during login." });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            try
            {
                var newAccessToken = await _authService.RefreshToken(_httpContextAccessor.HttpContext,refreshToken);
                if (newAccessToken == null)
                {
                    _logger.LogWarning("Invalid refresh token attempt: Invalid token.");
                    return Unauthorized(new { error = "Invalid email or password." });
                }

                _logger.LogInformation("User logged in successfully.");
                return Ok(newAccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error occurred during refreshing token.");
                return StatusCode(500, new { error = "An Error occurred during refreshing token." });
            }
        }



    }

}