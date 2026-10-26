using System;
using ApiEcommerce.Api.Models;
using ApiEcommerce.Api.Models.Dtos;
using AutoMapper;

namespace ApiEcommerce.Api.Mapping;

public class CategoryProfile : Profile
{
	public CategoryProfile()
	{
		CreateMap<Category, CategoryDto>().ReverseMap();
		CreateMap<Category, CreateCategoryDto>().ReverseMap();
	}
}
