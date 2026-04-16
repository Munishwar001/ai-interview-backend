# Deployment Guide

## Prerequisites
- Docker and Docker Compose installed
- PostgreSQL (or use the Docker service)
- Ollama service (or use the Docker service)
- Valid API keys for: Google OAuth, Groq, Cloudinary, SMTP

## Deployment Steps

### 1. Prepare Environment Variables
```bash
# Copy the example environment file
cp .env.example .env

# Edit .env with your production values
nano .env
```

**Required Environment Variables:**
- `DB_USER` - PostgreSQL username
- `DB_PASSWORD` - PostgreSQL password (use strong password: `^A-Za-z0-9!@#$%` minimum 16 chars)
- `DB_NAME` - Database name
- `JWT_SECRET_KEY` - JWT signing key (minimum 32 characters)
- `GOOGLE_CLIENT_ID` - From Google Cloud Console
- `GROQ_API_KEY` - From Groq API dashboard
- `CLOUDINARY_CLOUD_NAME` - From Cloudinary account
- `CLOUDINARY_API_KEY` - From Cloudinary account
- `CLOUDINARY_API_SECRET` - From Cloudinary account
- `FRONTEND_URL` - Your production frontend URL (e.g., https://yourdomain.com)
- `EMAIL_USERNAME` - SMTP email (e.g., no-reply@yourdomain.com)
- `EMAIL_PASSWORD` - SMTP password or app-specific password
- `OLLAMA_BASE_URL` - Ollama service URL

### 2. Build Docker Images

```bash
# Build the images
docker-compose -f docker-compose.prod.yml build

# Or build without cache (recommended for clean build)
docker-compose -f docker-compose.prod.yml build --no-cache
```

### 3. Start Services

```bash
# Start all services in detached mode
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f api

# Specific service logs
docker-compose -f docker-compose.prod.yml logs -f db
docker-compose -f docker-compose.prod.yml logs -f ollama
```

### 4. Database Migration (if using EF Core)

```bash
# Run migrations inside the container
docker-compose -f docker-compose.prod.yml exec api dotnet ef database update

# Or manually in container shell
docker-compose -f docker-compose.prod.yml exec api bash
# Then: dotnet ef database update
```

### 5. Pull Ollama Models (if needed)

```bash
# Pull the model inside Ollama container
docker-compose -f docker-compose.prod.yml exec ollama ollama pull llama3.2
```

### 6. Health Checks

```bash
# Check API health
curl http://localhost:8080/health

# Check database connection
docker-compose -f docker-compose.prod.yml exec db pg_isready -U postgres -d AI_Db

# Check Ollama
curl http://localhost:11434/api/tags
```

## Production Best Practices

### Security
- ✅ Use strong passwords (minimum 16 characters with special characters)
- ✅ Never commit `.env` file (already in .gitignore)
- ✅ Use HTTPS with a reverse proxy (Nginx/Traefik)
- ✅ Enable CORS only for your domain
- ✅ Use environment variables for all secrets
- ✅ Regularly rotate API keys and database passwords

### Performance
- ✅ Use a production-grade reverse proxy (Nginx/Traefik)
- ✅ Enable compression in Nginx
- ✅ Use a CDN for static files
- ✅ Configure database connection pooling
- ✅ Set appropriate logging levels (Warning in Production)
- ✅ Enable Redis for caching (optional, for future scalability)

### Monitoring & Logging
- ✅ Configure log aggregation (ELK, CloudWatch, etc.)
- ✅ Set up health check endpoints
- ✅ Monitor container resource usage
- ✅ Set up alerts for errors and performance issues

## Stopping Services

```bash
# Stop all services
docker-compose -f docker-compose.prod.yml down

# Stop and remove volumes (WARNING: Data loss!)
docker-compose -f docker-compose.prod.yml down -v
```

## Reverse Proxy Setup (Nginx Example)

Create an `nginx.conf` file in your deployment directory:

```nginx
upstream api {
    server api:8080;
}

server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;
    
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    
    # Enable gzip compression
    gzip on;
    gzip_types text/plain text/css application/json application/javascript;
    
    # API proxy
    location /api/ {
        proxy_pass http://api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
    
    # Health check endpoint
    location /health {
        proxy_pass http://api/health;
    }
}
```

## Troubleshooting

### Container won't start
```bash
# Check logs
docker-compose -f docker-compose.prod.yml logs api

# Check if ports are already in use
netstat -tulpn | grep :8080
lsof -i :8080
```

### Database connection issues
```bash
# Test database connection
docker-compose -f docker-compose.prod.yml exec db psql -U postgres -d AI_Db -c "SELECT 1"
```

### Ollama model not found
```bash
# List available models
docker-compose -f docker-compose.prod.yml exec ollama ollama list

# Pull required model
docker-compose -f docker-compose.prod.yml exec ollama ollama pull llama3.2
```

## Backup & Recovery

### Database Backup
```bash
# Create backup
docker-compose -f docker-compose.prod.yml exec db pg_dump -U postgres -d AI_Db > backup.sql

# Restore from backup
docker-compose -f docker-compose.prod.yml exec -T db psql -U postgres -d AI_Db < backup.sql
```

### Volume Backup
```bash
# Backup volumes
docker run --rm -v ai-interview-server_postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres_backup.tar.gz /data

# Restore volumes
docker run --rm -v ai-interview-server_postgres_data:/data -v $(pwd):/backup alpine tar xzf /backup/postgres_backup.tar.gz -C /
```
