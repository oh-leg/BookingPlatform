FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/BookingPlatform.ResourceService/BookingPlatform.ResourceService.csproj", "Services/BookingPlatform.ResourceService/"]
COPY ["src/BookingPlatform.Shared/BookingPlatform.Shared.csproj", "BookingPlatform.Shared/"]
RUN dotnet restore "Services/BookingPlatform.ResourceService/BookingPlatform.ResourceService.csproj"

COPY src/ .
WORKDIR "/src/Services/BookingPlatform.ResourceService"
RUN dotnet build "BookingPlatform.ResourceService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingPlatform.ResourceService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingPlatform.ResourceService.dll"]