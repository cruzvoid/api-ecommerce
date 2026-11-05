using System.Drawing;
using ApiEcommerce.Api.Models;
using ApiEcommerce.Api.Models.Dtos;
using ApiEcommerce.Api.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
	{
		private readonly IProductRepository _productRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly IMapper _mapper;

		public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult GetProducts()
		{
			var products = _productRepository.GetProducts();
			var productsDto = _mapper.Map<List<ProductDto>>(products);
			return Ok(productsDto);
		}

		[HttpGet("{productId:int}", Name = "GetProduct")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult GetProduct(int productId)
		{
			var product = _productRepository.GetProduct(productId);

			if (product == null)
			{
				return NotFound($"The product with id {productId} doesn't exist");
			}

			var ProductDto = _mapper.Map<ProductDto>(product);
			return Ok(ProductDto);
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
		{
			if (createProductDto == null)
			{
				return BadRequest(ModelState);
			}

			if (_productRepository.ProductExists(createProductDto.Name))
			{
				ModelState.AddModelError("CustomError", "The product already exist");
				return BadRequest(ModelState);
			}

			if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
			{
				ModelState.AddModelError("CustomError", $"The category with id {createProductDto.CategoryId} doesn't exist");
				return BadRequest(ModelState);
			}

			var product = _mapper.Map<Product>(createProductDto);
			if (!_productRepository.CreateProduct(product))
			{
				ModelState.AddModelError("CustomError", $"Something went wrong saving {product.Name}");
				return StatusCode(500, ModelState);
			}

			var createdProduct = _productRepository.GetProduct(product.ProductId);
			var productDto = _mapper.Map<ProductDto>(createdProduct);

			return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
		}

		[HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductsForCategory")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult GetProductsForCategory(int categoryId)
		{
			var products = _productRepository.GetProductsForCategory(categoryId);
			if (products.Count == 0)
			{
				return NotFound($"Los productos con la categoría {categoryId} no existen");
			}

			var productsDto = _mapper.Map<List<ProductDto>>(products);
			return Ok(productsDto);
		}

		[HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult SearchProducts(string searchTerm)
		{
			var products = _productRepository.SearchProducts(searchTerm);
			if (products.Count == 0)
			{
				return NotFound($"The products with that description or name '{searchTerm}' don't exist");
			}

			var productsDto = _mapper.Map<List<ProductDto>>(products);
			return Ok(productsDto);
		}

		[HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult BuyProduct(string name, int quantity)
		{
			if (string.IsNullOrWhiteSpace(name) || quantity <= 0)
			{
				return BadRequest("Name or quantity is not valid");
			}

			var foundProduct = _productRepository.ProductExists(name);
			if (!foundProduct)
			{
				return NotFound($"The product {name} doesn't exist");
			}

			if (!_productRepository.BuyProduct(name, quantity))
			{
				ModelState.AddModelError("CustomError", $"Something went wrong while purchasing '{name}', or the requested quantity exceeds the available stock");
				return BadRequest(ModelState);
			}

			var units = quantity == 1 ? "unity" : "units";
			return Ok($"You bought {quantity} {units} of product '{name}'");
		}

		[HttpPut("{productId:int}", Name = "UpdateProduct")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
		{
		if (updateProductDto == null)
		{
			return BadRequest(ModelState);
		}
		if (!_productRepository.ProductExists(productId))
		{
			ModelState.AddModelError("CustomError", "The product doesn't exist");
			return BadRequest(ModelState);
		}
		if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
		{
			ModelState.AddModelError("CustomError", $"The category with id {updateProductDto.CategoryId} doesn't exist");
			return BadRequest(ModelState);
		}
		var product = _mapper.Map<Product>(updateProductDto);
		product.ProductId = productId;
		if (!_productRepository.UpdateProduct(product))
		{
			ModelState.AddModelError("CustomError", $"Somethig went wrong while updating {product.Name}");
			return StatusCode(500, ModelState);
		}
		return NoContent();
		}

		[HttpDelete("{productId:int}", Name = "DeleteProduct")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IActionResult DeleteProduct(int productId)
		{
		if (productId == 0)
		{
			return BadRequest(ModelState);
		}

		var product = _productRepository.GetProduct(productId);
		if (product == null)
		{
			return NotFound($"The product with id {productId} doesn't exist");
		}
		if (!_productRepository.DeleteProduct(product))
		{
			ModelState.AddModelError("CustomError", $"Something went wrong while deleting {product.Name}");
			return StatusCode(500, ModelState);
		}
		return NoContent();
		}
	}
}
