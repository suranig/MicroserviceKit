FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency resolution
COPY ["src/Api/SimpleService.Api.csproj", "src/Api/"]
COPY ["src/Application/SimpleService.Application.csproj", "src/Application/"]
COPY ["src/Domain/SimpleService.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/SimpleService.Infrastructure.csproj", "src/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Api/SimpleService.Api.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/Api"
RUN dotnet build "SimpleService.Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Api"
RUN dotnet publish "SimpleService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleService.Api.dll"]