@ECHO OFF

:loop
echo Building...
msbuild Sider.sln /p:Configuration=Debug > nul
echo Running NUnit tests...
.\packages\NUnit.2.5.9.10348\Tools\nunit-console.exe Sider.Tests.nunit /nologo %1 %2 %3 %4 %5 %6

echo.
echo "Press CTRL+C to stop."
pause
goto loop
