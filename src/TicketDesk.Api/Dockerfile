FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TicketDesk.Api/TicketDesk.Api.csproj", "src/TicketDesk.Api/"]
RUN dotnet restore "src/TicketDesk.Api/TicketDesk.Api.csproj"
COPY . .
WORKDIR "/src/src/TicketDesk.Api"
RUN dotnet build "TicketDesk.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TicketDesk.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketDesk.Api.dll"]