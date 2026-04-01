# MyTourDataViewer

A full-stack dashboard application that consumes data from external APIs.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10 Web API |
| Frontend | Angular 19 |
| Database | SQLite (default) / PostgreSQL |
| ORM | Entity Framework Core 10 |
| Auth | ASP.NET Core Identity + JWT |
| Roles | `Administrator`, `Viewer` |
| Logging | Serilog |
| Containers | Docker + docker-compose |

## Project structure

```
MyTourDataViewer/
├── backend/
│   └── MyTourDataViewer.Api/
│       ├── Controllers/          # Auth, Users, ApiSettings, Dashboard
│       ├── Data/                 # AppDbContext, DbSeeder, Migrations/
│       ├── Entities/             # ApplicationUser, ApiSettings
│       ├── Models/               # Request/response DTOs
│       ├── Services/             # IAuthService, IUserService, IApiSettingsService,
│       │                         # IExternalApiClientService  (+ implementations)
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
├── frontend/
│   ├── src/app/
│   │   ├── auth/                 # Login page
│   │   ├── dashboard/            # Dashboard page
│   │   ├── admin/                # Users + API-settings pages
│   │   ├── core/
│   │   │   ├── guards/           # authGuard, adminGuard
│   │   │   ├── interceptors/     # jwtInterceptor
│   │   │   ├── models/           # Shared TypeScript interfaces
│   │   │   └── services/         # AuthService, UserService, ApiSettingsService,
│   │   │                         # DashboardService
│   │   └── shared/               # NavbarComponent
│   ├── src/environments/
│   ├── nginx.conf
│   └── Dockerfile
└── docker-compose.yml
```

## Default credentials

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123456` | Administrator |

> **Change the default password immediately after first login.**

## Quick start (local)

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [Angular CLI 19](https://angular.dev/): `npm install -g @angular/cli@19`

### Backend

```bash
cd backend/MyTourDataViewer.Api

# Migrations are applied automatically on startup.
# To apply manually (first time only):
dotnet ef database update

# Start the API
dotnet run
# API available at http://localhost:5000
```

### Frontend

```bash
cd frontend
npm install
ng serve
# App available at http://localhost:4200
```

## Quick start (Docker)

```bash
# Build and start all services
docker-compose up --build

# Frontend → http://localhost:4200
# Backend  → http://localhost:5000
```

### Switch to PostgreSQL

Set `DbProvider=postgres` in `docker-compose.yml` or in `appsettings.json` to use PostgreSQL instead of SQLite.

## Deploy on Fly.io

Fly.io handles HTTPS termination automatically. Backend and frontend are deployed as separate Fly apps.

### Architecture

```
Browser
  │  HTTPS (Fly.io edge TLS)
  ▼
