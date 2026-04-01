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
# Extract the bare hostname from BACKEND_URL (e.g. https://foo.fly.dev → foo.fly.dev),
# substitute it into the nginx config template, then start nginx.
CMD ["/bin/sh", "-c", "BACKEND_HOST=$(printf '%s' \"${BACKEND_URL}\" | sed -E -e 's|^https?://||' -e 's|/.*||') && envsubst '$BACKEND_HOST' < /etc/nginx/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
