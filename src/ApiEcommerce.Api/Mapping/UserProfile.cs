using System;
using ApiEcommerce.Api.Models;
using ApiEcommerce.Api.Models.Dtos;
using AutoMapper;

namespace ApiEcommerce.Api.Mapping;

public class UserProfile : Profile
{
	public UserProfile()
	{
		CreateMap<User, UserDto>().ReverseMap();
		CreateMap<User, CreateUserDto>().ReverseMap();
		CreateMap<User, UserLoginResponseDto>().ReverseMap();
		CreateMap<User, UserRegisterDto>().ReverseMap();
	}
}
