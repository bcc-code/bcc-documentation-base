# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY ["documentation-site/*.csproj", "./documentation-site/"]
RUN dotnet restore

# Copy everything else and build
COPY ["documentation-site/.", "./documentation-site/"]
WORKDIR "/source/documentation-site"
RUN dotnet publish -c release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app ./
EXPOSE 8080 44357
ENTRYPOINT ["dotnet", "documentation-site.dll"]
