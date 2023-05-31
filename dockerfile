FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

RUN mkdir /build
WORKDIR /build

COPY . .

RUN dotnet restore CertPOC.sln

RUN dotnet build CertPOC.sln -c Release
RUN dotnet publish CertPOC.csproj --no-build --output publish -c Release


FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

RUN mkdir /app
WORKDIR /app

COPY --from=build /build/publish .

ENTRYPOINT [ "dotnet", "CertPOC.dll" ]