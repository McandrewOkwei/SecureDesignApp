FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Create certificate directory
RUN mkdir -p /app/Cert/

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FinalProject.csproj", "."]
RUN dotnet restore "./FinalProject.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "FinalProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinalProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
# Copy the entire application content
COPY --from=publish /app/publish .

# CRITICAL: Copy the entire source directory to ensure all files are included
COPY . .

# Copy the certificate to the 
COPY Cert/drewslab.selfip.com.pfx Cert/drewslab.selfip.com.pfx
	
RUN mkdir -p /app/wwwroot/Images
RUN mkdir -p /app/wwwroot/Images/browse
RUN mkdir -p /app/wwwroot/images
RUN mkdir -p /app/wwwroot/images/browse

# Copy image files to both locations to ensure they're found regardless of casing
COPY wwwroot/Images /app/wwwroot/Images
COPY wwwroot/Images /app/wwwroot/images
COPY wwwroot/Images/browse /app/wwwroot/Images/browse
COPY wwwroot/Images/browse /app/wwwroot/images/browse
# Set environment to Development to see detailed errors
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/Cert/drewslab.selfip.com.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=hmmmm
ENV DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTPCLIENTTIMEOUTSECONDS=600
ENV DOTNET_RUNNING_IN_=true

ENTRYPOINT ["dotnet", "FinalProject.dll"]

