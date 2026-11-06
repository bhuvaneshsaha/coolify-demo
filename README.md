# coolify-demo

Lightweight [ASP.NET Core](https://dotnet.microsoft.com/) API and [Angular](https://angular.dev/) app for trying a multi-server [Coolify](https://coolify.io/) v4 deploy behind an Nginx load balancer.

The API reports the machine and container that served each request. The web page shows that payload, so a refresh makes it obvious which backend answered.

## Layout

- `api/` — ASP.NET Core minimal API. Listens on port **8080**. No database and no auth.
- `web/` — Angular standalone app. Nginx serves it on port **80**.

## Run locally

Docker with Compose v2 is enough. From the repository root:

```bash
docker compose up --build
```

Published ports are bound to loopback:

| Service | URL |
| --- | --- |
| Web UI | http://127.0.0.1:8081 |
| Web health | http://127.0.0.1:8081/health |
| API health | http://127.0.0.1:8080/health |
| API info | http://127.0.0.1:8080/api/info |
| Weather sample | http://127.0.0.1:8080/api/weatherforecast |

Open the web UI and press **Refresh**. The card shows the container hostname, machine name, environment, app version, and UTC time from `/api/info`.

Stop the stack with `docker compose down`.

### Environment variables

Set these in the shell or in a `.env` file next to `docker-compose.yml`. Defaults work for the local stack.

| Variable | Service | Default | Purpose |
| --- | --- | --- | --- |
| `API_URL` | web | `http://127.0.0.1:8080` | API base URL the **browser** calls. Do not use the Compose service name here. |
| `CORS_ORIGINS` | api | `http://127.0.0.1:8081,http://localhost:8081` | Comma-separated browser origins allowed to call the API. Use `*` to allow any origin. |
| `APP_VERSION` | api | `dev` | Value returned as `appVersion` by `/api/info`. |

`API_URL` is applied when the web container starts, not when the image is built. `web/docker-entrypoint.sh` writes `/usr/share/nginx/html/config.json`, and the Angular app fetches that file before calling `/api/info`. For a local `ng serve` session, `web/public/config.json` is the same kind of file.

## API

- `GET /health` — `200` `{"status":"ok"}`
- `GET /api/info` — machine name, host name, container hostname, environment name, app version, UTC time
- `GET /api/weatherforecast` — five-day sample forecast

Inside a container, the machine name, host name, and `HOSTNAME` are usually the same. That value changes per container, which is what you want behind a load balancer.

## Deploy on Coolify

Create **two Dockerfile apps** from this GitHub repository. Coolify builds each Dockerfile with the base directory as the build context.

### API

- Base directory: `/api`
- Port: `8080`
- Health check path: `/health`
- Environment variables:
  - `APP_VERSION` — for example `1.0.0`
  - `CORS_ORIGINS` — public origin of the web app, for example `https://demo.example.com`. Separate extra origins with commas.

### Web

- Base directory: `/web`
- Port: `80`
- Health check path: `/health`
- Environment variables:
  - `API_URL` — public base URL of the API, for example `https://api.example.com` (no path, no trailing slash required)

Point Coolify's proxy, or an Nginx load balancer in front of more than one server, at the API. Each `/api/info` response includes `containerHostname`, so the web page shows which server handled the request. Both images also define a Docker `HEALTHCHECK` against `/health`.
