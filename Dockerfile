ARG BuildVersion

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app


# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" tubescanuser && chown -R tubescanuser /app
USER tubescanuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
ARG BuildVersion
WORKDIR /src

COPY ["src/TubeScan/TubeScan.csproj", "src/TubeScan/"]
RUN dotnet restore "src/TubeScan/TubeScan.csproj"
COPY . .
WORKDIR "/src/src/TubeScan"
RUN dotnet tool restore
RUN dotnet paket restore
RUN dotnet build "TubeScan.csproj" -c Release -o /app/build /p:AssemblyInformationalVersion=${BuildVersion} /p:AssemblyFileVersion=${BuildVersion}

FROM build AS publish
ARG BuildVersion
RUN dotnet publish "TubeScan.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:AssemblyInformationalVersion=${BuildVersion} /p:AssemblyFileVersion=${BuildVersion} /p:Version=${BuildVersion}

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TubeScan.dll", "start"]
