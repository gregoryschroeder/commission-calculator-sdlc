#!/usr/bin/env bash
# Starts the app the way the README says and waits for an HTTP 200 on the given path.
set -euo pipefail
path="${1:-/}"
url="http://127.0.0.1:5199"
dotnet run --project src/CommissionCalculator.Web --urls "$url" > smoke.log 2>&1 &
pid=$!
trap 'kill "$pid" 2>/dev/null || true' EXIT
for _ in $(seq 1 60); do
  status="$(curl -s -o /dev/null -w '%{http_code}' "$url$path" || true)"
  if [ "$status" = "200" ]; then echo "smoke: GET $path -> 200"; exit 0; fi
  sleep 1
done
echo "smoke: GET $path did not return 200 within 60 s (last status: ${status:-none})" >&2
cat smoke.log >&2
exit 1
