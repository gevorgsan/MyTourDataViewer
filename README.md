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
