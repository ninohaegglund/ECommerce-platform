using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Repositories;
using PaymentServiceImplementation = PaymentService.Api.Services.PaymentService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentServiceImplementation>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
