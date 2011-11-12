@echo off


echo Git clean?
choice
if errorlevel 2 goto build
cmd /c git clean -xdf


:build
echo Building NuGet package...
mkdir build\packages
.\tools\NuGet.exe pack .\src\Sider\Sider.csproj -build -o build\packages -sym -verbose -e Sider.GUI -e Sider.Benchmark

echo Packages built:
dir build\packages /b/s

goto end


:end
echo Done.
