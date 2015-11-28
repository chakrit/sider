#!/bin/sh

type mono >/dev/null 2>&1 || {
  echo MONO command not found, please install Mono.framework first.
  exit 1
}

type xbuild >/dev/null 2>&1 || {
  echo XBUILD command not found, please install Mono.framework first.
  exit 1
}

[ -e .nuget/nuget.exe ] || {
  echo Downloading nuget.exe
  curl -v -o .nuget/nuget.exe "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
}

echo Building
xbuild /property:Configuration=Release

echo Packing
mkdir -p build/packages
mono .nuget/nuget.exe pack build/Sider.nuspec -output build/packages
