# Employee Management System Fullstack

A production-style fullstack employee management application built with **Angular** and **ASP.NET Core Web API**, featuring JWT authentication with refresh-token rotation, role-based authorization, and a clean layered architecture.

---

## Overview

This project manages employee records behind an authenticated, role-aware interface. It was built to demonstrate **engineering practice**, not just features: separation of concerns, dependency inversion, centralized error handling, and defence-in-depth validation across the stack.

**Core capabilities**

- User registration and login with hashed passwords
- Short-lived JWT access tokens with silent, automatic renewal
- Long-lived refresh tokens stored in `HttpOnly` cookies with rotation on every use
- Full employee CRUD with server-enforced uniqueness rules
- Role-based authorization (`Admin` vs `User`) enforced on the API and reflected in the UI

---

## Tech Stack

### Backend

| Technology | Purpose |
|---|---|
| **ASP.NET Core Web API** (.NET 10) | REST API framework |
| **Entity Framework Core 10** | ORM and data access |
| **SQL Server** | Relational database |
| **EF Core Fluent API** | Relationship, index, and constraint configuration |
| **EF Core Migrations** | Versioned schema management |
| **FluentValidation** | Request validation as testable rule classes |
| **BCrypt.Net-Next** | Password hashing |
| **JWT Bearer Authentication** | Stateless access-token auth |
| **Swashbuckle / OpenAPI** | Interactive API documentation |

### Frontend

| Technology | Purpose |
|---|---|
| **Angular 20** | SPA framework |
| **Standalone Components** | Modern module-free component architecture |
| **Angular Signals** | Reactive state management |
| **Reactive Forms** | Typed forms with client-side validation |
| **HTTP Interceptors** | Automatic token attachment and refresh |
| **Route Guards** | Client-side route protection |
| **RxJS** | Async stream handling |
| **TypeScript** | End-to-end type safety |

---

## Key Features

### Security

- **Password hashing** with BCrypt — plaintext passwords are never stored
- **Refresh token in `HttpOnly` cookie** — inaccessible to JavaScript, immune to XSS token theft
- **Refresh token rotation** — every refresh revokes the old token and issues a new one, limiting the blast radius of a stolen token
- **Access token kept short-lived** (15 minutes) to minimise exposure
- **No user enumeration** — invalid email and invalid password return an identical message
- **Privilege escalation prevented** — registration always assigns the least-privileged role; `Admin` cannot be self-selected
- **CORS with least-privilege** — a single allowed origin, an explicit method list, and an explicit header list
- **Server-side authorization as source of truth** — hiding a button in the UI never secures an endpoint

### Data Integrity

- **Unique index on employee email** — enforced at the database level
- **Filtered unique index on phone** (`WHERE Phone IS NOT NULL`) — allows many employees without a phone, but no two sharing one
- **Blank-to-`NULL` normalization** — empty strings from forms are converted to `NULL` so the filtered index behaves correctly
- **Two-layer uniqueness** — a service pre-check returns a friendly `409`, while the database index remains the race-condition-proof guarantee
- **Cascade delete** — removing a user automatically removes their refresh tokens

### Developer Experience

- **Global exception middleware** — controllers contain zero `try/catch` blocks
- **Consistent error contract** — every failure returns the same `ErrorResponse` shape
- **Interactive API docs** at `/swagger`
- **Typed contracts end-to-end** — TypeScript interfaces mirror C# DTOs one-to-one

---

## Architecture

The backend follows a layered architecture with dependency inversion at each boundary.

```
┌──────────────────────────────────────────────────────────┐
│  Controllers          Thin. HTTP only. No business logic. │
│                       Translate results → status codes.   │
└────────────────────────────┬─────────────────────────────┘
                             │ depends on interface
┌────────────────────────────▼─────────────────────────────┐
│  Services (IEmployeeService, IAuthService, ITokenService) │
│                       Business rules. Validation.         │
│                       Framework-agnostic — no HTTP.       │
└────────────────────────────┬─────────────────────────────┘
                             │
┌────────────────────────────▼─────────────────────────────┐
│  ApplicationDbContext (EF Core)                           │
│                       Unit of Work + Repository built in. │
└────────────────────────────┬─────────────────────────────┘
                             │
┌────────────────────────────▼─────────────────────────────┐
│  Entities → SQL Server                                    │
└──────────────────────────────────────────────────────────┘

  Cross-cutting: ExceptionHandlingMiddleware wraps the whole pipeline.
```

### Dependency Injection

Every dependency is injected through the constructor against an **interface**, never a concrete type:

```csharp
public class EmployeeService(
    ApplicationDbContext context,
    IValidator<CreateEmployeeRequest> createValidator,
    IValidator<UpdateEmployeeRequest> updateValidator) : IEmployeeService
```

Registered in `Program.cs` with the correct lifetimes:

```csharp
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
```

`AddScoped` is used because these services depend on `DbContext`, which is itself scoped to a single HTTP request.

### Where HTTP status codes come from

