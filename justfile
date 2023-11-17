@run:
  dotnet run --project MangoBot.Runner/MangoBot.Runner.csproj
  
@services-up:
  docker-compose up -d
  
@services-down:
  docker-compose down -d