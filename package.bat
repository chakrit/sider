@echo off


echo Building NuGet package...
mkdir build\packages
.\.nuget\NuGet.exe pack .\src\Sider\Sider.csproj -build -o build\packages -sym -verbosity detailed -exclude Sider.GUI -exclude Sider.Benchmark

echo Packages built:
dir build\packages /b/s

echo Done.

