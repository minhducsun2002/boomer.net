FROM mcr.microsoft.com/dotnet/sdk:6.0.101-alpine3.14 as build
WORKDIR /app

COPY . . 
COPY nuget.config .
RUN echo $BUILD_HASH
RUN dotnet publish Pepper -c release --runtime linux-musl-x64 --self-contained -o dist/ 

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.1-alpine3.14
COPY --from=build /app/dist /app
WORKDIR /app
CMD ./Pepper
