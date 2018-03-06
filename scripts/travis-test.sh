#!/bin/bash

dotnet test --configuration ${BuildConfiguration} --no-build AdvancedDLSupport.Tests
dotnet test --configuration ${BuildConfiguration} --no-build Mono.DllMap.Tests