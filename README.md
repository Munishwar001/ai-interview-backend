# AI Interview Backend

A production-ready .NET 10 backend for an AI-powered interview platform. Features resume analysis, mock AI interviews, job management, and company profiles — powered by Groq (primary) with Ollama as a local fallback.

---

## 🚀 Quick Links

- **Deployment Guide**: See [DEPLOYMENT.md](DEPLOYMENT.md) for production setup
- **API Documentation**: Swagger UI available at `/swagger` endpoint
- **Environment Setup**: Copy `appsettings.Development.json.example` and configure with your values

---

## Tech Stack

- .NET 10 / ASP.NET Core Web API
- PostgreSQL + Dapper
- ASP.NET Core Identity + JWT Authentication
- Groq API (llama-3.1-8b-instant) — primary AI
- Ollama (http://localhost:11434) — local AI fallback
- Clean layered architecture: Core → Application → Infrastructure → Server
- Docker & Docker Compose support
- Email templates with HTML formatting

---

## Project Structure

```
├── AIInterview.Core/           # DTOs, constants, shared models
├── AIInterview.Application/    # Interfaces, services, business logic
├── AIInterview.Infrastructure/ # Dapper repositories, DbContext, migrations
├── AIInterview.Server/         # Controllers, middleware, configuration
├── Templates/                  # Email templates (HTML)
├── docker-compose.yml          # Development compose
├── docker-compose.prod.yml     # Production compose
├── DEPLOYMENT.md               # Production deployment guide
└── deploy.sh / deploy.bat      # Deployment automation scripts
```

---

## 🔧 Getting Started (Local Development)

### Prerequisites

- .NET 10 SDK ([download](https://dotnet.microsoft.com/download/dotnet))
- PostgreSQL 15+ ([download](https://www.postgresql.org/download/))
- (Optional) Docker & Docker Compose for containerized development
- (Optional) Ollama for local AI fallback ([download](https://ollama.com))

### 1. Clone Repository

```bash
git clone <repo-url>
cd AIInterview.Server
```

### 2. Configure Environment

**Option A: Manual Setup**

Copy and edit configuration:
```bash
cd AIInterview.Server
cp appsettings.Development.json.example appsettings.Development.json
# Edit appsettings.Development.json with your values
```

**Option B: Docker Setup**

Use the development docker-compose:
```bash
docker-compose up -d
# This starts PostgreSQL, Ollama, and configures the database
```

### 3. Configure appsettings.Development.json

Update these required values:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=AI_Db;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "SecretKey": "YourSecretKeyWithAtLeast32CharactersForDevelopment"
  },
  "Google": {
    "ClientId": "your-google-oauth-client-id.apps.googleusercontent.com"
  },
  "Groq": {
    "ApiKey": "your-groq-api-key"
  },
  "CloudinarySettings": {
    "CloudName": "your-cloudinary-cloud-name",
    "ApiKey": "your-cloudinary-api-key",
    "ApiSecret": "your-cloudinary-api-secret"
  },
  "EmailSettings": {
    "Username": "your-email@gmail.com",
    "Password": "your-app-specific-password"
  }
}
```

**API Keys & Credentials:**
- **Groq API**: Get free key at https://console.groq.com
- **Google OAuth**: Set up at https://console.cloud.google.com
- **Cloudinary**: Sign up at https://cloudinary.com
- **Gmail App Password**: Enable 2FA and generate at https://myaccount.google.com/apppasswords

### 4. Run Database Migrations

```bash
# Apply migrations using EF Core
dotnet ef database update --project AIInterview.Infrastructure --startup-project AIInterview.Server
```

### 5. Run the Server

```bash
dotnet run --project AIInterview.Server
```

**Endpoints:**
- API: `https://localhost:7129`
- Swagger UI: `https://localhost:7129/swagger`
- Health Check: `https://localhost:7129/health`

---

## 📧 Email Template

Forgot-password emails use HTML template from `Templates/ForgotPasswordTemplate.html` with dynamic placeholders:
- `{{USER_EMAIL}}` — User's email address
- `{{RESET_URL}}` — Password reset link

Edit the template to customize styling and content.

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
| `CloudinarySettings:CloudName` | Cloudinary cloud name |
| `CloudinarySettings:ApiKey` | Cloudinary API key |
| `CloudinarySettings:ApiSecret` | Cloudinary API secret |
| `Cors:AllowedOrigins` | Array of allowed frontend origins |
