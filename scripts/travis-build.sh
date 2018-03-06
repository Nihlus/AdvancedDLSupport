#!/bin/bash

msbuild /p:Configuration=${BuildConfiguration} AdvanceDLSupport.sln
scripts/travis-test.sh
scripts/travis-coverage.sh