FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /source

RUN ls
RUN rm -rf /source
COPY . .


RUN dotnet publish -c release -o release

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /source/release
COPY --from=build-env /source/release ./
VOLUME /source/release/data
ENTRYPOINT ["dotnet", "EverlinkAdapter.dll"]