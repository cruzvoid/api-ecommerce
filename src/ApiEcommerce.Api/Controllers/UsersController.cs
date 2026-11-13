using ApiEcommerce.Api.Models.Dtos;
using ApiEcommerce.Api.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;

		public UsersController(IUserRepository userRepository, IMapper mapper)
		{
			_userRepository = userRepository;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult GetUsers()
		{
			var users = _userRepository.GetUsers();
			var usersDto = _mapper.Map<List<UserDto>>(users);
			return Ok(usersDto);
		}

		[HttpGet("{id:int}", Name = "GetUser")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult GetUser(int id)
		{
			if (id <= 0)
			{
				return BadRequest("The user id must be greater than 0");
			}

			var user = _userRepository.GetUser(id);
			if (user == null)
			{
				return NotFound($"The user with id {id} doesn't exist");
			}

			var userDto = _mapper.Map<UserDto>(user);
			return Ok(userDto);
		}

		[HttpPost(Name = "RegisterUser")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
		{
			if (createUserDto == null || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (string.IsNullOrWhiteSpace(createUserDto.Username))
			{
				return BadRequest("Username es requerido");
			}

			if (!_userRepository.IsUniqueUser(createUserDto.Username))
			{
				return BadRequest("El usuario ya existe");
			}

			var result = await _userRepository.Register(createUserDto);

			if (result == null)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario");
			}

			var userDto = _mapper.Map<UserDto>(result);
			return CreatedAtRoute("GetUser", new { id = result.Id }, userDto);
		}

		[HttpPost("Login", Name = "LoginUser")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
		{
			if (userLoginDto == null || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _userRepository.Login(userLoginDto);
			if (user == null)
			{
				return Unauthorized();
			}
			return Ok(user);
		}
	}
}
