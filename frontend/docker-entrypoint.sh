#!/bin/sh
set -e

# ── Resolve BACKEND_HOST ────────────────────────────────────────────────────
# Prefer BACKEND_HOST (bare hostname, e.g. mytour-backend.onrender.com).
# Fall back to extracting the hostname from BACKEND_URL if provided.
BACKEND_HOST="${BACKEND_HOST:-$(printf '%s' "${BACKEND_URL}" | sed -E -e 's|^https?://||' -e 's|/.*||')}"

if [ -z "$BACKEND_HOST" ]; then
  echo "ERROR: BACKEND_HOST (or BACKEND_URL) must be set" >&2
  exit 1
fi

# ── Resolve listening port ──────────────────────────────────────────────────
# Render injects PORT (default 10000); fall back to 8080 for local Docker.
PORT="${PORT:-8080}"

# ── Resolve DNS resolver for nginx ──────────────────────────────────────────
# Extract the first nameserver from /etc/resolv.conf so nginx can resolve
# upstream hostnames at request time (not just at config-load time).
DNS_RESOLVER="$(awk '/^nameserver/{print $2; exit}' /etc/resolv.conf)"
DNS_RESOLVER="${DNS_RESOLVER:-8.8.8.8}"

export BACKEND_HOST PORT DNS_RESOLVER

# ── Generate nginx config and start ─────────────────────────────────────────
envsubst '$BACKEND_HOST $PORT $DNS_RESOLVER' \
  < /etc/nginx/default.conf.template \
  > /etc/nginx/conf.d/default.conf

exec nginx -g 'daemon off;'
