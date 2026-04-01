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

# ── Resolve DNS server for nginx runtime resolution ─────────────────────────
# nginx needs an explicit resolver directive when proxy_pass uses a variable.
# Extract the first nameserver from the system resolver config; fall back to
# Google Public DNS (safe here because BACKEND_HOST is a public hostname).
DNS_RESOLVER=$(awk '/^nameserver/{print $2; exit}' /etc/resolv.conf)
DNS_RESOLVER="${DNS_RESOLVER:-8.8.8.8}"

export BACKEND_HOST DNS_RESOLVER PORT

# ── Generate nginx config and start ─────────────────────────────────────────
envsubst '$BACKEND_HOST $DNS_RESOLVER $PORT' \
  < /etc/nginx/default.conf.template \
  > /etc/nginx/conf.d/default.conf

exec nginx -g 'daemon off;'
