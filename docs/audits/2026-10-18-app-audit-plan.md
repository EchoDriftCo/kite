# Audit plan for the rest of the app

Written 2026-10-18, after the backbone audit (PRs #4–#7).

Each section works the way the backbone audit did:

1. List the code in scope.
2. Read it.
3. Prove each suspected problem with a test, a database query or a log, before calling it a finding.
4. Report the findings by severity, each with a fix.
5. Open one focused PR, with tests that fail before the fix.
6. Give the owner a separate list of settings only they can change.

The leads below are where to look first; none are confirmed findings yet.

| # | Section | Scope | Leads to check first |
|---|---------|-------|----------------------|
| 0 | Database exposure (now) | Supabase grants, RLS, Data API, auth settings | The 26 open tables (locked down by hand on 2026-10-18 and in the `LockDownPublicSchemaAccess` migration, PR #7; confirm the live state). Supabase's performance advisor (missing indexes on foreign keys). |
| 1 | Authorization and data isolation | 24 controllers, all domain services | Can user A read or edit user B's recipe, collection, meal plan, cooking log, API token or circle by ID? How are admin endpoints (`AdminSubjectIds`), share tokens and guest restrictions enforced? `RequiresTier` currently lets everyone through. |
| 2 | Outbound calls and cost control | Import (URL, HTML, video), Gemini, USDA, image storage, email | SSRF: the URL importer fetches whatever URL a user gives it, which could reach Fly's internal network. Command injection in the video downloader. Prompt injection from imported pages. Upload size and type checks. Invite-email spam. 180-second Gemini timeouts. Gemini spend per user. |
| 3 | Data layer and migrations | Data, Domain, the EF migrations and the hand-run SQL in `/migrations` | Drift between the EF model, the hand-run SQL and the live database. Delete rules (Restrict vs Cascade). Queries that load everything or run once per row. Missing pagination. `SeedUnitData` and the empty `Unit` table. |
| 4 | Backbone leftovers | Items 5, 7, 8 and 11–18 from the backbone audit | Duplicate CORS middleware, the two config-naming schemes and duplicate Fly secrets, database certificate verification, API keys in URLs, Sentry, Swagger, CSP. Mostly small, but several need settings changed alongside the code. |
| 5 | Business logic | Import pipeline, substitutions, meal plans, grocery checkout links, nutrition, dietary conflicts | Correctness against real data. Error handling: does `MessageExceptionResponseFilter` leak internal details? Concurrency in the import jobs. |
| 6 | API contract | Facade, DTOs, models, mappers | Over-posting (requests setting fields they shouldn't), validation gaps, consistent errors, response size. |
| 7 | Frontend | 14.8k lines of Angular | Guest-to-account conversion, where tokens are stored, XSS (`innerHTML`, `bypassSecurityTrust`), whether the CSP can be tightened, the dev and prod environment configs, the `feature/pwa` service worker branch. |
| 8 | Operations and privacy | Logging, Fly, Supabase | Emails written to logs (e.g. "invite sent to {Email}"). Fly machines run in dfw and iad, but the database is in us-west-2, so every query crosses the country. Backups and point-in-time recovery. Machine count against cost. |
| 9 | Tests and repo hygiene | Test projects, scripts, docs, branches | Every integration test is discovered twice. The Karma UI suite doesn't compile. `Data.Tests` has no tests. E2E has never run. Root scripts (`deploy.ps1`, `check-schema.sh`) and docs that no longer match the code. Stale branches. |

## Progress

| # | Status | Report | PR |
|---|--------|--------|----|
| 0 | Findings reported; fix merged | [00-database-exposure.md](00-database-exposure.md) | #8 |
| 1 | Findings reported; fix PR open | [01-authorization.md](01-authorization.md) | audit/authorization |
| 2–9 | Not started | | |

Section 0 found that the EF migrations can't build a fresh database (see its report); Section 3 should start there.

Section 1 left leads for Sections 2, 3, 4 and 5 (unused `Api` rate-limit policy, the ingredient-search SQL function that only exists in `docs/migrations`, controllers that turn "not found" into a 500 with the exception message); see "Found in passing" in its report.
