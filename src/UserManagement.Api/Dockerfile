FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build

ARG BUILDCONFIG=RELEASE
ARG VERSION=1.0.0

# copy src
COPY ./src/ ./src/
WORKDIR /src/UserManagement.Api/
RUN dotnet publish -c $BUILDCONFIG -o out /p:Version=$VERSION

# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1
WORKDIR /app
COPY --from=build /src/UserManagement.Api/out ./

EXPOSE 5000
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]