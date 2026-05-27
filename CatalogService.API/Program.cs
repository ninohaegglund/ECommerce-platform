using System.Text.Json.Serialization;
using CatalogService.Api.Data;
using CatalogService.Api.Interfaces;
using CatalogService.Api.Repositories;
using CatalogService.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedFrontendOrigins = new[]
{
    "http://localhost:5173",
    "https://localhost:5173",
    "https://spelvalvet.shop",
    "https://www.spelvalvet.shop"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedFrontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");

var configuredPath = app.Configuration.GetValue<string>("ProductImages:UploadPath");
var uploadRoot = string.IsNullOrWhiteSpace(configuredPath)
    ? Path.Combine(app.Environment.ContentRootPath, "Uploads", "Products")
    : configuredPath;

Directory.CreateDirectory(uploadRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads/products"
});

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();
