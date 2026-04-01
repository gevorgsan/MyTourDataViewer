# ── Build stage ────────────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/default.conf.template
EXPOSE 8080
# BACKEND_HOST must be the bare backend hostname (e.g. mytour-backend.onrender.com).
# On Render this is injected directly via fromService property: host.
# For other platforms, BACKEND_URL (full URL, e.g. https://foo.example.com) can be
# provided instead; the bare hostname is then extracted automatically.
CMD ["/bin/sh", "-c", "BACKEND_HOST=\"${BACKEND_HOST:-$(printf '%s' \"${BACKEND_URL}\" | sed -E -e 's|^https?://||' -e 's|/.*||')}\" && [ -n \"$BACKEND_HOST\" ] || { echo 'ERROR: BACKEND_HOST (or BACKEND_URL) must be set'; exit 1; } && envsubst '$BACKEND_HOST' < /etc/nginx/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
