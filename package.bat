@echo off

cmd /c git clean -xdf
msbuild /p:Configuration=Package Sider.sln
.\tools\7za.exe a -r build.zip build\*.*