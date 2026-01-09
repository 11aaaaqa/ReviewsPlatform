using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Services.Password_services;
using AccountMicroservice.Api.Services.Roles_services;
using AccountMicroservice.Api.Services.Token_services;
using AccountMicroservice.Api.Services.User_services;
using AccountMicroservice.Api.Services.User_services.Avatar_services;
using AccountMicroservice.Api.Services.User_services.Role_services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(x => 
    x.UseNpgsql(builder.Configuration["Database:ConnectionString"]));

builder.Services.AddTransient<IPasswordService, PasswordService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IUserRolesService, UserRolesService>();
builder.Services.AddTransient<IRoleService, RoleService>();
builder.Services.AddTransient<IAvatarService, AvatarService>();

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