mytour-frontend  (Docker / nginx, Fly.io app)
  │  /api/* → proxy_pass BACKEND_URL
  ▼
mytour-backend   (Docker / ASP.NET Core, Fly.io app)
  │  EF Core / Npgsql
  ▼
mytour-db        (Fly Postgres)
```

### Prerequisites

- [flyctl](https://fly.io/docs/getting-started/installing-flyctl/) installed and authenticated (`fly auth login`)
- This repository pushed to GitHub (optional, can also deploy from local)

---

### 1 — Create and attach a Fly Postgres database

```bash
# Create a Fly Postgres cluster (free allowance available)
fly postgres create --name mytour-db --region iad
```

### 2 — Deploy the backend

```bash
cd backend/MyTourDataViewer.Api

# Create the Fly app (first time only)
fly apps create mytour-backend

# Set secrets (never committed to source control)
fly secrets set Jwt__Key="$(openssl rand -base64 32)"
fly secrets set CORS_ORIGINS="https://mytour-frontend.fly.dev"

# Attach Fly Postgres – sets ConnectionStrings__Postgres automatically
fly postgres attach mytour-db --app mytour-backend --variable-name ConnectionStrings__Postgres

# Build and deploy
fly deploy
```

> `DbProvider=postgres`, `Jwt__Issuer`, and `Jwt__Audience` are already set in `fly.toml`.  
> The `/health` endpoint is used for Fly.io health checks; the app returns 503 while EF Core migrations run.

### 3 — Deploy the frontend

```bash
cd frontend

# Create the Fly app (first time only)
fly apps create mytour-frontend

# Update BACKEND_URL in fly.toml if your backend app name differs from "mytour-backend"
# Then deploy:
fly deploy
```

> `BACKEND_URL` in `frontend/fly.toml` points to `https://mytour-backend.fly.dev` by default.  
> nginx proxies all `/api/` requests to this URL at container startup.

### 4 — Wire CORS (first deploy only)

Once the frontend app is live, add its URL to the backend's `CORS_ORIGINS` secret:

```bash
fly secrets set CORS_ORIGINS="https://mytour-frontend.fly.dev" --app mytour-backend
```

If you use a custom app name, replace `mytour-frontend` with your actual app name.

---

### Required environment variables

#### Backend secrets (set via `fly secrets set`)

| Variable | Description |
|---|---|
| `Jwt__Key` | JWT signing key — at least 32 random characters. Generate with `openssl rand -base64 32` |
| `ConnectionStrings__Postgres` | PostgreSQL connection string (set automatically by `fly postgres attach`) |
| `CORS_ORIGINS` | Comma-separated list of allowed origins, e.g. `https://mytour-frontend.fly.dev` |

#### Backend env vars (already in `fly.toml`)

| Variable | Value |
|---|---|
| `PORT` | `8080` |
| `DbProvider` | `postgres` |
| `Jwt__Issuer` | `MyTourDataViewer` |
| `Jwt__Audience` | `MyTourDataViewerClients` |

#### Frontend env vars (already in `fly.toml`)

| Variable | Description |
|---|---|
| `PORT` | `8080` — nginx listen port inside the container |
| `BACKEND_URL` | Full `https://` URL of the backend Fly app |

---

### How HTTPS works on Fly.io

Fly.io's edge automatically issues TLS certificates for every `*.fly.dev` app and for custom domains.  
No TLS configuration is needed in the Dockerfiles, nginx, or the ASP.NET Core app.  
`force_https = true` in `fly.toml` ensures HTTP requests are redirected to HTTPS at the edge.

### How backend/frontend communicate

The frontend nginx container starts with `BACKEND_URL=https://mytour-backend.fly.dev`.  
All browser requests to `/api/*` are proxied by nginx to the backend over HTTPS.  
The backend verifies the JWT and processes the request. No direct browser-to-backend calls are made.

### Scaling and cost notes

| Resource | Fly.io free allowance |
|---|---|
| Machines | 3 shared-cpu-1x 256 MB machines |
| Postgres | Not included in free tier — ~$3/mo for the smallest cluster |
| Auto-stop | Enabled by default; machines wake on first request (~1–3 s) |

To disable auto-stop (always-on): set `min_machines_running = 1` in `fly.toml` and redeploy.

---

## Deploy on Render

Render handles HTTPS termination automatically. There are two ways to deploy:

- **Option A — Blueprint (recommended):** one-click, all services provisioned automatically from `render.yaml`.
- **Option B — Manual:** create each service by hand in the Render dashboard.

---

### Option A — Blueprint (one-click)

#### Prerequisites
- A [Render account](https://render.com/)
- This repository pushed to GitHub (or GitLab/Bitbucket)

#### Steps

1. Log in to [dashboard.render.com](https://dashboard.render.com/).
2. Click **New → Blueprint**.
3. Connect your GitHub account and select this repository.
4. Render reads `render.yaml` and shows a preview of the three resources it will create:
   - `mytour-db` — managed PostgreSQL database
   - `mytour-backend` — ASP.NET Core Web API (Docker)
   - `mytour-frontend` — Angular + nginx (Docker)
5. Click **Apply**. Render will:
   - Provision the PostgreSQL database.
   - Build and deploy the backend; EF Core migrations run automatically on first startup.
   - Build and deploy the frontend; nginx is configured at container start to proxy `/api/` to the backend.
   - Wire `CORS_ORIGINS` (backend) and `BACKEND_URL` (frontend) automatically using each service's public `https://` URL.
   - Generate a cryptographically secure random `Jwt__Key`.
6. Wait for both web services to show **Live** (first builds take 3–5 minutes).
7. Open the frontend URL (shown on the `mytour-frontend` service page) and log in with the default credentials.

> **After first login:** change the default admin password immediately via Admin → Users → Change Password.

#### Cost notes (free tier)

| Resource | Free-tier behaviour |
|---|---|
| Web services | Sleep after 15 min of inactivity; 30–60 s cold-start wake-up |
| PostgreSQL | Deleted after 90 days — upgrade to **Basic** ($7/mo) before the trial ends |
| Starter web services | $7/mo each — always-on, no sleep |

---

### Option B — Manual deployment

#### 1 — Create a PostgreSQL database

1. **New → PostgreSQL**
2. Name it `mytour-db`, choose **Free** plan, region **Oregon**.
3. After it's ready, copy the **Internal Database URL** (used in step 2).

#### 2 — Create the backend Web Service

1. **New → Web Service**, connect your repo.
2. Set:

| Setting | Value |
|---|---|
| **Runtime** | Docker |
| **Dockerfile path** | `backend/MyTourDataViewer.Api/Dockerfile` |
| **Docker context** | `backend/MyTourDataViewer.Api` |
| **Port** | `5000` |
| **Health check path** | `/swagger` |

3. Add environment variables:

| Variable | Value |
|---|---|
| `ASPNETCORE_URLS` | `http://+:5000` |
| `DbProvider` | `postgres` |
| `ConnectionStrings__Postgres` | Paste the **Internal Database URL** from step 1 |
| `Jwt__Key` | Generate with `openssl rand -base64 32` — **keep secret** |
| `Jwt__Issuer` | `MyTourDataViewer` |
| `Jwt__Audience` | `MyTourDataViewerClients` |
| `CORS_ORIGINS` | Your frontend URL, e.g. `https://mytour-frontend.onrender.com` (fill in after step 3) |

4. Click **Create Web Service** and wait for **Live**.

#### 3 — Create the frontend Web Service

1. **New → Web Service**, connect your repo.
2. Set:

| Setting | Value |
|---|---|
| **Runtime** | Docker |
| **Dockerfile path** | `frontend/Dockerfile` |
| **Docker context** | `frontend` |
| **Port** | `80` |

3. Add environment variables:

| Variable | Value |
|---|---|
| `BACKEND_URL` | Your backend URL, e.g. `https://mytour-backend.onrender.com` |

> `BACKEND_URL` is injected into the nginx config at container startup; nginx proxies all `/api/` requests to that URL.

4. Click **Create Web Service** and wait for **Live**.

#### 4 — Wire CORS

Once the frontend service URL is known (e.g. `https://mytour-frontend.onrender.com`), go back to the **backend** service → **Environment** and set:

```
CORS_ORIGINS = https://mytour-frontend.onrender.com
```

Then click **Save Changes** (triggers a re-deploy).

---

### Architecture overview

```
Browser
  │  HTTPS
  ▼
mytour-frontend  (Docker / nginx, port 80)
  │  /api/* → proxy_pass BACKEND_URL
  ▼
mytour-backend   (Docker / ASP.NET Core, port 5000)
  │  EF Core / Npgsql
  ▼
mytour-db        (Render managed PostgreSQL)
```

Render issues a free TLS certificate for every service URL automatically — no HTTPS configuration is needed.

---

### Post-deployment checklist

- [ ] Log in with `admin` / `Admin@123456` and **change the default password** immediately.
- [ ] Confirm the dashboard loads data (Admin → API Settings → add an external API).
- [ ] Upgrade the PostgreSQL plan to **Basic** before the 90-day free trial ends.
- [ ] (Optional) Upgrade web services to **Starter** ($7/mo each) to eliminate cold-start delays.

## Configuration

Key settings in `backend/MyTourDataViewer.Api/appsettings.json`:

Also add `Jwt:ExpiryMinutes` to control token lifetime (defaults to `60` minutes).

| Key | Description |
|---|---|
| `DbProvider` | `sqlite` (default) or `postgres` |
| `ConnectionStrings:DefaultConnection` | SQLite path |
| `ConnectionStrings:Postgres` | PostgreSQL connection string |
| `Jwt:Key` | JWT signing key — **change in production** |
| `Jwt:Issuer` | JWT issuer |
| `Jwt:Audience` | JWT audience |
| `Jwt:ExpiryMinutes` | Token lifetime in minutes (default: 60) |

## API endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | Public | Obtain JWT |
| GET | `/api/auth/me` | Any | Current user profile |
| GET | `/api/users` | Admin | List users |
| POST | `/api/users` | Admin | Create user |
| PUT | `/api/users/{id}` | Admin | Update user |
| DELETE | `/api/users/{id}` | Admin | Delete user |
| GET | `/api/apisettings` | Admin | List API configs |
| POST | `/api/apisettings` | Admin | Create API config |
| PUT | `/api/apisettings/{id}` | Admin | Update API config |
| DELETE | `/api/apisettings/{id}` | Admin | Delete API config |
| POST | `/api/apisettings/test` | Admin | Test API connection |
| GET | `/api/dashboard/data` | Any | Fetch external API data |
| GET | `/api/dashboard/apis` | Any | List active API configs |
