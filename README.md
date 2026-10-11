# School PlayGround ERP (AY 2026–27)

Multi-tenant K-12 administrative portal MVP: ASP.NET Core 8 API, PostgreSQL, and Next.js admin UI aligned with the School PlayGround ERP PRD.

## Stack

| Layer | Technology |
|-------|------------|
| API | ASP.NET Core 8, JWT, FluentValidation, Swagger |
| Data | PostgreSQL 16, EF Core 8 |
| UI | Next.js 15 (App Router), Tailwind, Recharts, TanStack Table |

## Quick start

### 1. Database

```bash
docker compose up -d
```

### 2. API

```bash
cd d:\SchoolERP
dotnet run --project src/Api
```

Swagger: http://localhost:5080/swagger

On first run (Development), the API applies migrations and seeds demo data.

### 3. Web client

```bash
cd client
cp .env.local.example .env.local
npm run dev
```

Open http://localhost:3000 and sign in.

## Demo accounts

| Email | Role | Password |
|-------|------|----------|
| principal@school.local | Principal | Demo@123 |
| accountant@school.local | Accountant | Demo@123 |
| academic@school.local | AcademicCoordinator | Demo@123 |

**Tenant header (dev):** `X-Tenant-Id: 11111111-1111-1111-1111-111111111111`

## PRD surfaces

| Route | Description |
|-------|-------------|
| `/dashboard` | KPI tiles, inflow chart, wing attendance, collections ticker, approval queue |
| `/finance/fees` | Institutional totals, invoice table, POS quick-collect |
| `/students/STU-2026-0842` | Student 360 (masked Aadhaar, attendance grid, ledger) |
| `/academics/timetable` | Class X-A grid, quota chips, conflicts, substitution drawer |

## API endpoints (v1)

- `GET /api/v1/analytics/dashboard-summary`
- `GET /api/v1/students/{admission_no}/360`
- `GET /api/v1/finance/fees/summary` · `GET /api/v1/finance/fees/invoices`
- `POST /api/v1/finance/pos/collect`
- `GET /api/v1/academics/timetable/{class_id}`
- `POST /api/v1/academics/substitutions/allocate`

## Seed data notes

Dashboard enrollment KPIs use a `DashboardSnapshot` row (3,420 students) while the database holds a **representative cohort** (~25 students) for fast local dev. Fee and attendance detail for the hero student `STU-2026-0842` / `INV-2026-4412` is fully seeded.

## Tests

```bash
dotnet test tests/SchoolERP.Api.Tests
```

Default: in-memory EF database (no Docker). For Testcontainers + PostgreSQL:

```bash
set SCHOOLERP_USE_TESTCONTAINERS=1
dotnet test tests/SchoolERP.Api.Tests
```

## Persona checklist

- **Principal:** Dashboard approvals, revenue KPIs, sign-in as `principal@school.local`.
- **Accountant:** Fees center POS collect, waiver toggle, invoice filters.
- **Academic coordinator:** Timetable conflicts, substitute allocation (`academic@school.local`).
- **Admissions / class teacher:** Student 360 profile, attendance grid, appraisal.

## Phase 2 (out of MVP)

Live payment gateways, biometric devices, SMS gateway, read replicas / offline POS sync.
