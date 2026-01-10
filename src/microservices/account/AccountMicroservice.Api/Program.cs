using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Services.Password_services;
using AccountMicroservice.Api.Services.Roles_services;
using AccountMicroservice.Api.Services.Token_services;
using AccountMicroservice.Api.Services.UnitOfWork;
using AccountMicroservice.Api.Services.User_services;
using AccountMicroservice.Api.Services.User_services.Avatar_services;
using AccountMicroservice.Api.Services.User_services.Role_services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(x => 
    x.UseNpgsql(builder.Configuration["Database:ConnectionString"]));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRolesService, UserRolesService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IAvatarService, AvatarService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
