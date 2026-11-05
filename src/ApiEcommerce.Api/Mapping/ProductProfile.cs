using System;
using ApiEcommerce.Api.Models;
using ApiEcommerce.Api.Models.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using AutoMapper;

namespace ApiEcommerce.Api.Mapping;

public class ProductProfile : Profile
{
	public ProductProfile()
	{
		CreateMap<Product, ProductDto>().ReverseMap();
		CreateMap<Product, CreateProductDto>().ReverseMap();
		CreateMap<Product, UpdateProductDto>().ReverseMap();
	}
}
