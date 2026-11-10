using System;
using ApiEcommerce.Api.Data;
using ApiEcommerce.Api.Models;
using ApiEcommerce.Api.Models.Dtos;
using ApiEcommerce.Api.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Api.Repository;

public class UserRepository(ApplicationDbContext db, IConfiguration configuration) : IUserRepository
{
	public readonly ApplicationDbContext _db = db;
	private string? secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");

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

	public Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
	{
		throw new NotImplementedException();
	}

	public Task<User> Register(CreateUserDto createUserDto)
	{
		throw new NotImplementedException();
	}
}
