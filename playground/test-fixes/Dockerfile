FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency resolution
COPY ["src/Api/TestService.Api.csproj", "src/Api/"]
COPY ["src/Application/TestService.Application.csproj", "src/Application/"]
COPY ["src/Domain/TestService.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/TestService.Infrastructure.csproj", "src/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Api/TestService.Api.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/Api"
RUN dotnet build "TestService.Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Api"
RUN dotnet publish "TestService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestService.Api.dll"]