Services are **HTTP-agnostic**. They return data, return `null`, or **throw a domain exception**. The middleware performs the translation:

| Thrown by | Exception | HTTP Response |
|---|---|---|
| FluentValidation | `ValidationException` | `400 Bad Request` (with field errors) |
| Service | `NotFoundException` | `404 Not Found` |
| Service | `ConflictException` | `409 Conflict` |
| Service | `UnauthorizedException` | `401 Unauthorized` |
| EF Core | `DbUpdateException` (unique violation) | `409 Conflict` |
| Anywhere | Anything unexpected | `500` — details never leaked |

This keeps services reusable outside a web context (a background job, a console app, a gRPC endpoint) and leaves controllers completely free of error-handling noise.

---

## Folder Structure

```
employee-management-fullstack/
│
├── backend/
│   ├── backend.sln
│   └── backend/
│       ├── Controllers/
│       │   ├── AuthController.cs           # register, login, refresh, logout
│       │   └── EmployeeController.cs       # CRUD, [Authorize(Roles="Admin")]
│       │
│       ├── Data/
│       │   ├── ApplicationDbContext.cs     # DbSets + Fluent API configuration
│       │   └── DbSeeder.cs                 # seeds the default admin on startup
│       │
│       ├── Entities/
│       │   ├── BaseEntity.cs               # Id, CreatedAt, UpdatedAt
│       │   ├── Employee.cs
│       │   ├── User.cs                     # stores PasswordHash, never a password
│       │   └── RefreshToken.cs             # + computed IsActive
│       │
│       ├── Enums/
│       │   └── UserRole.cs                 # Admin, User
│       │
│       ├── DTOs/
│       │   ├── Auth/
│       │   │   ├── RegisterRequest.cs
│       │   │   ├── LoginRequest.cs
│       │   │   ├── AuthResponse.cs         # access token only — refresh is a cookie
│       │   │   └── AuthResult.cs           # internal service → controller carrier
│       │   ├── Employee/
│       │   │   ├── CreateEmployeeRequest.cs
│       │   │   ├── UpdateEmployeeRequest.cs
│       │   │   └── EmployeeResponse.cs
│       │   └── Common/
│       │       └── ErrorResponse.cs        # one shape for every error
│       │
│       ├── Validators/
│       │   ├── RegisterRequestValidator.cs
│       │   ├── LoginRequestValidator.cs
│       │   ├── CreateEmployeeRequestValidator.cs
│       │   └── UpdateEmployeeRequestValidator.cs
│       │
│       ├── Services/
│       │   ├── Interfaces/
│       │   │   ├── IAuthService.cs
│       │   │   ├── IEmployeeService.cs
│       │   │   └── ITokenService.cs
│       │   └── Implementations/
│       │       ├── AuthService.cs          # register, login, refresh + rotation
│       │       ├── EmployeeService.cs      # CRUD + uniqueness rules
│       │       └── TokenService.cs         # JWT + refresh token generation
│       │
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       │
│       ├── Exceptions/
│       │   └── AppException.cs             # Conflict / NotFound / Unauthorized
│       │
│       ├── Options/
│       │   └── JwtSettings.cs              # strongly-typed configuration
│       │
│       ├── Swagger/
│       │   └── AuthorizeCheckOperationFilter.cs
│       │
│       ├── Migrations/
│       ├── appsettings.json
│       └── Program.cs                      # DI, CORS, JWT, pipeline, seeding
│
└── frontend/
    └── src/
        ├── app/
        │   ├── core/                       # app-wide singletons
        │   │   ├── config/
        │   │   │   └── api.config.ts        # single source for the base URL
        │   │   ├── models/                  # TypeScript mirrors of the C# DTOs
        │   │   │   ├── auth/
        │   │   │   │   ├── login-request.model.ts
        │   │   │   │   ├── register-request.model.ts
        │   │   │   │   └── auth-response.model.ts
        │   │   │   └── employee/
        │   │   │       ├── employee.model.ts
        │   │   │       ├── create-employee-request.model.ts
        │   │   │       └── update-employee-request.model.ts
        │   │   ├── services/
        │   │   │   ├── auth.service.ts      # login/register/refresh/logout + signals
        │   │   │   └── employee.service.ts  # typed CRUD calls
        │   │   ├── interceptors/
        │   │   │   └── auth.interceptor.ts  # attaches JWT, auto-refreshes on 401
        │   │   └── guards/
        │   │       ├── auth.guard.ts        # must be logged in
        │   │       └── admin.guard.ts       # must be an Admin
        │   │
        │   ├── features/                    # business screens
        │   │   ├── auth/
        │   │   │   ├── login/
        │   │   │   └── register/
        │   │   └── employees/
        │   │       ├── employee-list/       # the dashboard table
        │   │       ├── add-employee/
        │   │       └── edit-employee/
        │   │
        │   ├── shared/                      # reusable UI
        │   │   ├── navbar/                  # auth-aware links, role badge, logout
        │   │   ├── footer/
        │   │   └── confirm-dialog/          # reusable modal (delete + logout)
        │   │
        │   ├── app.ts / app.html / app.css  # root shell: navbar + outlet + footer
        │   ├── app.config.ts                # providers: router, HttpClient, interceptor
        │   └── app.routes.ts                # lazy-loaded routes with guards
        │
        └── styles.css                       # global reset and base theme
```

