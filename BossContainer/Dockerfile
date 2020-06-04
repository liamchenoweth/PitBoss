FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app

COPY /PitBoss /PitBoss
COPY ./BossContainer/*.csproj ./
RUN dotnet restore

COPY ./BossContainer ./
RUN ls -la
RUN dotnet clean
RUN dotnet publish -c Release -o out

FROM node:alpine as web-env
COPY ./BossContainer/application /web-app
WORKDIR /web-app
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
RUN dotnet tool install dotnet-script --tool-path /usr/bin
WORKDIR /app
COPY ./configuration ./configuration
COPY ./scripts ./scripts
COPY --from=build-env /app/out .
COPY --from=web-env /web-app/build ./wwwroot
EXPOSE 80
CMD ["dotnet", "BossContainer.dll"]