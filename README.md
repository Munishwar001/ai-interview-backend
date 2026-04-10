# AI Interview Backend

A production-ready .NET 10 backend for an AI-powered interview platform. Features resume analysis, mock AI interviews, job management, and company profiles — powered by Groq (primary) with Ollama as a local fallback.

---

## Tech Stack

- .NET 10 / ASP.NET Core Web API
- PostgreSQL + Dapper
- ASP.NET Core Identity + JWT Authentication
- Groq API (llama-3.1-8b-instant) — primary AI
- Ollama (http://localhost:11434) — local AI fallback
- Clean layered architecture: Core → Application → Infrastructure → Server

---

## Project Structure

```
├── AIInterview.Core/           # DTOs, constants, shared models
├── AIInterview.Application/    # Interfaces, services, business logic
├── AIInterview.Infrastructure/ # Dapper repositories, DbContext, migrations
└── AIInterview.Server/         # Controllers, middleware, configuration
```

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- (Optional) Ollama for local AI fallback — https://ollama.com

### 1. Clone & Configure

```bash
git clone <repo-url>
cd AIInterview.Server
```

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=AI_Db;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "yourIssuer",
    "Audience": "yourAudience",
    "AccessTokenExpiration": "60",
    "RefreshTokenExpiration": "10080"
  },
  "Google": {
    "ClientId": "your-google-client-id"
  },
  "Groq": {
    "ApiKey": "your-groq-api-key"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama3.2"
  }
}
```

Get a free Groq API key at https://console.groq.com

### 2. Run Database Migrations

```bash
dotnet ef database update --project AIInterview.Infrastructure --startup-project AIInterview.Server
```

For mock interview tables (Dapper — run manually in PostgreSQL):

```bash
psql -U postgres -d AI_Db -f AIInterview.Infrastructure/Migrations/mock_interview_tables.sql
```

### 3. Run the Server

```bash
dotnet run --project AIInterview.Server
```

Swagger UI: https://localhost:7129/swagger

---

## AI Architecture

```
Request
  └── FallbackAiService
        ├── GroqAiService (primary)   — api.groq.com
        └── OllamaAiService (fallback) — localhost:11434
```

If Groq fails for any reason (rate limit, network, etc.), the request automatically retries via Ollama. No code changes needed.

### Ollama Setup (optional but recommended)

```bash
# Install from https://ollama.com
ollama serve
ollama pull llama3.2
```

---

## API Endpoints

All endpoints require `Authorization: Bearer <token>` unless marked public.

### Auth — `/api/account`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/account/register` | Register new user | Public |
| POST | `/api/account/login` | Login, returns JWT | Public |
| POST | `/api/account/refresh` | Refresh access token | Public |
| POST | `/api/account/google-login` | Google OAuth login | Public |
| POST | `/api/account/logout` | Logout | Required |

### Job Seeker — `/api/jobseeker`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/jobseeker/profile` | Get profile |
| PUT | `/api/jobseeker/profile` | Create or update profile |
| POST | `/api/jobseeker/upload-avatar` | Upload avatar image |
| DELETE | `/api/jobseeker/delete-avatar` | Delete avatar |
| POST | `/api/jobseeker/upload-resume` | Upload resume (PDF/DOCX) |
| DELETE | `/api/jobseeker/delete-resume` | Delete resume |
| GET | `/api/jobseeker/download-resume` | Download resume |
| GET | `/api/jobseeker/experience` | Get work experience |
| POST | `/api/jobseeker/experience` | Add experience |
| PUT | `/api/jobseeker/experience/{id}` | Update experience |
| DELETE | `/api/jobseeker/experience/{id}` | Delete experience |
| GET | `/api/jobseeker/education` | Get education |
| POST | `/api/jobseeker/education` | Add education |
| PUT | `/api/jobseeker/education/{id}` | Update education |
| DELETE | `/api/jobseeker/education/{id}` | Delete education |
| GET | `/api/jobseeker/skills` | Get user skills |
| PUT | `/api/jobseeker/skills` | Sync skills |

### Resume Enhancer — `/api/resume-enhancer`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/resume-enhancer/analyze` | Analyze uploaded resume file |
| POST | `/api/resume-enhancer/analyze-from-profile` | Analyze resume from profile |
| GET | `/api/resume-enhancer/result` | Get last cached analysis |

### Mock Interview — `/api/mock-interview`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/mock-interview/start` | Start new interview session |
| POST | `/api/mock-interview/message` | Send answer, get next question |
| GET | `/api/mock-interview/session/{sessionId}` | Get session with full chat history |
| GET | `/api/mock-interview/sessions` | Get all sessions for current user |

**Start interview — request:**
```json
{
  "skills": ["React", "TypeScript"]
}
```
> If `skills` is empty or omitted, skills are loaded from the user's profile automatically.

**Start interview — response:**
```json
{
  "sessionId": "abc-123...",
  "skills": ["React", "TypeScript"],
  "firstQuestion": "Can you explain the difference between props and state in React?"
}
```

**Send message — request:**
```json
{
  "sessionId": "abc-123...",
  "userMessage": "Props are read-only and passed from parent, state is managed internally."
}
```

**Send message — response:**
```json
{
  "aiMessage": "Good answer! Next question: How does the useEffect hook work?",
  "isCompleted": false,
  "feedbackSummary": null
}
```

After 5 questions `isCompleted` becomes `true` and `feedbackSummary` contains the overall performance summary.

### Jobs — `/api/jobs`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/jobs/my-jobs` | Get employer's posted jobs |
| POST | `/api/jobs` | Create job posting |
| PUT | `/api/jobs/{id}` | Update job |
| DELETE | `/api/jobs/{id}` | Delete job |
| PATCH | `/api/jobs/{id}/close` | Close job |
| PATCH | `/api/jobs/{id}/reopen` | Reopen job |
| GET | `/api/jobs/{id}/applicants` | Get applicants |
| POST | `/api/jobs/generate-description` | AI-generate job description |

**Generate description — request:**
```json
{
  "title": "Frontend Developer",
  "skills": ["React", "TypeScript", "Redux"]
}
```

### Company Profile — `/api/companyprofile`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/companyprofile` | Get company profile |
| PUT | `/api/companyprofile` | Update company profile |
| POST | `/api/companyprofile/upload-logo` | Upload company logo |

### Lookups — `/api/lookup`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/lookup/skills` | Get all available skills | Public |
| GET | `/api/lookup/job-types` | Get job types | Public |
| GET | `/api/lookup/company-sizes` | Get company sizes | Public |

---

## Environment Variables Reference

| Key | Description |
|-----|-------------|
| `ConnectionStrings:Default` | PostgreSQL connection string |
| `Jwt:SecretKey` | JWT signing key (min 32 chars) |
| `Jwt:Issuer` | JWT issuer |
| `Jwt:Audience` | JWT audience |
| `Jwt:AccessTokenExpiration` | Access token TTL in minutes |
| `Jwt:RefreshTokenExpiration` | Refresh token TTL in minutes |
| `Google:ClientId` | Google OAuth client ID |
| `Groq:ApiKey` | Groq API key (console.groq.com) |
| `Ollama:BaseUrl` | Ollama base URL (default: http://localhost:11434) |
| `Ollama:Model` | Ollama model name (default: llama3.2) |
| `Cors:AllowedOrigins` | Array of allowed frontend origins |
