# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

# Serve stage
FROM nginx:alpine
# Angular 19's application builder outputs browser files into dist/frontend/browser/
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
# Store the config as a template; BACKEND_URL is substituted at container start.
COPY nginx.conf /etc/nginx/nginx.conf.template
EXPOSE 80
# Default backend URL for local Docker Compose usage.
ENV BACKEND_URL=http://backend:5000
# Default port for local Docker Compose usage.
ENV PORT=80
# envsubst replaces only ${BACKEND_URL}, ${PORT}, and ${NGINX_RESOLVER}, leaving nginx
# variables ($host etc.) intact.
# Prepend https:// when BACKEND_URL is a bare hostname (Render injects the host property without a scheme).
# NGINX_RESOLVER is read from /etc/resolv.conf so nginx can resolve the upstream at request time
# (avoids "host not found in upstream" startup failures when the backend isn't ready yet).
CMD ["/bin/sh", "-c", "set -e; case \"$BACKEND_URL\" in http*) ;; *) BACKEND_URL=\"https://$BACKEND_URL\";; esac; export NGINX_RESOLVER=$(grep nameserver /etc/resolv.conf | head -1 | sed 's/nameserver //'); NGINX_RESOLVER=${NGINX_RESOLVER:-127.0.0.11}; export NGINX_RESOLVER; envsubst '${BACKEND_URL} ${PORT} ${NGINX_RESOLVER}' < /etc/nginx/nginx.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
