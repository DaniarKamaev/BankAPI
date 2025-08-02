using BankAPI;
using BankAPI.Models.BankAPI.Models;
using FluentValidation;
//using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IValidator<Account>, AccountValidator>();
//builder.Services.AddFluentValidationAutoValidation();
var app = builder.Build();



app.UseAuthorization();
app.MapControllers();
app.Run();