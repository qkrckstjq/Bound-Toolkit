$env:Path = "C:\Program Files\dotnet;$env:Path"
dotnet clean -c Release
dotnet build -c Release
pause
