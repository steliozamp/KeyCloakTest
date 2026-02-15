# Keycloak Auth Demo (.NET 10)

Production-style authentication and authorization demo using:
- ASP.NET Core Web API (`net10.0`)
- Keycloak in Docker
- Postgres for Keycloak persistence
- JWT bearer validation with policy-based authorization (roles + scopes)

## Architecture

```text
+----------------------+          +-----------------------+
|   Client / Swagger   |          |      Service Client   |
|  (Auth Code + PKCE)  |          | (Client Credentials)  |
+----------+-----------+          +-----------+-----------+
           |                                  |
           | tokens                           | tokens
           v                                  v
+----------------------+     validates     +------------------------+
|      Keycloak        +------------------>+   ASP.NET Core API     |
|  realm: auth-demo    |                   |  JWT + policies + CORS |
+----------+-----------+                   +------------+-----------+
           |                                             |
           | persists realm/users/clients                |
           v                                             v
+----------------------+                         +-------------------+
|       Postgres       |                         | Protected APIs    |
+----------------------+                         +-------------------+
```

## What Is Included

- Keycloak realm import (`auth-demo`) with:
  - Roles: `user`, `manager`, `admin`
  - Scopes: `api.read`, `api.write`
  - Users: `alice`, `bob`, `carol`
  - Clients:
    - `auth-demo-api` (API audience)
    - `auth-demo-swagger` (public, auth code + PKCE)
    - `auth-demo-service-read` (client credentials, read scope)
    - `auth-demo-service-write` (client credentials, read/write scopes)
- API security:
  - JWT Bearer auth
  - Policy-based authorization by role and scope
  - Keycloak role mapping from `realm_access` and `resource_access`
- Operational concerns:
  - Health checks (`/health/live`, `/health/ready`)
  - Rate limiting on write endpoint
  - CORS policy
  - Structured JSON logging + correlation id header (`X-Correlation-ID`)
  - ProblemDetails for consistent errors

## Quick Start

1. Start services:

```bash
docker compose up -d --build
```

2. Open Keycloak Admin Console:

```text
http://localhost:8080/admin
```

3. Use admin credentials from `.env` (demo defaults):
- Username: `admin`
- Password: `admin123!`

4. Open API Swagger:

```text
http://localhost:5083/swagger
```

## Endpoint Authorization Matrix

| Endpoint | Method | Requirement |
|---|---|---|
| `/api/demo/public` | GET | Anonymous |
| `/api/demo/me` | GET | Authenticated token |
| `/api/demo/reports` | GET | `api.read` scope OR `admin` role |
| `/api/demo/reports` | POST | `api.write` scope OR `admin` role (+ rate limit) |
| `/api/demo/admin` | GET | `admin` realm role |

## Token Examples

### 1) Read-only client credentials token

```bash
curl -X POST "http://localhost:8080/realms/auth-demo/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=auth-demo-service-read&client_secret=demo-read-secret"
```

### 2) Write-capable client credentials token

```bash
curl -X POST "http://localhost:8080/realms/auth-demo/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=auth-demo-service-write&client_secret=demo-write-secret"
```

### 3) Demo user password grant (local demo only)

```bash
curl -X POST "http://localhost:8080/realms/auth-demo/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=auth-demo-swagger&username=carol&password=carol123&scope=openid profile email api.read api.write"
```

Use the `access_token` as:

```bash
curl "http://localhost:5083/api/demo/reports" -H "Authorization: Bearer <TOKEN>"
```

## Local Request File

Use `KeyCloakTest/KeyCloakTest.http` for ready-to-run requests in sequence.

## Troubleshooting

- Realm import not applied:
  - Stop and remove volumes, then restart:
  - `docker compose down -v && docker compose up -d --build`
- 401 from protected endpoints:
  - Check issuer is `http://localhost:8080/realms/auth-demo`
  - Check `aud` includes `auth-demo-api`
- 403 from write/admin endpoints:
  - Token authenticated but missing scope/role

## Security Notes

- This is a demo with non-production secrets and `start-dev` Keycloak mode.
- For production, use HTTPS, secret management, hardened Keycloak config, and stronger token/session policies.
