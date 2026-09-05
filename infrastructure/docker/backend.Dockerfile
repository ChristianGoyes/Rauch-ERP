FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend ./backend
RUN dotnet restore backend/src/RauchERP.Api/RauchERP.Api.csproj
RUN dotnet publish backend/src/RauchERP.Api/RauchERP.Api.csproj -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RauchERP.Api.dll"]
