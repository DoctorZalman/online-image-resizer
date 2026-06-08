# - build stage: restore and publish the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY server.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# - runtime stage: minimal image without SDK
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# - temp storage for uploaded and resized images
RUN mkdir -p /tmp/image-resizer

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "server.dll"]