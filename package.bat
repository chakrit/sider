@echo off

cmd /c git clean -xdf
msbuild /p:Configuration=Package Sider.sln

echo Building sider.all.zip...
.\tools\7za.exe a -r sider.all.zip build\*.*

echo Building NuSpec package...
.\tools\NuPack.exe .\package.nuspec
.\tools\7za.exe a -r sider.nupkg.zip sider.*.nupkg