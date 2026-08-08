FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/BookingPlatform.NotificationService/BookingPlatform.NotificationService.csproj", "Services/BookingPlatform.NotificationService/"]
COPY ["src/BookingPlatform.Shared/BookingPlatform.Shared.csproj", "BookingPlatform.Shared/"]
RUN dotnet restore "Services/BookingPlatform.NotificationService/BookingPlatform.NotificationService.csproj"

COPY src/ .
WORKDIR "/src/Services/BookingPlatform.NotificationService"
RUN dotnet build "BookingPlatform.NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingPlatform.NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingPlatform.NotificationService.dll"]