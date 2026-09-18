FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MyAi.Domain/MyAi.Domain.csproj", "MyAi.Domain/"]
COPY ["src/MyAi.Application/MyAi.Application.csproj", "MyAi.Application/"]
COPY ["src/MyAi.Infrastructure/MyAi.Infrastructure.csproj", "MyAi.Infrastructure/"]
COPY ["src/MyAi.Api/MyAi.Api.csproj", "MyAi.Api/"]
RUN dotnet restore "MyAi.Api/MyAi.Api.csproj"

COPY src/ .
RUN dotnet publish "MyAi.Api/MyAi.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyAi.Api.dll"]
