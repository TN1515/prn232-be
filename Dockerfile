# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY HandMadeShopping.sln ./
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY EmailService/EmailService.csproj EmailService/
COPY Infrastructer/Infrastructer.csproj Infrastructer/
COPY RabibtMQ/RabbitMQ.csproj RabibtMQ/
COPY SePaySerivce/SePaySerivce.csproj SePaySerivce/

RUN dotnet restore "HandMadeShopping.sln"

COPY . ./
RUN dotnet publish "API/API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "API.dll"]
