# RecipeVault — Automated QA Results

**Date:** 2026-02-19  
**Time:** 08:50 MST (15:50 UTC)  
**Target:** https://myrecipevault.io  
**Method:** web_fetch + PowerShell Invoke-WebRequest (no browser, no auth)

---

## Summary

| Category | Passed | Failed | Warnings |
|---|---|---|---|
| API Health Checks | 1 | 1 | 1 |
| Page Load Tests | 2 | 0 | 1 |
| Security Headers | 6 | 0 | 3 |
| Performance | 5 | 0 | 0 |
| **Total** | **14** | **1** | **5** |

---

## 1. API Health Checks

### GET /api/health

| Attempt | Status | Result | Notes |
|---|---|---|---|
| Cold start (~2s uptime) | 503 | ⚠️ WARN | Checks not yet cached |
| Warm (~49s uptime) | 200 | ✅ PASS | Both checks healthy |

**Response body (warm):**
```json
{
  "host": "9185594da25708",
  "service": "recipevault-api",
  "version": "1.0.0-local",
  "healthy": true,
  "status": "ok",
  "uptime": "P0Y0M0DT0H0M49S",
  "checks": {
    "recipevault-db": { "healthy": true, "status": "ok", "statusDetail": "PostgreSQL 17.6 on aarch64-unknown-linux-gnu" },
    "gemini":         { "healthy": true, "status": "ok", "statusDetail": "Gemini API reachable" }
  }
}
```

**Content-Type:** `application/json; charset=utf-8` ✅  
**Response time:** 153ms ✅

> ⚠️ **Issue (Cold Start):** On first hit after container start, the service returns HTTP 503 with `healthy: false` and `"statusDetail": "status not cached"`. Health check polling hasn't run yet. This will fail any uptime monitor that hits the endpoint immediately after a deploy/restart. Consider adding a warmup check or a small startup delay before marking the service ready.

---

### GET /api/v1/version

| Status | Result | Notes |
|---|---|---|
| 400 Bad Request | ❌ FAIL | Endpoint returns `Bad Request` with no body/content-type |

**Expected:** JSON `{ "version": "1.0.0" }`  
**Actual:** HTTP 400, empty body, no Content-Type header  
**Response time:** 142ms

> ❌ **Issue:** `/api/v1/version` is not working. Either the endpoint doesn't exist at this path, requires query parameters, or requires an `Accept` header. Needs investigation.

---

### Version String

The `/api/health` response includes `"version": "1.0.0-local"` and `"tag": "local"`. The build metadata is not being injected during Fly.io deploy — the build timestamp is `0001-01-01T00:00:00` (zero value). Build args (`--build-arg VERSION=...` etc.) may not be set in `fly.toml` or the CI/CD pipeline.

---

## 2. Page Load Tests

### GET / (Root)

| Check | Result | Detail |
|---|---|---|
| HTTP status | ✅ PASS | 200 OK |
| Content-Type | ✅ PASS | `text/html` |
| Title | ✅ PASS | "RecipeVault - Your Personal Recipe Collection" |
| Response time | ✅ PASS | 146ms |

HTML confirms Angular SPA with SSR pre-rendering (`data-beasties-container`).

---

### GET /login

| Check | Result | Detail |
|---|---|---|
| HTTP status | ✅ PASS | 200 OK |
| Response time | ✅ PASS | 108ms |
| Login form elements | ⚠️ WARN | SPA — form rendered client-side via JS, not in static HTML |

> **Note:** This is an Angular SPA. All route rendering (including login form elements) happens in the browser. Static HTML only contains the app shell. Login form presence cannot be verified without a headless browser.

---

### GET /recipes (Protected Route Check)

| Check | Result | Detail |
|---|---|---|
| HTTP status | ⚠️ WARN | 200 OK — no server-side redirect |
| Location header | — | None (expected for SPA) |
| Auth redirect | ⚠️ WARN | Handled client-side only |

