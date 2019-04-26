# Dotnet Build
FROM microsoft/dotnet:2.2.100-sdk as dotnetbuild

COPY . /app

WORKDIR /app/

RUN dotnet restore
RUN dotnet publish -c Release --output ./out

# Run image.
FROM microsoft/dotnet:2.2.0-runtime

# This will install the ps command, and this will help create dump of the process if necessary
RUN apt-get update && apt-get install -y procps

WORKDIR /app
COPY --from=dotnetbuild /app/out .

EXPOSE 80
CMD ["dotnet", "SeCoucherMoinsBeteRssFeed.dll"]