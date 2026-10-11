using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Auth;
using SchoolERP.Infrastructure.Data;

namespace SchoolERP.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly SchoolErpDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(SchoolErpDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public record LoginRequest(string Email, string Password);

    public record LoginResponse(string Token, string Role, string DisplayName, Guid TenantId);

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _db.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized();

        return Ok(new LoginResponse(_jwt.CreateToken(user), user.Role, user.DisplayName, user.TenantId));
    }
}
