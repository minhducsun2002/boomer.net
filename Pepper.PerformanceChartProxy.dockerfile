FROM mcr.microsoft.com/dotnet/sdk:5.0.301-alpine3.13-amd64 AS build
WORKDIR /app
COPY . .
RUN dotnet restore Pepper.PerformanceChartProxy
RUN dotnet publish Pepper.PerformanceChartProxy -c release --runtime linux-musl-x64 --self-contained -o dist

FROM mcr.microsoft.com/dotnet/aspnet:5.0.7-alpine3.13-amd64 AS base
WORKDIR /app
ARG PORT
ENV PORT=$PORT
COPY --from=build /app/dist .
ENTRYPOINT "/app/Pepper.PerformanceChartProxy"
