using e_commerce.Model;
using e_commerce.Model.Dtos;

namespace e_commerce.Repository.IRepository;

public interface IUserRepository
{
    ICollection<User> GetAllUsers();
    User? GetUser(int id);
    bool IsUniqueUser(string username);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<User> Register(CreateUserDto createEUserDto);
}