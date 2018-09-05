@echo off


echo Building NuGet package...
mkdir build\packages

dotnet pack .\src\Sider\Sider.csproj -o ..\..\build\packages\ --include-symbols -v d -c Release

echo Packages built:
dir build\packages /b/s

echo Done.

