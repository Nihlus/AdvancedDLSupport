#!/bin/bash

msbuild /p:Configuration=${BuildConfiguration} /p:Platform=${TargetPlatform} AdvanceDLSupport.sln
scripts/travis-test.sh
scripts/travis-coverage.sh