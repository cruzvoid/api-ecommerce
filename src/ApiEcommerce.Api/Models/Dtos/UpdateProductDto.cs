using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Api.Models.Dtos;

public class UpdateProductDto
{
	[Required(ErrorMessage = "Name can't be null or empty")]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
	public decimal Price { get; set; }
	public string ImgUrl { get; set; } = string.Empty;
	[Required(ErrorMessage = "SKU can't be null or empty")]
	public string SKU { get; set; } = string.Empty;
	[Range(0, int.MaxValue)]
	public int Stock { get; set; }
	// public DateTime? UpdateDate { get; set; } = null;
	[Range(1,int.MaxValue)]
	public int CategoryId { get; set; }
}