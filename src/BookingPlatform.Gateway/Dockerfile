FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/BookingPlatform.Gateway/BookingPlatform.Gateway.csproj", "BookingPlatform.Gateway/"]
COPY ["src/BookingPlatform.Shared/BookingPlatform.Shared.csproj", "BookingPlatform.Shared/"]
RUN dotnet restore "BookingPlatform.Gateway/BookingPlatform.Gateway.csproj"

COPY src/ .
WORKDIR "/src/BookingPlatform.Gateway"
RUN dotnet build "BookingPlatform.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingPlatform.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "BookingPlatform.Gateway.dll"]