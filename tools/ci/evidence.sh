#!/usr/bin/env bash
# Writes a CI-evidence record for a CI gate (not a test): tasks T035, maintainer decision D1.
# Arguments: <job> <check> <requirement-ids, comma-separated> <outcome>
set -euo pipefail
mkdir -p ci-evidence
job="$1"; check="$2"; requirements="$3"; outcome="$4"
ids="$(printf '%s' "$requirements" | sed 's/[^,][^,]*/"&"/g')"
cat > "ci-evidence/$job.json" <<JSON
{
  "job": "$job",
  "check": "$check",
  "requirements": [$ids],
  "result": "$outcome",
  "commit": "${GITHUB_SHA:-local}",
  "runUrl": "${GITHUB_SERVER_URL:-}/${GITHUB_REPOSITORY:-}/actions/runs/${GITHUB_RUN_ID:-local}"
}
JSON
cat "ci-evidence/$job.json"
