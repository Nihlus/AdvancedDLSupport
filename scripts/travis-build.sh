#!/bin/bash

if [ -z "${Platform}" ]; then
	dotnet build --configuration "${Configuration}"
else
	dotnet build --configuration "${Configuration}" /p:Platform="${Platform}"
fi


