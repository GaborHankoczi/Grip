# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# install nodejs
RUN apt update && apt install -y curl gnupg
RUN curl -sL https://deb.nodesource.com/setup_16.x | bash -
RUN apt install -y nodejs

# copy csproj and restore as distinct layers
#COPY *.sln .
#COPY *.csproj .
# RUN dotnet restore

# copy everything else and build app
RUN ls
RUN rm -rf /source
COPY . .
WORKDIR /source
VOLUME /etc/cert
RUN dotnet publish -c release -o release

EXPOSE 443

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /source/release
COPY --from=build /source/release ./
ENTRYPOINT ["dotnet", "Grip.dll", "--environment", "TestEnvironment"]
# docker password: dckr_pat_i9IlNKaHdRdPQrOue0KmYYqLSV0