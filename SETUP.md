# Setup Guide for New Contributors

Welcome! This guide will help you get the project running locally.

## 📋 Prerequisites

Before you start, make sure you have:
- **.NET 10 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet)
- **PostgreSQL 15+** — [Download](https://www.postgresql.org/download/) (or use Docker)
- **Git** — [Download](https://git-scm.com)

Optional but recommended:
- **Docker & Docker Compose** — [Download](https://www.docker.com/products/docker-desktop)
- **Ollama** — [Download](https://ollama.com) for local AI fallback

---

## 🚀 Quick Start (5 minutes)

### 1. Clone the Repository

```bash
git clone <repo-url>
cd AIInterview.Server
```

### 2. Set Up Configuration

```bash
cd AIInterview.Server

# Copy the example configuration
cp appsettings.Development.json.example appsettings.Development.json

# Open in your editor and fill in the values
# nano appsettings.Development.json  (Linux/Mac)
# notepad appsettings.Development.json  (Windows)
```

### 3. Start Database (Choose One)

**Option A: Using Docker** (recommended)
```bash
# From root directory
docker-compose up -d

# Wait for PostgreSQL to be healthy (~10 seconds)
# Database will be ready at localhost:5432
```

**Option B: Using Local PostgreSQL**
```bash
# Create database and user
psql -U postgres

# Run these SQL commands:
CREATE DATABASE AI_Db;
CREATE USER app_user WITH PASSWORD 'your_password';
ALTER ROLE app_user WITH SUPERUSER;
ALTER ROLE app_user WITH CREATEDB;
```

### 4. Configure Connection String

Edit `AIInterview.Server/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=AI_Db;Username=postgres;Password=your_password"
  }
}
```

### 5. Run Migrations

```bash
cd AIInterview.Server
dotnet ef database update --project ../AIInterview.Infrastructure --startup-project .
```

### 6. Start the Application

```bash
dotnet run
```

The API will be available at:
- **API**: https://localhost:7129
- **Swagger UI**: https://localhost:7129/swagger
- **Health Check**: https://localhost:7129/health

---

## 🔑 Required API Keys & Credentials

You'll need to sign up for these services (most have free tiers):

### 1. **Groq API** (AI Provider)
- Visit: https://console.groq.com
- Create account → Generate API key
- Copy to `appsettings.Development.json` under `Groq:ApiKey`

### 2. **Google OAuth** (Login)
- Visit: https://console.cloud.google.com
- Create project → Enable Google+ API
- Create OAuth 2.0 credential (Desktop application)
- Copy Client ID to `appsettings.Development.json` under `Google:ClientId`

### 3. **Cloudinary** (Image Hosting)
- Visit: https://cloudinary.com
- Sign up → Get Cloud Name, API Key, API Secret
- Copy to `appsettings.Development.json` under `CloudinarySettings`

### 4. **SMTP for Emails**
- Using Gmail:
  1. Enable 2-step verification: https://myaccount.google.com/security
  2. Generate app password: https://myaccount.google.com/apppasswords
  3. Use email and app password in `EmailSettings`

---

## 🐳 Docker-Based Development

If you prefer everything containerized:

```bash
# Start all services (API, PostgreSQL, Ollama)
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop all services
docker-compose down

# Remove volumes (WARNING: deletes data!)
docker-compose down -v
```

---

## 📧 Email Templates

Forgot-password emails use an HTML template:
- **File**: `AIInterview.Server/Templates/ForgotPasswordTemplate.html`
- **Placeholders**: `{{USER_EMAIL}}` and `{{RESET_URL}}`
- **Edit** the template to customize styling

---

## ✅ Verify Everything Works

### Check API Health
```bash
curl https://localhost:7129/health
```

### Check Database Connection
```bash
dotnet run --project AIInterview.Server

# Look for startup logs without database errors
```

### Access Swagger UI
Open browser: https://localhost:7129/swagger

---

## 🔧 Common Issues

### **PostgreSQL Connection Error**
```
Error: unable to connect to database server
```
**Solution:**
- Verify PostgreSQL is running
- Check connection string in `appsettings.Development.json`
- Test manually: `psql -U postgres -d AI_Db`

### **Port Already in Use**
```
error: port 7129 is already in use
```
**Solution:**
```bash
# Kill the process on that port
# Windows:
netstat -ano | findstr :7129
taskkill /PID <PID> /F

# Linux/Mac:
lsof -i :7129
kill -9 <PID>
```

### **Groq API Key Invalid**
```
Unauthorized: Invalid API Key
```
**Solution:**
- Verify key from https://console.groq.com
- Check it's correctly copied (no spaces)

### **EF Core Migration Issues**
```bash
# Reset migrations (WARNING: deletes data!)
dotnet ef database drop --force
dotnet ef database update
```

---

## 📚 Additional Resources

- **Main README**: [README.md](README.md)
- **API Endpoints**: See README.md API section
- **Deployment**: [DEPLOYMENT.md](DEPLOYMENT.md)
- **Code Style**: See project structure conventions

---

## 🤝 Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make changes and test locally
3. Commit with clear messages: `git commit -m "feat: description"`
4. Push: `git push origin feature/your-feature`
5. Create Pull Request

Never commit:
- `.env` files
- `appsettings.Development.json` (use `.example`)
- API keys or passwords
- Build artifacts (`bin/`, `obj/`)

---

## 📞 Need Help?

- Check [DEPLOYMENT.md](DEPLOYMENT.md) for production setup
- Review [README.md](README.md) for API documentation
- Check logs: `docker-compose logs api`

---

Happy coding! 🎉
