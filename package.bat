@echo off


echo Git clean?
choice
if errorlevel 2 goto build
cmd /c git clean -xdf


:build
echo Building solution...
msbuild /p:Configuration=Package Sider.sln /noconsolelogger
if not errorlevel 0 goto error

echo Structuring built files for NuGet...
mkdir build\nuget_temp\lib\net40
copy build\*.dll build\nuget_temp\lib\net40

echo Building NuGet package...
mkdir build\packages
.\tools\NuGet.exe pack .\Sider.nuspec -b build\nuget_temp -o build\packages

echo Packages built:
dir build\packages /b/s

goto end


:error
echo Build error detected, stopping.


:end
echo Done.