> **Note:** `/recipes` returns HTTP 200 with the same SPA shell. Route guarding is handled by Angular's client-side auth guards. This is standard SPA behavior — a server-side 302 redirect is not expected. However, unauthenticated users *could* receive the app bundle. To verify the Angular guard actually redirects to `/login`, a browser test is needed.

---

## 3. Security Headers

Tested on both `https://myrecipevault.io/` and `https://myrecipevault.io/api/health`.

| Header | Value | Result |
|---|---|---|
| `X-Frame-Options` | `DENY` | ✅ PASS |
| `X-Content-Type-Options` | `nosniff` | ✅ PASS |
| `Strict-Transport-Security` | `max-age=2592000` (30 days) | ✅ PASS (⚠️ short) |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | ✅ PASS |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` | ✅ PASS |
| `Content-Security-Policy` | **MISSING** | ⚠️ WARN |
| `X-XSS-Protection` | **MISSING** | ⚠️ INFO |
| `Access-Control-Allow-Origin` | **MISSING** | ✅ OK (API not cross-origin) |

**Content-Type on API responses:** `application/json; charset=utf-8` ✅ — correct.

### Header Notes

- **CSP Missing:** No `Content-Security-Policy` header on any response. This is a moderate security gap — CSP protects against XSS and data injection attacks. Recommended to add at minimum `default-src 'self'` with appropriate exceptions for fonts/CDN.
- **HSTS max-age:** 30 days (`2592000s`) is acceptable but OWASP recommends at minimum 1 year (`31536000`). Consider bumping and adding `includeSubDomains; preload` once confident in HTTPS-only setup.
- **X-XSS-Protection:** Deprecated in modern browsers (superseded by CSP) — missing is acceptable, but some scanners flag it.
- **CORS:** No `Access-Control-Allow-Origin` header on the API — appropriate if the API is not intended to be called cross-origin from external domains.

---

## 4. Performance

All timings measured from local Windows host to Fly.io container.

| Endpoint | Response Time | Result |
|---|---|---|
| `GET /api/health` | 153ms | ✅ PASS |
| `GET /api/v1/version` | 142ms | ✅ PASS (returns 400, but fast) |
| `GET /` | 146ms | ✅ PASS |
| `GET /login` | 108–192ms | ✅ PASS |
| `GET /recipes` | 122–213ms | ✅ PASS |

All endpoints respond well under the 3-second threshold. Network latency (local → Fly.io) likely accounts for 50–100ms of each figure.

---

## Issues Summary

| Severity | Issue |
|---|---|
| 🔴 HIGH | `/api/v1/version` returns **HTTP 400** — endpoint broken |
| 🟡 MEDIUM | **Cold start 503** on `/api/health` — uptime monitors will false-alarm after deploys |
| 🟡 MEDIUM | **No Content-Security-Policy** header — XSS exposure |
| 🟠 LOW | Build metadata not injected — version shows `1.0.0-local`, timestamp is zero |
| 🟠 LOW | HSTS `max-age` is 30 days — consider increasing to 1 year |
| ℹ️ INFO | Client-side-only auth guards — server returns 200 on `/recipes` (expected for SPA) |

---

## Recommendations

1. **Fix `/api/v1/version`** — verify the route exists, check for required headers/params, add to integration tests.
2. **Add health check readiness delay** — use a `/api/ready` vs `/api/health` split (readiness vs liveness), or configure Fly.io health check with an `initial_delay_seconds` to avoid cold-start false failures.
3. **Add CSP header** — even a basic policy adds meaningful XSS protection.
4. **Inject build metadata in CI/CD** — pass `VERSION`, `GIT_SHA`, and `BUILD_TIMESTAMP` as Docker build args so the health endpoint reflects the actual deployed version.
5. **Increase HSTS max-age** — bump to `31536000` (1 year) and consider `preload`.

---

*Generated by automated QA run — no browser, no test account required.*
