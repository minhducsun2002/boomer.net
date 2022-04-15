FROM mcr.microsoft.com/dotnet/sdk:6.0.101-alpine3.14 as build
WORKDIR /app

COPY . . 
COPY nuget.config .
RUN echo $BUILD_HASH
RUN dotnet publish Pepper -c release -o dist/ 

FROM mcr.microsoft.com/dotnet/runtime:6.0.4-alpine3.14
RUN apk add --no-cache icu-libs

# Disable the invariant mode
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8
COPY --from=build /app/dist /app
WORKDIR /app
CMD dotnet ./Pepper.dll
