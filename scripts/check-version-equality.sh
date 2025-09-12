#!/bin/bash
# filepath: /workspace/check-version-equality.sh

set -e

REPO_ROOT=$(git rev-parse --show-cdup)
cd "$REPO_ROOT"

proj1="src/WebJobs.Extensions.IBMMQ/WebJobs.Extensions.IBMMQ.csproj"
proj2="src/Worker.Extensions.IBMMQ/Worker.Extensions.IBMMQ.csproj"

version1=$(grep -oPm1 '(?<=<Version>)[^<]+' "$proj1")
version2=$(grep -oPm1 '(?<=<Version>)[^<]+' "$proj2")

if [[ "$version1" == "$version2" ]]; then
  echo "Versions match: $version1"
  exit 0
else
  echo "Versions differ: $version1 != $version2"
  exit 1
fi