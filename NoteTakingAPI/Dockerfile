FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["NoteTakingAPI.csproj", "./"]
RUN dotnet restore "NoteTakingAPI.csproj"

COPY . .
RUN dotnet build "NoteTakingAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NoteTakingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

EXPOSE 80

ENTRYPOINT ["dotnet", "NoteTakingAPI.dll"]