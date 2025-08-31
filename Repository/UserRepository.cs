using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using e_commerce.Data;
using e_commerce.Model;
using e_commerce.Model.Dtos;
using e_commerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace e_commerce.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private string? secreteKey;

    public UserRepository(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        secreteKey = config.GetValue<string>("ApiSettings:SecretKey");
    }

    public ICollection<User> GetAllUsers()
    {
        return _db.Users.OrderBy(u => u.Username).ToList();
    }

    public User? GetUser(int id)
    {
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Username))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "El username es requeido"
            };
        }
        
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "El username no fue encontrado"
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Las credenciales son incorrectas"
            };
        }
        //JWT
        var handlerToken = new JwtSecurityTokenHandler();
        
        if (string.IsNullOrWhiteSpace(secreteKey)) throw new InvalidOperationException("Secretekey no esta configurada");
        
        var key = Encoding.UTF8.GetBytes(secreteKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Rol ?? String.Empty)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = handlerToken.CreateToken(tokenDescriptor);
        
        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = new UserRegisterDto()
            {
                Username = user.Username,
                Name = user.Name,
                Rol = user.Rol,
                Password = user.Password ?? "",
            },
            Message = $"El usuario a iniciado sesion correctamente"
        };
    }

    public async Task<User> Register(CreateUserDto createEUserDto)
    {
        var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(createEUserDto.Password);

        var user = new User()
        {
            Username = createEUserDto.Username ?? "No Username",
            Password = encryptedPassword,
            Name = createEUserDto.Name,
            Rol = createEUserDto.Role,
        };
        
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}