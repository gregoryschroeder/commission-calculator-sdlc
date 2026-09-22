#!/usr/bin/env bash
# Runs the published app in the ASP.NET runtime image with no network and requests a page from a
# sidecar in the same network namespace (the runtime image has no HTTP client; research R16).
# A request from that namespace to an external host must fail: the evidence that there is no
# network. Arguments: [path] [network] — network defaults to "none"; guards pass "bridge".
set -euo pipefail
path="${1:-/}"
network="${2:-none}"
app="offline-smoke-app-$$"
publish="$PWD/out/publish"

dotnet publish src/CommissionCalculator.Web -c Release -o "$publish" >/dev/null
docker run -d --name "$app" --network "$network" -v "$publish:/app:ro" -w /app \
  -e ASPNETCORE_URLS=http://127.0.0.1:8080 mcr.microsoft.com/dotnet/aspnet:10.0.12 \
  dotnet CommissionCalculator.Web.dll >/dev/null
trap 'docker rm -f "$app" >/dev/null 2>&1 || true' EXIT

echo "NetworkMode: $(docker inspect -f '{{.HostConfig.NetworkMode}}' "$app")"
sidecar() { docker run --rm --network "container:$app" curlimages/curl:8.22.0 "$@"; }

status="000"
for _ in $(seq 1 30); do
  status="$(sidecar -s -o /dev/null -w '%{http_code}' "http://127.0.0.1:8080$path" || true)"
  [ "$status" = "200" ] && break
  sleep 1
done
[ "$status" = "200" ] || { echo "offline smoke: GET $path -> $status, expected 200" >&2; docker logs "$app" >&2; exit 1; }
echo "offline smoke: GET $path -> 200"

if sidecar -s -m 5 -o /dev/null https://example.com/; then
  echo "offline smoke: an external request succeeded, so the app was not running without a network" >&2
  exit 1
fi
echo "offline smoke: external request failed, as it must with no network"
