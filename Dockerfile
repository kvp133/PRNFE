# Use the official .NET 8.0 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY PRNFE.MVC/*.csproj ./PRNFE.MVC/
RUN dotnet restore ./PRNFE.MVC/PRNFE.MVC.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/PRNFE.MVC
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 9999
EXPOSE 9999

# Set ASP.NET Core to listen on port 9999
ENV ASPNETCORE_URLS=http://+:9999

ENTRYPOINT ["dotnet", "PRNFE.MVC.dll"]