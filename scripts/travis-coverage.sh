#!/bin/bash

ANY_TESTS_FAILED=0

function runCoverage
{
	PROJECT=$1

	XMLREPORT="coverage-$PROJECT.xml"

	cd ${PROJECT}

	dotnet test /p:AltCover=true /p:CopyLocalLockFileAssemblies=true /p:AltCoverXmlReport=${XMLREPORT} --configuration ${Configuration}
	if [ $? != 0 ]; then
		ANY_TESTS_FAILED=1
	fi

	mv coverage*.xml ../

	cd -
}

# Run coverage
runCoverage AdvancedDLSupport.Tests
runCoverage AdvancedDLSupport.AOT.Tests
runCoverage Mono.DllMap.Tests

if [ ${ANY_TESTS_FAILED} != 0 ]; then
	echo "Tests failed."
	exit 1
fi
