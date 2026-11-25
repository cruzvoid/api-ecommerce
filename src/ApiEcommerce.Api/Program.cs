using System.Text;
using ApiEcommerce.Api.Constants;
using ApiEcommerce.Api.Data;
using ApiEcommerce.Api.Repository;
using ApiEcommerce.Api.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey))
{
	throw new InvalidOperationException("SecretKey won't configure");
}

/*
	Registering the authentication service
	- A "scheme" is a way of identifying users: cookies, JWT, Google and so on.
	- This says: "By default, identify people with JWT Bearer tokens."
*/
builder.Services.AddAuthentication(options =>
{
	// Authenticate Scheme : how to read who the user is from the request
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	// Challenge Scheme: what to do when someone isn't logged in. For JWT, the answer is 401 Unauthorized.
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => // Configuring the JWT checks
{
	options.RequireHttpsMetadata = false; // allow plain http (fine for local dev only)
	options.SaveToken = true; // keep the raw token available later in the request
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true, // Check the signature
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
		ValidateIssuer = false, // "Issuer" don't check who created the token (for example "ApiEcommerce")
		ValidateAudience = false // "Audience don't check who the token is meant for (for example "ApiEcommerce-clients")
	};
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
	options.AddDocumentTransformer((document, context, cancellationToken) =>
	{
		// 1) Define the security scheme: "this API accepts a JWT in the Authorization header"
		document.Components ??= new OpenApiComponents();
		document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
		{
			Type = SecuritySchemeType.Http, // the standard HTTP Authorization header
			Scheme = "bearer",              // Swagger UI adds "Bearer " before the token for you
			BearerFormat = "JWT",
			Description = "Paste the token you get from POST /api/users/Login"
		};

		// 2) Apply that scheme to every operation (this is what makes Swagger UI send the header)
		document.SecurityRequirements.Add(new OpenApiSecurityRequirement
		{
			[new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
			}] = Array.Empty<string>()
		});

		return Task.CompletedTask;
	});
});

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
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/openapi/v1.json", "ApiEcommerce v1");
		options.EnablePersistAuthorization();
	});
}

app.UseHttpsRedirection();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
