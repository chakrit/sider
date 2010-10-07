@echo off

cmd /c git clean -xdf
msbuild /p:Configuration=Package Sider.sln

echo Building sider.all.zip...
.\tools\7za.exe a -r sider.all.zip build\*.*

echo Build NuPkg?
choice
if errorlevel 2 goto end

echo Building NuSpec package...
.\tools\NuPack.exe .\Sider.nuspec
.\tools\7za.exe a -r sider.nupkg.zip sider.*.nupkg

:end