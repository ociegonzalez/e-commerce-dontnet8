using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using e_commerce.Data;
using e_commerce.Model;
using e_commerce.Model.Dtos;
using e_commerce.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace e_commerce.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private string? secreteKey;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserRepository(
        ApplicationDbContext db,
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper
    )
    {
        _db = db;
        secreteKey = config.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public ICollection<ApplicationUser> GetAllUsers()
    {
        return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
    }

    public ApplicationUser? GetUser(string id)
    {
        return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
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
        
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(
            u => u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim()
        );

        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "El username no fue encontrado"
            };
        }

        if (userLoginDto.Password == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Password requerido"
            };
        }
        
        bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

        if (!isValid)
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
        
        var roles = await _userManager.GetRolesAsync(user);
        
        var key = Encoding.UTF8.GetBytes(secreteKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? String.Empty)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = handlerToken.CreateToken(tokenDescriptor);
        
        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = _mapper.Map<UserDataDto>(user),
            Message = $"El usuario a iniciado sesion correctamente"
        };
    }

    public async Task<UserDataDto> Register(CreateUserDto createEUserDto)
    {
        if (string.IsNullOrEmpty(createEUserDto.Username)) throw new ArgumentNullException("El Username es requerido");

        if (createEUserDto.Password == null) throw new ArgumentNullException("El password es requerido");

        var user = new ApplicationUser()
        {
            UserName = createEUserDto.Username,
            Email = createEUserDto.Username,
            NormalizedEmail = createEUserDto.Username.ToUpper(),
            Name = createEUserDto.Name,
        };
        
        var result = await _userManager.CreateAsync(user, createEUserDto.Password);

        if (result.Succeeded)
        {
            var userRole = createEUserDto.Role ?? "User";
            var roleExists = await _roleManager.RoleExistsAsync(userRole);

            if (!roleExists)
            {
                var identityRole = new IdentityRole(userRole);
                await _roleManager.CreateAsync(identityRole);
            }
            await _userManager.AddToRoleAsync(user, userRole);
            var createdUser = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == createEUserDto.Username);
            return _mapper.Map<UserDataDto>(createdUser);
        }
        
        var errors = string.Join(",", result.Errors.Select(e => e.Description));
        
        throw new ApplicationException("No se puedo realizar el registro");
    }
}