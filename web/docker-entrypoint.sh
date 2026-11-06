#!/bin/sh
# Writes /config.json from API_URL so the SPA learns the API address at container start.
set -eu

api_url="${API_URL:-http://127.0.0.1:8080}"
escaped=$(printf '%s' "$api_url" | sed 's/\\/\\\\/g; s/"/\\"/g')
printf '{"apiUrl":"%s"}\n' "$escaped" > /usr/share/nginx/html/config.json

exec nginx -g 'daemon off;'
