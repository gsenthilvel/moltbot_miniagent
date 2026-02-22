---
name: mapbox-token-security
description: Security best practices for Mapbox access tokens, including scope management, URL restrictions, rotation strategies, and protecting sensitive data. Use when creating, managing, or advising on Mapbox token security.
---

# Mapbox Token Security Skill

Expert guidance for securing Mapbox access tokens. Covers token types, scope management, URL restrictions, storage practices, rotation strategies, monitoring, and incident response. Based on best practices from the Mapbox security team and the [Mapbox Agent Skills](https://github.com/mapbox/mapbox-agent-skills) repository.

## Use This Skill When

User says things like:

- "How do I secure my Mapbox token?"
- "What token should I use for my app?"
- "My Mapbox token was exposed, what do I do?"
- "How do I restrict token usage by URL?"
- "When should I rotate my Mapbox tokens?"
- "How do I store Mapbox tokens securely?"
- "What scopes does my token need?"

---

## Token Types and When to Use Them

Mapbox provides three token types with different security characteristics:

| Token Type | Prefix | Visibility | Use Case |
|------------|--------|------------|----------|
| **Public** | `pk.*` | Exposed in client-side code | Maps, geocoding, directions in browsers and mobile apps |
| **Secret** | `sk.*` | Server-side only, never exposed | Server-side APIs, batch jobs, sensitive operations |
| **Temporary** | `tk.*` | Short-lived, client or server | Time-limited access, reduced exposure window |

### Public Tokens (`pk.*`)

- **When to use:** Any client-side application (web, iOS, Android) that needs maps, search, geocoding, or directions
- **Always:** Apply URL restrictions and minimal scopes
- **Never:** Use for server-side operations or operations requiring secret capabilities

### Secret Tokens (`sk.*`)

- **When to use:** Backend services, batch geocoding, tile generation, server-side API calls
- **Always:** Store in environment variables or secrets manager; never commit to version control
- **Never:** Expose in client-side code, HTML, JavaScript, or mobile app bundles

### Temporary Tokens (`tk.*`)

- **When to use:** When you need to reduce exposure (e.g., sharing a demo, third-party integration with time limits)
- **Always:** Set appropriate expiration (hours to days)
- **Never:** Use for long-term production integrations; rotate to new temporary tokens before expiry

---

## Scope Management Best Practices

### Principle of Least Privilege

Grant only the scopes required for the specific use case. Each additional scope increases risk if the token is compromised.

### Scope Combinations by Use Case

| Use Case | Recommended Scopes | Avoid |
|----------|-------------------|-------|
| **Display map only** | `styles:tiles`, `styles:read` | `styles:write`, `datasets:*` |
| **Map + Geocoding** | `styles:tiles`, `geocoding:read` | `geocoding:write` |
| **Map + Directions** | `styles:tiles`, `directions:read` | `directions:write` |
| **Map + Search** | `styles:tiles`, `search:read` | `search:write` |
| **Full map app** | `styles:tiles`, `geocoding:read`, `directions:read`, `search:read` | Any `write` or `*` scope |
| **Server-side batch** | `geocoding:read` (or specific) | `styles:*` if not needed |
| **Static map images** | `styles:tiles`, `styles:read` | Unnecessary scopes |

### Scope Naming Convention

- `*:read` = Read-only access
- `*:write` = Modify/create access
- `*` = Full access (avoid unless necessary)

---

## URL Restrictions

### Patterns

Configure URL restrictions in the Mapbox Account dashboard to limit where tokens can be used:

| Pattern Type | Example | Use Case |
|--------------|---------|----------|
| **Exact domain** | `https://myapp.com` | Production app |
| **Subdomain wildcard** | `https://*.myapp.com` | All subdomains (staging, app, etc.) |
| **Path restriction** | `https://myapp.com/map/*` | Specific routes only |
| **Localhost (dev)** | `http://localhost:*` | Local development |

### Multiple Environment Strategy

Create separate tokens per environment with environment-specific URL restrictions:

Production:  pk.prod.xxx  -> https://app.mycompany.com
Staging:     pk.staging.xxx -> https://staging.mycompany.com
Development: pk.dev.xxx   -> http://localhost:*

**Benefits:**

- Compromised dev token cannot be used in production
- Easier to rotate per environment
- Clear audit trail of which environment a token belongs to

---

## Token Storage and Handling

### Server-Side

**Recommended: Environment variables**

```bash
# .env (never commit to git)
MAPBOX_SECRET_TOKEN=sk.eyJ1...
```

```javascript
// Node.js
const token = process.env.MAPBOX_SECRET_TOKEN;
```

```python
# Python
import os
token = os.environ.get('MAPBOX_SECRET_TOKEN')
```

**Recommended: Secrets manager (production)**

- AWS Secrets Manager
- Azure Key Vault
- HashiCorp Vault
- Google Secret Manager

```javascript
// Example: AWS Secrets Manager (pseudo-code)
const secret = await secretsManager.getSecretValue({ SecretId: 'mapbox-token' }).promise();
const token = JSON.parse(secret.SecretString).MAPBOX_SECRET_TOKEN;
```

### Client-Side

**Public tokens only.** Never use secret tokens in client-side code.

```html
<!-- Bad: Hardcoded token -->
<script>
  mapboxgl.accessToken = 'pk.eyJ1...';
</script>
```

```javascript
// Better: Injected at build time from env (still public, but not in source)
mapboxgl.accessToken = process.env.VITE_MAPBOX_TOKEN;
```

```javascript
// Best: Server provides token for authenticated sessions (optional extra layer)
// Backend returns token only after auth; frontend uses it for the session
const response = await fetch('/api/map-token', { credentials: 'include' });
const { token } = await response.json();
mapboxgl.accessToken = token;
```

**Key rules:**

- Use public tokens only
- Apply URL restrictions so token only works on your domains
- Consider serving token from your backend for authenticated users to add an extra control layer

---

## Token Rotation Strategy

### When to Rotate

| Trigger | Action |
|---------|--------|
| **Scheduled** | Every 90 days (recommended) |
| **Suspected compromise** | Immediately |
| **Employee offboarding** | Rotate tokens they had access to |
| **Public exposure** (e.g., in a repo, screenshot) | Immediately |
| **Scope change** | Create new token with new scopes; deprecate old |

### Rotation Process: Zero Downtime

1. **Create** a new token with same scopes and URL restrictions
2. **Deploy** new token to your application (env var, secrets manager)
3. **Verify** application works with new token
4. **Revoke** old token in Mapbox Account dashboard
5. **Monitor** for any missed references to old token

### Emergency Rotation

1. **Revoke** compromised token immediately in Mapbox Account
2. **Create** new token with same (or reduced) scopes
3. **Deploy** new token as fast as possible
4. **Audit** logs to assess impact of compromise
5. **Document** incident and update runbooks

---

## Monitoring and Auditing

### Track Token Usage

- Use Mapbox Account usage dashboards to monitor request volume per token
- Set up alerts for unusual spikes (possible abuse) or drops (possible revocation)
- Correlate with application logs to detect anomalies

### Regular Security Audits

#### Monthly Checklist

- [ ] Review token usage in Mapbox Account
- [ ] Confirm no tokens in version control (search for `pk.` and `sk.`)
- [ ] Verify URL restrictions are still correct
- [ ] Check for any new tokens created without documentation

#### Quarterly Checklist

- [ ] Rotate tokens (zero-downtime process)
- [ ] Audit scope assignments - remove unused scopes
- [ ] Review who has access to token values
- [ ] Update incident response plan if needed
- [ ] Test token rotation in staging

---

## Common Security Mistakes

### 1. Secret Token in Client-Side Code

**Bad:**

```javascript
// NEVER do this
const token = 'sk.eyJ1IjoieW91ci11c2VyIiwiYSI6InRva2VuIn0.xxx';
fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${query}.json?access_token=${token}`);
```

**Good:**

```javascript
// Server-side only
// Client calls your backend; backend uses secret token
app.get('/api/geocode', async (req, res) => {
  const token = process.env.MAPBOX_SECRET_TOKEN;
  const result = await fetch(`https://api.mapbox.com/geocoding/v5/mapbox.places/${req.query.q}.json?access_token=${token}`);
  res.json(await result.json());
});
```

### 2. No URL Restrictions on Public Tokens

**Bad:** Public token with no URL restrictions - usable from any website if leaked.

**Good:** Restrict to your domains: `https://yourdomain.com`, `https://*.yourdomain.com`

