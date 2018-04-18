#!/bin/bash

OutputDir="${Configuration}"
if [ "${Platform}" == "x86" ] || [ "${Platform}" == "x64" ]; then
	OutputDir="${Platform}/${Configuration}"
fi

# Fix inconsistent spacing in platform targets
if [ "${Platform}" == "Any CPU" ]; then
	Platform="AnyCPU"
fi

ALTCOVER_VERSION="3.0.422"
ALTCOVER_PATH="altcover/altcover.$ALTCOVER_VERSION/tools/netcoreapp2.0/AltCover.dll"

RUNTIME_VERSION=$(dotnet --info | sed -n '/Microsoft .NET Core Shared Framework Host/,$p' | grep Version | awk '{print $3}')

function runCoverage
{
	PROJECT=$1
	FRAMEWORK=$2

	INPUT_DIRECTORY="$PROJECT/bin/$OutputDir/$FRAMEWORK"
	OUTPUT_DIRECTORY="instrumented/$PROJECT-$FRAMEWORK"
	XMLREPORT="coverage-$PROJECT-$FRAMEWORK.xml"

	dotnet ${ALTCOVER_PATH} \
	--inputDirectory ${INPUT_DIRECTORY} \
	--outputDirectory ${OUTPUT_DIRECTORY} \
	--xmlReport ${XMLREPORT} \
	--opencover \
	--save \
	--inplace \
	--assemblyExcludeFilter ".+\.Tests" \
	--assemblyExcludeFilter "AltCover.+"

	cd ${PROJECT}

	dotnet xunit -nobuild -noshadow -framework ${FRAMEWORK} -fxversion ${RUNTIME_VERSION} -configuration ${Configuration}

	cd -

	dotnet ${ALTCOVER_PATH} runner --recorderDirectory ${INPUT_DIRECTORY} --collect
}

# Install AltCover
nuget install altcover -OutputDirectory altcover -Version ${ALTCOVER_VERSION}

# Run coverage
runCoverage AdvancedDLSupport.Tests netcoreapp2.0
runCoverage AdvancedDLSupport.Tests net461

runCoverage Mono.DllMap.Tests netcoreapp2.0
runCoverage Mono.DllMap.Tests net461

# Restore inconsistent spacing in platform targets
if [ "${Platform}" == "AnyCPU" ]; then
	Platform="Any CPU"
fi

