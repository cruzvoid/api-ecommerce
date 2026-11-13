using System;

namespace ApiEcommerce.Api.Models.Dtos;

public class UserLoginResponseDto
{
	public UserDto? User { get; set; }
	public string? Token { get; set; }
	public string? Message { get; set; }
}
