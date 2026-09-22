#!/usr/bin/env bash
# Engine line-coverage floor over every per-project cobertura report of the last test run.
set -euo pipefail
floor="${1:-80}"
reports=()
while IFS= read -r report; do reports+=("$report"); done < <(find TestResults -maxdepth 1 -name '*.cobertura.xml')
[ "${#reports[@]}" -gt 0 ] || { echo "no cobertura reports in TestResults/" >&2; exit 1; }
dotnet run --project tools/CommissionCalculator.Tools --no-build -- coverage-gate "$floor" \
  src/CommissionCalculator.Engine/bin/Debug/net10.0/CommissionCalculator.Engine.dll "${reports[@]}"
