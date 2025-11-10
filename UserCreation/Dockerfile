FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080


# Set environment (Development, Staging, or Production)
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Common ./Common
COPY UserCreation.Business ./UserCreation.Business
COPY UserCreation ./UserCreation
RUN dotnet restore "UserCreation/UserCreation.csproj"
RUN dotnet build "UserCreation/UserCreation.csproj" -c Release -o /app/build
RUN dotnet publish "UserCreation/UserCreation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserCreation.dll"]
