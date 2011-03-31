@ECHO OFF

msbuild Sider.sln /p:Configuration=Debug
.\packages\NUnit.2.5.9.10348\Tools\nunit-console.exe Sider.Tests.nunit