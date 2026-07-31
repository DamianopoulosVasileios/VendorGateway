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
        public async Task<IActionResult> LoginAsync(LoginAccountRequest request)
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
        public async Task<IActionResult> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
        {
            var success = await authService.RegisterAsync(request, ct);
            if (success)
                return StatusCode(201);

            return Conflict($"Username '{request.Username}' is already taken.");
        }
    }
}
