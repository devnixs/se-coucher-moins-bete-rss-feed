# Dotnet Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview4 as dotnetbuild

COPY . /app

WORKDIR /app/SeCoucherMoinsBeteRssFeed

RUN dotnet restore
RUN dotnet publish -c Release --output ./out

# Run image.
FROM microsoft/microsoft-dotnet-core-sdk:3.0.100-preview4

# This will install the ps command, and this will help create dump of the process if necessary
RUN apt-get update && apt-get install -y procps

WORKDIR /app
COPY --from=dotnetbuild /app/SeCoucherMoinsBeteRssFeed/out .
RUN rm .env
RUN echo '' > .env

EXPOSE 80
CMD ["dotnet", "SeCoucherMoinsBeteRssFeed.dll"]