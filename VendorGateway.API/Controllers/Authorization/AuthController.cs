using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorGateway.Application.Dtos.Authentication;
using VendorGateway.Application.Interfaces;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.API.Controllers.Authorization
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginUserRequest request)
        {
            var token = await authService.LoginAsync(request);

            if (token == null)
                return Unauthorized();

            return Ok(new
            {
                token
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterUserRequest request)
        {
            var success = await authService.RegisterAsync(request);
            if (success)
                return StatusCode(201);

            return StatusCode(500);
        }
    }
}
