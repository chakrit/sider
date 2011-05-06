@echo off


echo Git clean?
choice
if errorlevel 2 goto build
cmd /c git clean -xdf


:build
echo Building solution...
msbuild /p:Configuration=Package Sider.sln /noconsolelogger
if not errorlevel 0 goto error

echo Building NuGet package...
mkdir build\packages
.\tools\NuGet.exe pack .\src\Sider\Sider.csproj -o build\packages -sym

echo Packages built:
dir build\packages /b/s

goto end


:error
echo Build error detected, stopping.


:end
echo Done.