---

## Authentication Flow

Two tokens, each with a distinct job and storage strategy.

|  | Access Token | Refresh Token |
|---|---|---|
| **Purpose** | Prove identity on every API call | Obtain a new access token |
| **Lifespan** | 15 minutes | 7 days |
| **Stored in** | `localStorage` | `HttpOnly` cookie |
| **Stored in the DB?** | No (stateless) | Yes (so it can be revoked) |
| **Sent with** | Every protected request | Only `/auth/refresh` |

### Login

```
Client  ──  POST /auth/login  { email, password }   (withCredentials: true)
Server  ──  verify password with BCrypt
        ──  generate access token (JWT, 15 min) + refresh token (random, 7 days)
        ──  persist the refresh token, clean up the user's stale tokens
        ──  Set-Cookie: refreshToken=...; HttpOnly; Secure; SameSite=None
        ──  200 { accessToken, email, role }

Client  ──  store accessToken + role in localStorage
        ──  the refresh cookie is held by the browser; JavaScript cannot read it
```

### Silent refresh

```
Client  ──  GET /api/employee   (Authorization: Bearer <expired token>)
Server  ──  401

Interceptor catches the 401
        ──  POST /auth/refresh   (browser attaches the cookie automatically)
Server  ──  read the token from the cookie, validate it
        ──  ROTATE: revoke the old token, issue a new one
        ──  Set-Cookie: refreshToken=<new>
        ──  200 { accessToken, email, role }

Interceptor stores the new access token and retries the original request
        ──  GET /api/employee → 200

The user sees nothing.
```

### Session lifetime

Because the refresh token rotates on every use, its 7-day clock **slides forward** for an active user — they remain logged in indefinitely. A user who is away for more than 7 days has their refresh token expire and must authenticate again.

### CORS

Configured with **least privilege** — a single named origin, an explicit method list, an explicit header list, and credentials enabled. `AllowAnyOrigin()` is deliberately not used, because browsers forbid combining a wildcard origin with credentials.

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins("https://localhost:4200")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();
    });
});
```

`app.UseCors(...)` is placed **before** `UseAuthentication()` in the pipeline.

---

## Roles & Permissions

The user's role is embedded as a claim in the JWT at login, so authorization requires no database lookup on each request.

| Action | User | Admin |
|---|---|---|
| Register / Login | ✅ | ✅ |
| View employee list | ✅ | ✅ |
| View a single employee | ✅ | ✅ |
| Add employee | ❌ | ✅ |
| Edit employee | ❌ | ✅ |
| Delete employee | ❌ | ✅ |

**Enforcement is layered:**

- **Backend** — `[Authorize(Roles = "Admin")]` on create, update, and delete. A non-admin receives **`403 Forbidden`** — distinct from `401 Unauthorized`, because they *are* authenticated, just not permitted.
- **Frontend** — restricted buttons are rendered **disabled with an explanatory tooltip**, an informational banner explains view-only access, and `adminGuard` blocks direct URL navigation.

The UI restrictions exist for **user experience**. The API is the **security boundary**. Hiding a button never secures an endpoint.

---

## API Reference

Base URL: `https://localhost:7270/api`

### Auth

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/auth/register` | — | Create an account (always role `User`) |
| `POST` | `/auth/login` | — | Authenticate; returns an access token, sets the refresh cookie |
| `POST` | `/auth/refresh` | Cookie | Rotate the refresh token, issue a new access token |
| `POST` | `/auth/logout` | — | Clear the refresh cookie |

### Employees

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/employee` | Any authenticated | List all employees (ordered by name) |
| `GET` | `/employee/{id}` | Any authenticated | Get a single employee |
| `POST` | `/employee` | **Admin** | Create — returns `201` with a `Location` header |
| `PUT` | `/employee/{id}` | **Admin** | Update — full replace |
| `DELETE` | `/employee/{id}` | **Admin** | Delete — returns `204 No Content` |

### Status codes

| Code | Meaning |
|---|---|
| `200` | Success |
| `201` | Created |
| `204` | Deleted, no body |
| `400` | Validation failure |
| `401` | Not authenticated / token invalid or expired |
| `403` | Authenticated but not permitted (wrong role) |
| `404` | Resource not found |
| `409` | Conflict (duplicate email or phone) |
| `500` | Unexpected server error |

Interactive documentation is available at **`https://localhost:7270/swagger`** when running in development.

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 18+
- SQL Server (LocalDB, Express, or full)
- Angular CLI — `npm install -g @angular/cli`

The default admin is seeded automatically on first run.

## Git Workflow

The project follows a **three-tier branching model**, with every merge performed via command line.

```
main                    stable, release-ready
  └── develop           integration branch
        └── feature/*   one branch per unit of work
```

