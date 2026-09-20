using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorGateway.API.Extensions;
using VendorGateway.Application.Dtos.Authentication;
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
            var result = await authService.LoginAsync(request);
            return result.ToActionResult(token => new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
        {
            var result = await authService.RegisterAsync(request, ct);
            return result.IsSuccess ? StatusCode(201) : result.ToActionResult();
        }
    }
}
