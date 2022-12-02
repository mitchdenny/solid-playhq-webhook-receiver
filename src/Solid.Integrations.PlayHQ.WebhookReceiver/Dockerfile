#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 3000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Solid.Integrations.PlayHQ.WebhookReceiver.csproj", "Solid.Integrations.PlayHQ.WebhookReceiver/"]
RUN dotnet restore "Solid.Integrations.PlayHQ.WebhookReceiver/Solid.Integrations.PlayHQ.WebhookReceiver.csproj"
COPY . Solid.Integrations.PlayHQ.WebhookReceiver/
WORKDIR "/src/Solid.Integrations.PlayHQ.WebhookReceiver"
RUN dotnet build "Solid.Integrations.PlayHQ.WebhookReceiver.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Solid.Integrations.PlayHQ.WebhookReceiver.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Solid.Integrations.PlayHQ.WebhookReceiver.dll"]