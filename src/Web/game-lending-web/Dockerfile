FROM node:24.15.0-bookworm-slim AS build
WORKDIR /app
COPY src/Web/game-lending-web/package*.json ./
ENV CYPRESS_INSTALL_BINARY=0
RUN npm ci
COPY src/Web/game-lending-web/ ./
RUN npm run build
FROM nginx:1.28.0-alpine
COPY src/Web/game-lending-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/game-lending-web/browser /usr/share/nginx/html
EXPOSE 80
