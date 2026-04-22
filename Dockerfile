# Use .NET 8 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ITEC275LiveQuiz/ITEC275LiveQuiz.csproj ITEC275LiveQuiz/
RUN dotnet restore ITEC275LiveQuiz/ITEC275LiveQuiz.csproj

# Copy everything else and build
COPY ITEC275LiveQuiz/ ITEC275LiveQuiz/
WORKDIR /src/ITEC275LiveQuiz
RUN dotnet publish -c Release -o /app/publish

# Use .NET 8 runtime for final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ITEC275LiveQuiz.dll"]
