using e_commerce.Model;
using e_commerce.Model.Dtos;

namespace e_commerce.Repository.IRepository;

public interface IUserRepository
{
    ICollection<ApplicationUser> GetAllUsers();
    // ICollection<User> GetAllUsers();
    ApplicationUser? GetUser(string id);
    //User? GetUser(int id);
    bool IsUniqueUser(string username);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    // Task<User> Register(CreateUserDto createEUserDto);
    Task<UserDataDto> Register(CreateUserDto createEUserDto);
}