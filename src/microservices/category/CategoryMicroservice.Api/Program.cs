using CategoryMicroservice.Api.Database;
using CategoryMicroservice.Api.MessageBus.Consumers;
using CategoryMicroservice.Api.Models.Business;
using CategoryMicroservice.Api.Services;
using CategoryMicroservice.Api.Services.CategoryServices;
using CategoryMicroservice.Api.Services.ItemServices;
using CategoryMicroservice.Api.Services.UnitOfWork;
using MessageBus.Extensions;
using MessageBus.Messages.Review;
using MessageBus.Messages.Saga.CreateItemWIthReview;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMqMessageBus.Extensions;
using RestrictionGrpcService;
using System.Text;

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
    x.UseNpgsql(builder.Configuration["Database:ConnectionString"]!));

builder.Services.AddScoped<ICategoryRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<ICategoryRepository<Subcategory>, SubcategoryRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<ImageValidator>();

builder.Services.AddRabbitMqMessageBus(new RabbitMqOptions
    {
        UserName = builder.Configuration["RABBITMQ_DEFAULT_USER"]!,
        Password = builder.Configuration["RABBITMQ_DEFAULT_PASS"]!,
        HostName = builder.Configuration["RABBITMQ_HOSTNAME"]!,
        VirtualHost = builder.Configuration["RABBITMQ_DEFAULT_VHOST"]!,
        QueueName = "CategoryMicroservice"
    }).AddMessageBusHandler<ReviewFailedToCreateSagaEvent, ReviewFailedToCreateSagaEventConsumer>()
    .AddMessageBusHandler<ReviewCreatedSagaEvent, ReviewCreatedSagaEventConsumer>()
    .AddMessageBusHandler<ReviewRemovedEvent, ReviewRemovedEventConsumer>()
    .AddMessageBusHandler<ReviewAcceptedEvent, ReviewAcceptedEventConsumer>()
    .AddMessageBusHandler<ReviewCreatedWithItemRejectedEvent, ReviewCreatedWithItemRejectedEventConsumer>();

builder.Services.AddGrpcClient<RestrictionInfo.RestrictionInfoClient>(x =>
{
    x.Address = new Uri(builder.Configuration["Url:RestrictionMicroservice:Grpc"]!);
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
