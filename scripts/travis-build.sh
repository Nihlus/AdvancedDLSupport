#!/bin/bash

msbuild /p:Configuration=Debug AdvanceDLSupport.sln
scripts/travis-test.sh
scripts/travis-coverage.sh