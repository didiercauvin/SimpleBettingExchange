########################################
#  First stage of multistage build
########################################
#  Use Build image with label `builder
########################################
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

# Setup working directory for project
WORKDIR /app

# Copy everything
COPY . .

WORKDIR /app/Simple.BettingExchange.Api

# Restore as distinct layers
RUN dotnet restore

# Build and publish a release
RUN dotnet publish -c Release -o out

########################################
#  Second stage of multistage build
########################################
#  Use other build image as the final one
#    that won't have source codes
########################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Setup working directory for project
WORKDIR /app

# Copy published in previous stage binaries
# from the `builder` image
COPY --from=builder /app/Simple.BettingExchange.Api/out .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000

# sets entry point command to automatically
# run application on `docker run`
ENTRYPOINT ["dotnet", "Simple.BettingExchange.Api.dll"]
