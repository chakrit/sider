#!/usr/bin/make
ifneq ($(strip $(wildcard Makefile.vars)),)
include Makefile.vars
else
VERSION = master
endif

CONFIG     = Debug
PKG_FILES  = Sider/bin/$(CONFIG)/Sider.dll Sider/bin/$(CONFIG)/Sider.dll.mdb
TEST_FILE  = Sider.Tests/bin/$(CONFIG)/Sider.Tests.dll
NUPKG_FILE = Sider.$(VERSION).nupkg

ARTIFACT_FILES = \
	$(wildcard Sider.*.nupkg) \
	$(wildcard **/bin/**/*) \
	$(wildcard **/obj/**/*)

build: $(PKG_FILES)
$(PKG_FILES):
	@xbuild /property:Configuration=$(CONFIG)

test: build
	@nunit-console -labels $(TEST_FILE)

clean:
ifneq ($(strip $(ARTIFACT_FILES)),)
	@rm -v $(ARTIFACT_FILES) 2>/dev/null
endif

version: Makefile.vars
Makefile.vars:
	@echo "VERSION := $(shell cat Sider/Properties/AssemblyInfo.cs | \
		grep 'AssemblyVersion' | \
		cut -f 2 -d '"' | \
		rev | cut -c 3- | rev)" > \
		Makefile.vars

nupkg: $(NUPKG_FILE)
$(NUPKG_FILE): CONFIG=Release
$(NUPKG_FILE): build
	nuget pack -verbosity detailed Sider.nuspec
