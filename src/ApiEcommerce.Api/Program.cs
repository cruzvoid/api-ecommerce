using ApiEcommerce.Api.Constants;
using ApiEcommerce.Api.Data;
using ApiEcommerce.Api.Repository;
using ApiEcommerce.Api.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy(PolicyNames.AllowSpecificOrigin,
		policy =>
		{
			policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
		}
	);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "ApiEcommerce v1"));
}

app.UseHttpsRedirection();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseAuthorization();

app.MapControllers();

app.Run();
