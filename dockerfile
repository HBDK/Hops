FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env
WORKDIR /App

# Copy everything
COPY ./src ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App

RUN mkdir /git \
    && apt update \
    && apt install --no-install-recommends git openssh-client curl -y\
    && rm -rf /var/lib/apt/lists/*\
    && apt autoremove -y \
    && curl -sSL https://get.docker.com/ | sh

COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "hops.dll"]