FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/microservices/review/ReviewMicroservice.Api/ReviewMicroservice.Api.csproj", "src/microservices/review/ReviewMicroservice.Api/"]
COPY ["src/message-bus/RabbitMqMessageBus/RabbitMqMessageBus.csproj", "src/message-bus/RabbitMqMessageBus/"]
COPY ["src/message-bus/MessageBus/MessageBus.csproj", "src/message-bus/MessageBus/"]
RUN dotnet restore "./src/microservices/review/ReviewMicroservice.Api/ReviewMicroservice.Api.csproj"
COPY . .
WORKDIR "/src/src/microservices/review/ReviewMicroservice.Api"
RUN dotnet build "./ReviewMicroservice.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ReviewMicroservice.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReviewMicroservice.Api.dll"]