using System;
using ApiEcommerce.Api.Data;
using ApiEcommerce.Api.Models;
using ApiEcommerce.Api.Models.Dtos;
using ApiEcommerce.Api.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ApiEcommerce.Api.Repository;

public class UserRepository : IUserRepository
{
	public readonly ApplicationDbContext _db;
	private string? secretKey;

	public UserRepository(ApplicationDbContext db, IConfiguration configuration)
	{
		_db = db;
		secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
	}

	public User? GetUser(int id)
	{
		return _db.Users.FirstOrDefault(u => u.Id == id);
	}

	public ICollection<User> GetUsers()
	{
		return _db.Users.OrderBy(u => u.Username).ToList();
	}

	public bool IsUniqueUser(string username)
	{
		return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
	}

	public async Task<UserLoginResponseDto?> Login(UserLoginDto userLoginDto)
	{
		/*
			Check "Username" isn't empty.
			If it is, it returns null.
		*/
		if (string.IsNullOrEmpty(userLoginDto.Username))
		{
			return null;
		}

		/*
			Looks up the user in the DB with FirstOrDefaultAsync.
			The comparison ignores case and extra spaces (ToLower().Trim()).
			If not user matches, it returns null.
		*/
		var user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
		if (user == null)
		{
			return null;
		}

		/*
			Check the password with BCrypt.Verify.
			The DB never stores the real password, only a hash of it.
			BCrypt hashes the password the user typed and checks whether it matches the stored one.
			it returns null.
		*/
		if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
		{
			return null;
		}

		/*
			JWT
			Builds a JWT (JSON Web Token), which is the user's "pass" for later requests:
			- Claims are the data stored inside the token: id, username, and role.
			- Expires is set 2 hours from now.
			- SigningCredentials signs the token with your SecretKey (from ApiSettings:SecretKey in config) using HMAC-SHA256.
			  If anyone changes the token, the signature no longer matches and the server rejects it.
		*/
		var handlerToken = new JwtSecurityTokenHandler();
		if (string.IsNullOrWhiteSpace(secretKey))
		{
			throw new InvalidOperationException("SecretKey won't configure");
		}

		var key = Encoding.UTF8.GetBytes(secretKey);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
				new Claim("id",user.Id.ToString()),
				new Claim("username",user.Username),
				new Claim(ClaimTypes.Role,user.Role ?? string.Empty),
			}
			),
			Expires = DateTime.UtcNow.AddHours(2),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		// Returns the token as a string, some user info and a success message.
		var token = handlerToken.CreateToken(tokenDescriptor);
		return new UserLoginResponseDto()
		{
			Token = handlerToken.WriteToken(token),
			User = new UserDto()
			{
				Id = user.Id,
				Username = user.Username,
				Name = user.Name,
				Role = user.Role,
			},
			Message = "User login correctly"
		};
	}

	public async Task<User> Register(CreateUserDto createUserDto)
	{
		var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
		var user = new User()
		{
			Username = createUserDto.Username ?? "No Username",
			Name = createUserDto.Name,
			Role = "User",
			Password = encriptedPassword
		};

		_db.Users.Add(user);
		await _db.SaveChangesAsync();
		return user;
	}
}
