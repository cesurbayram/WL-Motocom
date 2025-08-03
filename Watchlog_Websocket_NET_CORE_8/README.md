# Servis olarak derle
dotnet publish Watchlog_Websocket_NET_CORE_8.csproj -c Release -r win-x64 --self-contained false
- sonra "C:\Users\Can\source\repos\Watchlog_Websocket_NET_CORE_8\Watchlog_Websocket_NET_CORE_8\bin\Release\net8.0\win-x64" bu klasöün içindekileri kopyla ve al


 
 
 
# Servisi oluştur
sc.exe create "Service" binPath= "C:\Users\Can\Desktop\1\RobotBackupService5\Watchlog_Websocket_NET_CORE_8.exe"
 
# Servis açıklamasını ayarla
sc.exe description "Service" "Robot Yedekleme ve İzleme Servisi"

# Otomatik başlatma ayarı
sc.exe config "Service" start= auto

# Servisi başlat
sc.exe start "Service" 



# DATA BASE SIFIRLAMA
DO $$ 
DECLARE 
    r RECORD;
BEGIN 
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename <> 'users') LOOP
        EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.tablename) || ' CASCADE;';
    END LOOP;
END $$ LANGUAGE plpgsql;





<ItemGroup>
  <Reference Include="YMConnect">
    <HintPath>lib/YMConnect.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="YMConnect_CS">
    <HintPath>lib/YMConnect_CS.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>





# DOCKER
docker build -t watchlog_console_app .
docker run --rm watchlog_console_app


# TAR ÇALIŞTIRMA
- dizine git
- docker load -i watchlog_console_app.tar
- docker run --rm watchlog_console_app



# 1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Watchlog_Websocket_NET_CORE_8/*.csproj ./Watchlog_Websocket_NET_CORE_8/
RUN dotnet restore

COPY . .
WORKDIR /src/Watchlog_Websocket_NET_CORE_8
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Watchlog_Websocket_NET_CORE_8.dll"]


# 2

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sadece proje dosyasını kopyala
COPY Watchlog_Websocket_NET_CORE_8/*.csproj ./Watchlog_Websocket_NET_CORE_8/

# Restore yap
RUN dotnet restore ./Watchlog_Websocket_NET_CORE_8/Watchlog_Websocket_NET_CORE_8.csproj

# Gerekli tüm dosyaları tek tek kopyala (kaynak kod değil, publish için gerekli olanlar)
COPY Watchlog_Websocket_NET_CORE_8/ ./Watchlog_Websocket_NET_CORE_8/

WORKDIR /src/Watchlog_Websocket_NET_CORE_8

RUN dotnet publish -c Release -o /app/publish

# Final image – yalnızca derlenmiş hal
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Watchlog_Websocket_NET_CORE_8.dll"]
