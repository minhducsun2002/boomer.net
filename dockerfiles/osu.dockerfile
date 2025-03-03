FROM mcr.microsoft.com/dotnet/sdk:8.0.406-alpine3.21 as build
WORKDIR /app

COPY . . 
COPY nuget.config .
RUN echo $BUILD_HASH
RUN dotnet restore
RUN dotnet publish Pepper.Frontends.Osu -c release -o dist/ 

FROM mcr.microsoft.com/dotnet/runtime:8.0.13-alpine3.21
RUN apk add --no-cache icu-libs

# Disable the invariant mode
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
COPY --from=build /app/dist /app
WORKDIR /app
CMD dotnet ./Pepper.Frontends.Osu.dll
