#!/usr/bin/env bash
set -euo pipefail

job="${1:-all}"
repo_root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$repo_root"

run_job() {
  docker compose -f docker-compose.ci.yml run --rm "$1"
}

case "$job" in
  api) run_job ci-test-api ;;
  web) run_job ci-test-web ;;
  all)
    run_job ci-test-api
    run_job ci-test-web
    ;;
  *)
    echo "Usage: $0 [all|api|web]" >&2
    exit 1
    ;;
esac
