# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ITEC275LiveQuiz/ITEC275LiveQuiz.csproj ITEC275LiveQuiz/
RUN cd ITEC275LiveQuiz && dotnet restore

# Copy everything else and build
COPY ITEC275LiveQuiz/ ITEC275LiveQuiz/
RUN cd ITEC275LiveQuiz && dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Expose port (Railway will inject PORT environment variable)
EXPOSE 8080

ENTRYPOINT ["dotnet", "ITEC275LiveQuiz.dll"]
