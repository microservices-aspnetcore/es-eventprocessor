#!/bin/bash
cd /pipeline/source/app/publish
echo "Starting Event Processor"
mkdir /pipeline/source/app/publish/tmp
export TMPDIR=/pipeline/source/app/publish/tmp

dotnet StatlerWaldorfCorp.EventProcessor.dll --server.urls=http://0.0.0.0:${PORT-"8080"}