### 3. Overly Broad Scopes

**Bad:** Token with `datasets:read`, `datasets:write`, `styles:write` when you only need to show a map.

**Good:** Token with only `styles:tiles`, `styles:read` for map display.

### 4. Tokens in Version Control

**Bad:**

```bash
# .env committed to git
MAPBOX_TOKEN=pk.eyJ1...
```

**Good:**

```bash
# .env in .gitignore
echo ".env" >> .gitignore
# Use .env.example with placeholders
# MAPBOX_TOKEN=pk.your_token_here
```

### 5. Single Token for All Environments

**Bad:** One token used in dev, staging, and production.

**Good:** Separate tokens per environment with environment-specific URL restrictions.

---

## Incident Response Plan

### If a Token Is Exposed

1. **Revoke immediately** in Mapbox Account -> Access tokens -> Revoke
2. **Create replacement** token with same or reduced scopes
3. **Deploy** new token to affected systems
4. **Assess impact:** Check usage logs for abnormal requests
5. **Notify** stakeholders if customer data or systems were at risk
6. **Document** the incident, root cause, and remediation
7. **Update** processes to prevent recurrence (e.g., pre-commit hooks to block token commits)

### If You Suspect Abuse

1. **Review** Mapbox usage dashboard for unusual patterns
2. **Revoke** token if abuse is confirmed
3. **Rotate** to new token and tighten URL restrictions and scopes
4. **Monitor** new token for continued abuse

---

## Best Practices Summary Checklist

- [ ] Use **public** tokens (`pk.*`) for client-side; **secret** (`sk.*`) for server-side only
- [ ] Apply **Principle of Least Privilege** - minimal scopes per use case
- [ ] Set **URL restrictions** on all public tokens
- [ ] Use **separate tokens** per environment (dev, staging, prod)
- [ ] Store secret tokens in **environment variables** or **secrets manager** - never in code
- [ ] Add `.env` to `.gitignore`; use `.env.example` with placeholders
- [ ] **Rotate** tokens every 90 days (or per policy)
- [ ] **Revoke** immediately if exposed or compromised
- [ ] **Monitor** token usage; set up alerts for anomalies
- [ ] **Audit** monthly and quarterly per checklists above
- [ ] Document token purpose and owners in your team wiki or runbook
