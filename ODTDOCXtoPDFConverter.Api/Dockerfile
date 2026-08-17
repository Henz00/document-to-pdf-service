FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["ODTDOCXtoPDFConverter.Api.csproj", "./"]

RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

RUN apt-get update \
    && apt-get install -y libreoffice \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

ENTRYPOINT ["dotnet", "ODTDOCXtoPDFConverter.Api.dll"]