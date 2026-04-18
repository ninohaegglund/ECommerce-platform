using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
