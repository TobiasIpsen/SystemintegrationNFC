# Inspiration taken from CloudBackend's Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

 EXPOSE 8080

WORKDIR /rasppibackend


FROM mcr.microsoft.com/dotnet/sdk:10.0 as build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RaspberryPiAPI/RaspberryPiAPI.csproj", "RaspberryPiAPI/"]
COPY ["ClassLibrary/ClassLibrary.csproj", "ClassLibrary/"]
RUN dotnet restore "./RaspberryPiAPI/RaspberryPiAPI.csproj"
COPY . .
WORKDIR "/src/RaspberryPiAPI"
RUN dotnet build "./RaspberryPiAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build



FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RaspberryPiAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false



FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RaspberryPiAPI.dll"]
