# syntax=docker/dockerfile:1

FROM node:22.23.4-alpine AS build
WORKDIR /src

RUN --mount=type=bind,source=package.json,target=package.json \
    --mount=type=bind,source=package-lock.json,target=package-lock.json \
    --mount=type=cache,target=/root/.npm \
    npm ci

COPY . .
RUN npm run build

FROM nginx:1.28.3-alpine AS runtime
WORKDIR /usr/share/nginx/html

COPY nginx.conf /etc/nginx/nginx.conf
COPY default.conf /etc/nginx/conf.d/default.conf
COPY docker-entrypoint.sh /entrypoint.sh
COPY --from=build --link --chown=101:101 /src/dist/web/ ./

# nginx listens on port 80. The file capability lets the nginx user bind it.
RUN --mount=type=cache,target=/etc/apk/cache,sharing=locked \
    apk add libcap \
    && setcap 'cap_net_bind_service=+ep' /usr/sbin/nginx \
    && apk del libcap \
    && chmod 755 /entrypoint.sh

USER nginx
EXPOSE 80

HEALTHCHECK --interval=5s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -qO- http://127.0.0.1/health | grep -q ok

LABEL org.opencontainers.image.source="https://github.com/bhuvaneshsaha/coolify-demo" \
      org.opencontainers.image.title="coolify-demo-web"

ENTRYPOINT ["/entrypoint.sh"]
