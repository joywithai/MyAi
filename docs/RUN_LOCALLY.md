# MyAi — লোকাল মেশিনে চালানোর গাইড (VS Code / Visual Studio)

## ধাপ ০: যা যা ইনস্টল থাকতে হবে

| Tool | কোথা থেকে | নোট |
|---|---|---|
| **.NET 10 SDK** | https://dotnet.microsoft.com/download/dotnet/10.0 | `dotnet --version` → 10.x দেখালেই হবে |
| **Node.js 20+ (LTS)** | https://nodejs.org | `node -v` দিয়ে চেক |
| **Docker Desktop** | https://www.docker.com/products/docker-desktop | PostgreSQL + Redis চালানোর জন্য |
| **VS Code** + **C# Dev Kit** extension | VS Code marketplace | Visual Studio 2026/2022 (latest) হলে সেটাও চলবে |
| Git | https://git-scm.com | |

## ধাপ ১: কোড ক্লোন

```bash
git clone -b arena/01a0b443-myai https://github.com/joywithai/MyAi.git
cd MyAi
code .          # VS Code-এ খুলুন
```

## ধাপ ২: PostgreSQL + Redis চালু (Docker)

```bash
docker compose up -d postgres redis
docker compose ps        # দুটোই "running" দেখাবে
```

- PostgreSQL: `localhost:5432`, user `myai`, password `myai_secret`, db `myai`
- Redis: `localhost:6379` (password `myai_redis`)
- Redis না চালালেও সমস্যা নেই — অ্যাপ নিজে থেকেই in-memory cache ব্যবহার করবে।

## ধাপ ৩: প্রথমবারের মতো EF Migration বানান (একবারই)

```bash
dotnet tool install --global dotnet-ef      # একবারই
dotnet ef migrations add InitialCreate -p src/MyAi.Infrastructure -s src/MyAi.Api
```

> তৈরি হওয়া `src/MyAi.Infrastructure/Migrations/` ফোল্ডারটা git-এ commit করে দিন —
> তাহলে পরেরবার থেকে আর এই ধাপ লাগবে না, docker-compose-এও টেবিল নিজেই তৈরি হবে।

## ধাপ ৪: Backend চালান

VS Code-এর Terminal-এ (Ctrl+`):

```bash
dotnet run --project src/MyAi.Api
```

- API চলবে: **http://localhost:8080** (launchSettings.json-এ সেট করা)
- Swagger UI: http://localhost:8080/swagger
- Hangfire ড্যাশবোর্ড: http://localhost:8080/hangfire
- Health: http://localhost:8080/health

প্রথম স্টার্টেই নিজে থেকে:
1. Migration apply হবে (সব ১৫টা টেবিল)
2. Seed হবে — ৩ রোলের feature flag, ১২ expression, ৬ animation, ২টা প্ল্যান, **AIAssistantAvatar.vrm** মডেল, আর bootstrap admin:

```
Email:    admin@myai.local
Password: Admin123!
```

### (ঐচ্ছিক) আসল AI উত্তর চাইলে
https://openrouter.ai থেকে ফ্রি key নিয়ে `src/MyAi.Api/appsettings.Development.json`-এ:

```json
"OpenRouter": { "ApiKey": "sk-or-v1-.....", "DefaultModel": "google/gemini-2.0-flash-001:free" }
```

Key না দিলেও চ্যাট কাজ করবে — **Demo AI** canned reply দেবে (ডেমো TTS-সহ)।

## ধাপ ৫: Frontend চালান

নতুন টার্মিনাল খুলুন (VS Code-এ `+` আইকন):

```bash
cd web
npm install
npm run dev
```

- সাইট চলবে: **http://localhost:3000**
- `web/.env.local` আগেই সেট করা → API `http://localhost:8080/api/v1`-এ যাবে।

## ধাপ ৬: সব মিলিয়ে দেখুন ✅

1. http://localhost:3000 → **Register** করুন → **Chat**-এ যান
2. বাংলায় লিখুন (যেমন "তুমি কেমন আছো?") → অ্যাভাটার উত্তর দেবে, TTS বাজবে, **মুখ নড়বে** (lip-sync), expression বদলাবে
3. বাঁ দিকের chip দিয়ে expression বদলান, ভয়েস speed/pitch ট্রাই করুন
4. **admin@myai.local / Admin123!** দিয়ে login → `/admin` প্যানেল (users, flags, catalog, system settings)
5. `/subscription` → ডেমো পেমেন্ট → সাথে সাথেই subscriber (সব ফিচার আনলক)

## Visual Studio দিয়ে করতে চাইলে

1. `MyAi.sln` ডাবল-ক্লিক করে খুলুন
2. Startup project: **MyAi.Api** (রাইট-ক্লিক → Set as Startup Project)
3. ▶ (http profile) চাপুন — একই জায়গায় 8080-এ চলবে
4. বাকি সব ধাপ একই (Docker, `npm run dev` VS Code/সাধারণ টার্মিনালেই)

## এক কমান্ডে সব (Docker)

```bash
docker compose up --build
# api → :8080, web → :3000 (ধাপ ৩-এর migration একবার commit করা থাকলে টেবিলও অটো-তৈরি)
```

## Troubleshooting

| সমস্যা | সমাধান |
|---|---|
| `dotnet: command not found` | SDK ইনস্টলের পর টার্মিনাল রিস্টার্ট করুন |
| `dotnet ef` চেনে না | `export PATH="$PATH:$HOME/.dotnet/tools"` (Windows: নতুন টার্মিনাল) |
| Port 8080 ব্যস্ত | `src/MyAi.Api/Properties/launchSettings.json`-এ port বদলান + `web/.env.local`-ও মিলিয়ে দিন |
| Web-এ CORS error | API ঠিক 8080-এ চলছে কিনা দেখুন; `Cors:AllowedOrigins`-এ `http://localhost:3000` আছে কিনা চেক করুন |
| Chat 429 দেয় | দিনের লিমিট শেষ — admin দিয়ে flag বাড়ান বা পরদিন |
| TTS নীরব | Edge TTS internet চায়; log-এ `tts` error দেখুন, fail হলে অ্যাপ text-only reply দেয় |
