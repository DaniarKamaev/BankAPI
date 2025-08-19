using BankAPI.Features.Accounts.Create;
using BankAPI.Features.Accounts.Deposit;
using BankAPI.Features.Accounts.GetAccounts;
using BankAPI.Features.Accounts.Interest;
using BankAPI.Features.Accounts.Transfer;
using BankAPI.Features.Authentication;
using BankAPI.Shared;
using BankAPI.Shared.Behaviors;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection String: {connectionString ?? "ConnectionString is null"}");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("ConnectionString 'DefaultConnection' is not configured.");
}

var jwtKey = builder.Configuration["Jwt:Key"];
Console.WriteLine($"JWT Key: {jwtKey ?? "JWT Key is null"}");
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured.");
}
var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);
Console.WriteLine($"JWT Key length: {jwtKeyBytes.Length} bytes");
if (jwtKeyBytes.Length < 64)
{
    throw new InvalidOperationException("JWT Key must be at least 64 bytes long for HMAC-SHA512.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IInterestAccrualService, InterestAccrualService>();

builder.Services.AddTransient<IRequestHandler<TransferRequest, TransferResponse>, TransferHandler>();
builder.Services.AddSingleton<RabbitMqService>();

builder.Services.AddLogging(logging => logging.AddConsole());


var app = builder.Build();

var rabbitService = app.Services.GetRequiredService<RabbitMqService>();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exception != null)
        {
            await context.Response.WriteAsJsonAsync(new { Error = exception.Error.Message });
        }
    });
});

using (var scope = app.Services.CreateScope())
{
    try
    {
        var RabbitService = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
        Console.WriteLine("RabbitMQ подключен и очереди созданы");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка подключения к RabbitMQ: {ex.Message}");
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCreateAccountEndpoint();
app.MapGetAccountsEndpoint();
app.MapDepositEndpoint();
app.MapTransferEndpoint();
app.MapLoginEndpoint();


app.Run();