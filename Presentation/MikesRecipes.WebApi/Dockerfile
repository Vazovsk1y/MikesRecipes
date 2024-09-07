FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5555

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Presentation/MikesRecipes.WebApi/MikesRecipes.WebApi.csproj", "Presentation/MikesRecipes.WebApi/"]
COPY ["Infrastructure/MikesRecipes.Application.Implementation/MikesRecipes.Application.Implementation.csproj", "Infrastructure/MikesRecipes.Application.Implementation/"]
COPY ["Application/MikesRecipes.Application/MikesRecipes.Application.csproj", "Application/MikesRecipes.Application/"]
COPY ["Core/MikesRecipes.Domain/MikesRecipes.Domain.csproj", "Core/MikesRecipes.Domain/"]
COPY ["Application/MikesRecipes.Auth/MikesRecipes.Auth.csproj", "Application/MikesRecipes.Auth/"]
COPY ["Infrastructure/MikesRecipes.DAL.PostgreSQL/MikesRecipes.DAL.PostgreSQL.csproj", "Infrastructure/MikesRecipes.DAL.PostgreSQL/"]
COPY ["Infrastructure/MikesRecipes.Framework/MikesRecipes.Framework.csproj", "Infrastructure/MikesRecipes.Framework/"]
COPY ["Infrastructure/MikesRecipes.Auth.Implementation/MikesRecipes.Auth.Implementation.csproj", "Infrastructure/MikesRecipes.Auth.Implementation/"]
RUN dotnet restore "Presentation/MikesRecipes.WebApi/MikesRecipes.WebApi.csproj"
COPY . .
WORKDIR "/src/Presentation/MikesRecipes.WebApi"
RUN dotnet build "MikesRecipes.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MikesRecipes.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MikesRecipes.WebApi.dll"]