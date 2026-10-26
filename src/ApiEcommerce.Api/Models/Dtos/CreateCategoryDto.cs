using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Api.Models.Dtos;

public class CreateCategoryDto
{
 	[Required(ErrorMessage = "Name is required")]
	[MaxLength(50, ErrorMessage = "Name: max length 50.")]
	[MinLength(3, ErrorMessage = "Name: min length 3.")]
	public string Name { get; set; } = string.Empty;
}
