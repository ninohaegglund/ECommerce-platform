using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NotificationService.Api.Data;
using NotificationService.Api.Interfaces;
using NotificationService.Api.JWT;
using NotificationService.Api.Options;
using NotificationService.Api.Repositories;
using NotificationService.Api.Services;
using NotificationServiceImplementation = NotificationService.Api.Services.NotificationService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token from IdentityService."
    });
});
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

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

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationServiceImplementation>();
builder.Services.AddScoped<INewsletterRepository, NewsletterRepository>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection(ResendOptions.SectionName));
builder.Services.AddHttpClient<IEmailSender, ResendEmailSender>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
