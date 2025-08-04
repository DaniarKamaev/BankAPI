using BankAPI.Features.Accounts.Create;
using BankAPI.Features.Accounts.Deposit;
using BankAPI.Features.Accounts.GetAccounts;
using BankAPI.Features.Accounts.Transfer;
using BankAPI.Shared;
using FluentValidation;
using MediatR;
using SharpGrip.FluentValidation.AutoValidation.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountValidator>();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddSingleton<BankDbContext>();

var app = builder.Build();

app.UseAuthorization();
app.MapCreateAccountEndpoint();
app.MapGetAccountsEndpoint();
app.MapDepositEndpoint();
app.MapTransferEndpoint();

app.Run();