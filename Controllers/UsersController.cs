using AutoMapper;
using e_commerce.Model.Dtos;
using e_commerce.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce.Controllers
{
    
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _UserRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _UserRepository = userRepository;
            _mapper = mapper;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var users = _UserRepository.GetAllUsers();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            
            return Ok(userDtos);
        }

        [HttpGet("{id:int}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser(int id)
        {
            var user = _UserRepository.GetUser(id);

            if (user == null) return NotFound($"El usuario con el id {id} no existe");

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost(Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null || !ModelState.IsValid) return BadRequest(ModelState);
            
            if (string.IsNullOrWhiteSpace(createUserDto.Username)) return BadRequest("Username es requerido");
            
            if (!_UserRepository.IsUniqueUser(createUserDto.Username)) return BadRequest("El usuario ya existe");
            
            var result = await _UserRepository.Register(createUserDto);
            
            if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el usuario");
            
            return CreatedAtRoute("GetUser", new { id = result.Id }, result);
        }

        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null || !ModelState.IsValid) return BadRequest(ModelState);
            
            var user = await _UserRepository.Login(userLoginDto);
            
            if (user == null) return Unauthorized();
            
            return Ok(user);
        }
    }
}
