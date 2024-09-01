#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5150
EXPOSE 7150

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SytsBackendGen2.Web/SytsBackendGen2.Web.csproj", "SytsBackendGen2.Web/"]
COPY ["SytsBackendGen2.Application/SytsBackendGen2.Application.csproj", "SytsBackendGen2.Application/"]
COPY ["SytsBackendGen2.Domain/SytsBackendGen2.Domain.csproj", "SytsBackendGen2.Domain/"]
COPY ["SytsBackendGen2.Infrastructure/SytsBackendGen2.Infrastructure.csproj", "SytsBackendGen2.Infrastructure/"]
RUN dotnet restore "./SytsBackendGen2.Web/SytsBackendGen2.Web.csproj"
COPY . .
WORKDIR "/src/SytsBackendGen2.Web"
RUN dotnet build "./SytsBackendGen2.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SytsBackendGen2.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SytsBackendGen2.Web.dll"]