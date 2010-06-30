@echo off

cmd /c git clean -xdf
msbuild /p:Configuration=Release Sider.sln
.\tools\7za.exe ar build.zip build\*.*