# Generating development keys

The API signs JWTs with **RS256** and encrypts custom API keys with **AES-256-CBC**.
Both keys are configured in `src/MyAi.Api/appsettings.json` (or user-secrets / env vars).

If you leave `Jwt:PrivateKeyBase64` and `Jwt:PrivateKeyPath` empty, the app generates an
**ephemeral** RSA key at startup (fine for local dev — tokens invalidate on restart, a
warning is logged). For a stable local key or production, generate one:

## 1. RSA private key (JWT signing)

```bash
# PKCS#8 PEM
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out myai-jwt.pem

# Option A — PEM file: point Jwt:PrivateKeyPath at the file
# Option B — inline: base64 the DER body into Jwt:PrivateKeyBase64
openssl pkcs8 -topk8 -nocrypt -in myai-jwt.pem | openssl base64 -A
```

The same key is used by the token generator **and** the JWT bearer validator
(see `JwtSigningKeyProvider`), so both stay in sync automatically.

## 2. AES-256 encryption key (custom AI API keys at rest)

```bash
openssl rand -base64 32
```

Put the value in `Jwt:EncryptionKeyBase64`. It must decode to exactly 32 bytes.
In Development, if it is missing, the app falls back to a well-known dev key and
logs a warning — never rely on the fallback in production.

## 3. Seed admin

Set `SeedAdmin:Email` / `SeedAdmin:Password` before first start; the seeder
creates the bootstrap admin (role `admin`, feature flags all-true).
