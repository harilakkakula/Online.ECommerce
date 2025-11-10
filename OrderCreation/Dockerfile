FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Common ./Common
COPY OrderCreation.Business ./OrderCreation.Business
COPY OrderCreation ./OrderCreation
RUN dotnet restore "OrderCreation/OrderCreation.csproj"
RUN dotnet build "OrderCreation/OrderCreation.csproj" -c Release -o /app/build
RUN dotnet publish "OrderCreation/OrderCreation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderCreation.dll"]
