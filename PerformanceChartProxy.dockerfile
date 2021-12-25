FROM mcr.microsoft.com/dotnet/aspnet:5.0.7-alpine3.13-amd64 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.301-alpine3.13-amd64 AS build
WORKDIR /app
COPY . .
RUN dotnet restore Pepper.PerformanceChartProxy
RUN dotnet publish Pepper.PerformanceChartProxy -c release --runtime linux-musl-x64 --self-contained -o dist

FROM base AS final
ARG PORT
ENV PORT=$PORT
WORKDIR /app
COPY --from=build /app/dist .
ENTRYPOINT "./Pepper.PerformanceChartProxy.dll"