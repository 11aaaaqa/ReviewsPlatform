using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RabbitMqMessageBus.Extensions;
using System.Text;
using MessageBus.Extensions;
using MessageBus.Messages.Item;
using MessageBus.Messages.Saga.CreateItemWIthReview;
using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Database;
using ReviewMicroservice.Api.MessageBus.Consumers;
using ReviewMicroservice.Api.Services.ReviewServices;
using ReviewMicroservice.Api.Services.UnitOfWork;

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

builder.Services.AddRabbitMqMessageBus(new RabbitMqOptions
    {
        UserName = builder.Configuration["RABBITMQ_DEFAULT_USER"]!,
        Password = builder.Configuration["RABBITMQ_DEFAULT_PASS"]!,
        HostName = builder.Configuration["RABBITMQ_HOSTNAME"]!,
        VirtualHost = builder.Configuration["RABBITMQ_DEFAULT_VHOST"]!,
        QueueName = "ReviewMicroservice"
    }).AddMessageBusHandler<ItemCreatedSagaEvent, ItemCreatedSagaEventConsumer>()
    .AddMessageBusHandler<ItemRemovedEvent, ItemRemovedEventConsumer>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
