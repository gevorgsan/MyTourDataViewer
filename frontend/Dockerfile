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
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh
# Render injects PORT=10000 by default; keep 10000 as the documented port.
# For local Docker (docker-compose) you can override with -e PORT=8080.
EXPOSE 10000
# BACKEND_HOST must be the bare backend hostname (e.g. mytour-backend.onrender.com).
# On Render this is injected directly via fromService property: host.
# For other platforms, BACKEND_URL (full URL, e.g. https://foo.example.com) can be
# provided instead; the bare hostname is then extracted automatically.
CMD ["/docker-entrypoint.sh"]
