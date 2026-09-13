#!/bin/sh
set -e

LOCK_SHA_FILE="node_modules/.package-lock.sha"

if [ ! -f "$LOCK_SHA_FILE" ] || ! cmp -s package-lock.json "$LOCK_SHA_FILE"; then
  echo "package-lock.json changed — running npm ci..."
  npm ci
  cp package-lock.json "$LOCK_SHA_FILE"
fi

exec "$@"
