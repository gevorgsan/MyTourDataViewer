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
# envsubst replaces only ${BACKEND_URL} and ${PORT}, leaving nginx variables ($host etc.) intact.
CMD ["/bin/sh", "-c", "set -e; envsubst '${BACKEND_URL} ${PORT}' < /etc/nginx/nginx.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
