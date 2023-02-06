# Copyright (c) 2023 Koji Hasegawa.
# This software is released under the MIT License.

PACKAGE_HOME?=$(dir $(abspath $(lastword $(MAKEFILE_LIST))))
PROJECT_HOME?=$(PACKAGE_HOME)/UnityProject~
BUILD_DIR?=$(PROJECT_HOME)/Build
LOG_DIR?=$(PROJECT_HOME)/Logs
UNITY_VERSION?=$(shell grep '"unity":' $(PACKAGE_HOME)/package.json | grep -o -E '\d{4}\.[1-4]').$(shell grep '"unityRelease":' $(PACKAGE_HOME)/package.json | grep -o -E '\d+[abfp]\d+')
PACKAGE_NAME?=$(shell grep -o -E '"name": "(.+)"' $(PACKAGE_HOME)/package.json | cut -d ' ' -f2)
ASSEMBLY_NAME?=$(shell ls $(PACKAGE_HOME)/Runtime/*.asmdef| sed -e s/.*\\/// | sed -e s/.asmdef//)

# Code Coverage report filter (comma separated)
# see: https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html
COVERAGE_ASSEMBLY_FILTERS?=+$(ASSEMBLY_NAME)*,-*Tests

UNAME := $(shell uname)
ifeq ($(UNAME), Darwin)
# macOS
UNITY_HOME=/Applications/Unity/HUB/Editor/$(UNITY_VERSION)/Unity.app/Contents
UNITY?=$(UNITY_HOME)/MacOS/Unity
else
ifeq ($(UNAME), Linux)
# Linux: not test yet
UNITY_HOME=$HOME/Unity/Hub/Editor/<version>
UNITY?=$(UNITY_HOME)/Unity
else
# Windows: not test yet
UNITY_HOME=C:\Program Files\Unity\Hub\Editor\$(UNITY_VERSION)\Editor
UNITY?=$(UNITY_HOME)\Unity.exe
endif
endif

define test_arguments
  -projectPath $(PROJECT_HOME) \
  -batchmode \
  -nographics \
  -silent-crashes \
  -stackTraceLogType Full \
  -runTests \
  -testCategory "!IgnoreCI" \
  -testPlatform $(TEST_PLATFORM) \
  -testResults $(LOG_DIR)/test_$(TEST_PLATFORM)_results.xml \
  -logFile $(LOG_DIR)/test_$(TEST_PLATFORM).log
endef

define test
  $(eval TEST_PLATFORM=$1)
  $(eval TEST_ARGUMENTS=$(call test_arguments))
  mkdir -p $(LOG_DIR)
  $(UNITY) \
    $(TEST_ARGUMENTS)
endef

define cover
  $(eval TEST_PLATFORM=$1)
  $(eval TEST_ARGUMENTS=$(call test_arguments))
  mkdir -p $(LOG_DIR)
  $(UNITY) \
    $(TEST_ARGUMENTS) \
    -burst-disable-compilation \
    -debugCodeOptimization \
    -enableCodeCoverage \
    -coverageResultsPath $(LOG_DIR) \
    -coverageOptions 'generateAdditionalMetrics;generateTestReferences;dontClear;assemblyFilters:$(COVERAGE_ASSEMBLY_FILTERS)'
endef

define cover_report
  mkdir -p $(LOG_DIR)
  $(UNITY) \
    -projectPath $(PROJECT_HOME) \
    -batchmode \
    -quit \
    -enableCodeCoverage \
    -coverageResultsPath $(LOG_DIR) \
    -coverageOptions 'generateHtmlReport;generateAdditionalMetrics;generateAdditionalReports;assemblyFilters:$(COVERAGE_ASSEMBLY_FILTERS)'
endef

.PHONY: usage
usage:
	@echo "Tasks:"
	@echo "  create_project_for_run_tests: Create Unity project for run UPM package tests."
	@echo "  remove_project: Remove created project."
	@echo "  clean: Clean Build and Logs directories in created project."
	@echo "  test_editmode: Run Edit Mode tests."
	@echo "  test_playmode: Run Play Mode tests."
	@echo "  cover_report: Create code coverage HTML report."
	@echo "  test: Run test_editmode, test_playmode, and cover_report. Recommended to use with '-k' option."

# Create Unity project for run UPM package tests. And upgrade and add dependencies for tests.
# Required install [openupm-cli](https://github.com/openupm/openupm-cli).
.PHONY: create_project_for_run_tests
create_project_for_run_tests:
	$(UNITY) \
	  -createProject $(PROJECT_HOME) \
	  -batchmode \
	  -quit
	cp $(PACKAGE_HOME)/.gitignore $(PROJECT_HOME)
	touch UnityProject~/Assets/.gitkeep
	openupm -c $(PROJECT_HOME) add com.unity.test-framework
	openupm -c $(PROJECT_HOME) add com.unity.testtools.codecoverage
	openupm -c $(PROJECT_HOME) add com.cysharp.unitask
	openupm -c $(PROJECT_HOME) add --test $(PACKAGE_NAME)@file:../../

.PHONY: remove_project
remove_project:
	rm -rf $(PROJECT_HOME)

.PHONY: clean
clean:
	rm -rf $(BUILD_DIR)
	rm -rf $(LOG_DIR)

.PHONY: test_editmode
test_editmode:
	$(call cover,editmode)

.PHONY: test_playmode
test_playmode:
	$(call cover,playmode)

.PHONY: cover_report
cover_report:
	$(call cover_report)

# Run Edit Mode and Play Mode tests with coverage and html-report by code coverage package.
# If you run this target with the `-k` option, if the Edit/Play Mode test fails,
# it will run through to Html report generation and return an exit code indicating an error.
.PHONY: test
test: test_editmode test_playmode cover_report
