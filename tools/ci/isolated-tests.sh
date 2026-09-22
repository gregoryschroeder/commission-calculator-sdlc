#!/usr/bin/env bash
# Runs every test of the last suite run on its own, so a test that only passes because another
# test ran first fails here (constitution Principle II: each test passes alone).
set -euo pipefail
failed=0
for trx in TestResults/*.trx; do
  project_name="$(basename "$trx" | sed -E 's/_net[0-9.]+_[a-z0-9]+\.trx$//')"
  project="$(find tests -name "$project_name.csproj" | head -1)"
  [ -n "$project" ] || { echo "no project for $trx" >&2; exit 1; }
  while IFS= read -r test; do
    if dotnet test --project "$project" --no-build --fail-skips on --filter-method "$test" >/dev/null 2>&1; then
      echo "alone ok   $test"
    else
      echo "alone FAIL $test"; failed=1
    fi
  done < <(dotnet run --project tools/CommissionCalculator.Tools --no-build -- list-tests "$trx")
done
exit "$failed"
