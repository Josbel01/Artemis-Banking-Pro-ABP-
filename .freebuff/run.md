# Artemis Banking Pro — WebAPI Preview

## How to reproduce artifacts
No special artifacts needed. The .NET project builds from source.

## How to run the server
```bash
dotnet run --project "Artemis Banking Pro WebApi/ArtemisBankingApi.csproj" --urls http://localhost:5144 --no-launch-profile
```
- Uses HTTP only on port 5144 (avoids HTTPS redirect CORS issues in Swagger)
- Requires SQL Server Express running locally
- Connection string in appsettings.json: `Server=.\SQLEXPRESS;Database=ArtemisBankingAppDb`
- If port 5144 is in use, kill the old process first: `netstat -ano | grep :5144`
- Background launch (Git Bash): `dotnet run ... > .freebuff/preview.log 2> .freebuff/preview.log.err &`
