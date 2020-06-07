FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app

COPY /PitBoss /PitBoss
COPY ./ContainerService/*.csproj ./
RUN dotnet restore

COPY ./ContainerService ./
RUN ls -la
RUN dotnet clean
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
RUN dotnet tool install dotnet-script --tool-path /usr/bin
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80
CMD ["dotnet", "ContainerService.dll"]