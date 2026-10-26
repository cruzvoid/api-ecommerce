using ApiEcommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Api.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{

	}

	public DbSet<Category> Categories { get; set; }
}