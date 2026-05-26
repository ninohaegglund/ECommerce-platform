using IdentityService.API.Data;
using IdentityService.API.Interfaces;
using IdentityService.API.JWT;
using IdentityService.API.Repositories;
using IdentityService.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<INotificationClient, NotificationClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var notificationServiceUrl = configuration["Services:NotificationServiceUrl"];
    if (!string.IsNullOrWhiteSpace(notificationServiceUrl))
    {
        client.BaseAddress = new Uri(notificationServiceUrl.TrimEnd('/') + "/");
    }
});

builder.Services.AddJwtAuthentication(builder.Configuration);

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
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedFrontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext);
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
