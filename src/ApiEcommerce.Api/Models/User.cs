using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace ApiEcommerce.Api.Models;

public class User
{
	[Key]
	public int Id { get; set; }
	public string? Name { get; set; }
	public string Username { get; set; } = string.Empty;
	public string? Password { get; set; }
	public string? Role { get; set; }
}
