# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# Copy solution file
COPY *.sln .

# Copy project file
COPY Exam4/*.csproj ./Exam4/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY Exam4/ ./Exam4/

# Build and publish
WORKDIR /source/Exam4
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

# Expose port
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "Exam4.dll"]