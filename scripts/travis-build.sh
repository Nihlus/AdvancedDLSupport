#!/bin/bash

dotnet build --configuration "${Configuration}" /p:Platform="${Platform}"
