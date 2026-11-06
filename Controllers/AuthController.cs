using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DbContextType = pract_8.Models.Pract8Context;
using UserEntity = pract_8.Models.User;

namespace pract_8.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly DbContextType _db;
    private readonly IPasswordHasher<UserEntity> _hasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        DbContextType db,
        IPasswordHasher<UserEntity> hasher,
        IConfiguration configuration)
    {
        _db = db;
        _hasher = hasher;
        _configuration = configuration;
    }

    public record AuthRequest(string Login, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Login == request.Login))
            return Conflict("Логин уже существует");

        var user = new UserEntity { Login = request.Login, Role = "User" };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok("Пользователь зарегистрирован");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
        if (user is null)
            return Unauthorized("Неверная пара логин/пароль");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Неверная пара логин/пароль");

        return Ok(new { access_token = GenerateJwtToken(user) });
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "ExampleApi",
            audience: "ExampleApiClient",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}