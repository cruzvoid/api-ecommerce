using ApiEcommerce.Api.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiEcommerce.Api.Models.Dtos;
using ApiEcommerce.Api.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace ApiEcommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
	{
		private readonly ICategoryRepository _categoryRepository;
		private readonly IMapper _mapper;

		public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
		{
			_categoryRepository = categoryRepository;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]

		public IActionResult GetCategories()
		{
			var categories = _categoryRepository.GetCategories();
			var categoriesDto = new List<CategoryDto>();
			foreach (var category in categories)
			{
				categoriesDto.Add(_mapper.Map<CategoryDto>(category));
			}
			return Ok(categoriesDto);
		}

		[HttpGet("{id:int}", Name = "GetCategory")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]

		public IActionResult GetCategory(int id)
		{
			var category = _categoryRepository.GetCategory(id);

			if (category == null)
			{
				return NotFound($"Category: {id}, not found.");
			}

			var categoryDto = _mapper.Map<CategoryDto>(category);

			return Ok(categoryDto);
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]

		public IActionResult CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
		{
			if (createCategoryDto == null)
			{
				return BadRequest(ModelState);
			}

			if (_categoryRepository.CategoryExists(createCategoryDto.Name))
			{
				ModelState.AddModelError("CustomError", "The category already exists.");
				return BadRequest(ModelState); // Prevents an insert before an issue.
			}

			var category = _mapper.Map<Category>(createCategoryDto);

			if (!_categoryRepository.CreateCategory(category))
			{
				ModelState.AddModelError("CustomError",$"Something went wrong saving {category.Name}");
				return StatusCode(StatusCodes.Status500InternalServerError, ModelState); // Prevents an insert before an issue.
			}

			return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
		}


		[HttpPatch("{id:int}", Name = "UpdateCategory")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]

		public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
		{
			if (updateCategoryDto == null)
			{
				return BadRequest(ModelState);
			}

			if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
			{
				ModelState.AddModelError("CustomError", "The category already exist");
				return BadRequest(ModelState);
			}

			var category = _categoryRepository.GetCategory(id);

			if (category == null)
			{
				return NotFound($"The category with ID:{id} doesn't exist");
			}

			category.Name = updateCategoryDto.Name;

			if (!_categoryRepository.UpdateCategory(category))
			{
				ModelState.AddModelError("CustomError", $"Something went wrong updating {category.Name}");
				return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
			}

			return NoContent();
		}

		[HttpDelete("{id:int}", Name = "DeleteCategory")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]

		public IActionResult DeleteCategory(int id)
		{
			if (!_categoryRepository.CategoryExists(id))
			{
				return NotFound($"The category: {id} doesn't exist");
			}

			var category = _categoryRepository.GetCategory(id);

			if (category == null)
			{
				return NotFound($"The category: {id} doesn't exist");
			}

			if (!_categoryRepository.DeleteCategory(category))
			{
				ModelState.AddModelError("CustomError", $"Something went wrong deleting with {category.Name}");
				return StatusCode(500, ModelState);
			}

			return NoContent();
		}
	}
}
