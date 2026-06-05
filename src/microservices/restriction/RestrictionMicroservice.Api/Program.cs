using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMqMessageBus.Extensions;
using RestrictionMicroservice.Api.Database;
using RestrictionMicroservice.Api.Services.ReportRepository;
using RestrictionMicroservice.Api.Services.RestrictionRepository;
using RestrictionMicroservice.Api.Services.UnitOfWork;
using System.Text;
using MessageBus.Extensions;
using MessageBus.Messages.Review;
using RestrictionMicroservice.Api.MessageBus.Consumers;
using RestrictionMicroservice.Api.Services.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]!)),
        ValidIssuer = builder.Configuration["JWT_ISSUER"],
        ValidAudience = builder.Configuration["JWT_AUDIENCE"]
    };
});

builder.Services.AddDbContext<ApplicationDbContext>(x => 
    x.UseNpgsql(builder.Configuration["Database:ConnectionString"]));

builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IRestrictionRepository, RestrictionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddRabbitMqMessageBus(new RabbitMqOptions
{
    UserName = builder.Configuration["RABBITMQ_DEFAULT_USER"]!,
    Password = builder.Configuration["RABBITMQ_DEFAULT_PASS"]!,
    HostName = builder.Configuration["RABBITMQ_HOSTNAME"]!,
    VirtualHost = builder.Configuration["RABBITMQ_DEFAULT_VHOST"]!,
    QueueName = "RestrictionMicroservice"
}).AddMessageBusHandler<ReviewRemovedEvent, ReviewRemovedEventConsumer>();

builder.Services.AddGrpc();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<RestrictionService>();

app.UseAuthorization();

app.MapControllers();

app.Run();
