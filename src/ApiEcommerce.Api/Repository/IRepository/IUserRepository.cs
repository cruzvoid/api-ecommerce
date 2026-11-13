using System;
using ApiEcommerce.Api.Models;
using ApiEcommerce.Api.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace ApiEcommerce.Api.Repository.IRepository;

public interface IUserRepository
{
	ICollection<User> GetUsers();
	User? GetUser(int id);
	bool IsUniqueUser(string name);
	Task<UserLoginResponseDto?> Login(UserLoginDto userLoginDto);
	Task<User> Register(CreateUserDto createUserDto);
}
