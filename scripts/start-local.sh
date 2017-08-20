#!/bin/bash
export ASPNETCORE_ENVIRONMENT=local
cd src/Collectively.Services.Users
dotnet run --no-restore --urls "http://*:10002"