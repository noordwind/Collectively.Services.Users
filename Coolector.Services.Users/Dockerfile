FROM microsoft/dotnet:latest
COPY . /app
WORKDIR /app
 
RUN ["dotnet", "restore", "--source", "https://api.nuget.org/v3/index.json", "--source", "https://www.myget.org/F/coolector/api/v3/index.json", "--no-cache"]
RUN ["dotnet", "build"]
 
EXPOSE 10001/tcp
ENV ASPNETCORE_URLS http://*:10001
ENV ASPNETCORE_ENVIRONMENT local
 
ENTRYPOINT ["dotnet", "run"]
