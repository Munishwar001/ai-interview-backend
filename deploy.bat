@echo off
REM AI Interview - Production Deployment Script (Windows)

setlocal enabledelayedexpansion

echo AI Interview - Production Deployment Script
echo ============================================

REM Check if .env file exists
if not exist .env (
    echo Error: .env file not found!
    echo Please create .env file from .env.example:
    echo   copy .env.example .env
    echo   notepad .env
    pause
    exit /b 1
)

REM Function to show menu
:menu
echo.
echo Select an option:
echo 1) Full deployment (build + start + migrate)
echo 2) Build images
echo 3) Start services
echo 4) Stop services
echo 5) Check health
echo 6) View logs
echo 7) View status
echo 8) Pull Ollama model
echo 9) Run migrations
echo 0) Exit
set /p choice="Enter your choice: "

if "%choice%"=="1" goto deploy_all
if "%choice%"=="2" goto build
if "%choice%"=="3" goto start
if "%choice%"=="4" goto stop
if "%choice%"=="5" goto health
if "%choice%"=="6" goto logs
if "%choice%"=="7" goto status
if "%choice%"=="8" goto ollama
if "%choice%"=="9" goto migrate
if "%choice%"=="0" exit /b 0
echo Invalid option
goto menu

:deploy_all
echo.
echo Building Docker images...
docker-compose -f docker-compose.prod.yml build --no-cache
echo.
echo Starting services...
docker-compose -f docker-compose.prod.yml up -d
echo.
echo Checking health...
timeout /t 5 /nobreak
docker-compose -f docker-compose.prod.yml ps
echo.
echo Running migrations...
docker-compose -f docker-compose.prod.yml exec api dotnet ef database update
echo.
echo Pulling Ollama model...
docker-compose -f docker-compose.prod.yml exec ollama ollama pull llama3.2
goto menu

:build
echo.
echo Building Docker images...
docker-compose -f docker-compose.prod.yml build --no-cache
goto menu

:start
echo.
echo Starting services...
docker-compose -f docker-compose.prod.yml up -d
timeout /t 3 /nobreak
docker-compose -f docker-compose.prod.yml ps
goto menu

:stop
echo.
echo Stopping services...
docker-compose -f docker-compose.prod.yml down
goto menu

:health
echo.
echo Checking service health...
timeout /t 5 /nobreak
echo Checking API...
curl http://localhost:8080/health
echo.
echo Service Status:
docker-compose -f docker-compose.prod.yml ps
goto menu

:logs
echo.
echo Service Logs (Ctrl+C to exit):
docker-compose -f docker-compose.prod.yml logs -f
goto menu

:status
echo.
echo Service Status:
docker-compose -f docker-compose.prod.yml ps
goto menu

:ollama
echo.
echo Pulling Ollama model...
docker-compose -f docker-compose.prod.yml exec ollama ollama pull llama3.2
goto menu

:migrate
echo.
echo Running database migrations...
docker-compose -f docker-compose.prod.yml exec api dotnet ef database update
goto menu
