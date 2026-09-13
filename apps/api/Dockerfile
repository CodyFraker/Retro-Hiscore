FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY apps/api/RetroHiscore.Api.csproj apps/api/
RUN dotnet restore apps/api/RetroHiscore.Api.csproj
COPY apps/api/ apps/api/
RUN dotnet publish apps/api/RetroHiscore.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RetroHiscore.Api.dll"]
