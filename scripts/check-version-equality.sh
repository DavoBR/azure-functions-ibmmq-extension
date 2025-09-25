#!/bin/bash

set -e

REPO_ROOT=$(git rev-parse --show-cdup)
cd "$REPO_ROOT"

webjob_ibmmq="src/WebJobs.Extensions.IBMMQ/WebJobs.Extensions.IBMMQ.csproj"
worker_ibmmq="src/Worker.Extensions.IBMMQ/Properties/AssemblyInfo.cs"

webjob_ibmmq_version=$(grep -oPm1 '(?<=<Version>)[^<]+' "$webjob_ibmmq")
worker_ibmmq_version=$(grep -oPm1 '(?<=ExtensionInformation\("AzureWebJobs.Extensions.IBMMQ", ")[^"]+' "$worker_ibmmq")


if [[ "$webjob_ibmmq_version" == "$worker_ibmmq_version" ]]; then
  echo "Versions match: $webjob_ibmmq_version"
  exit 0
else
  echo "Versions differ: $webjob_ibmmq_version != $worker_ibmmq_version"
  exit 1
fi