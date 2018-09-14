#!/bin/bash

function runCoverage
{
	PROJECT=$1

	XMLREPORT="coverage-$PROJECT.xml"

	cd ${PROJECT}

	dotnet test /p:AltCover=true /p:CopyLocalLockFileAssemblies=true /p:AltCoverXmlReport=${XMLREPORT} --configuration ${Configuration}

	mv coverage*.xml ../

	cd -
}

# Run coverage
runCoverage AdvancedDLSupport.Tests
runCoverage AdvancedDLSupport.AOT.Tests
runCoverage Mono.DllMap.Tests
