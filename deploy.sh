#!/bin/bash

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}AI Interview - Production Deployment Script${NC}"
echo "=============================================="

# Check if .env file exists
if [ ! -f .env ]; then
    echo -e "${RED}Error: .env file not found!${NC}"
    echo "Please create .env file from .env.example:"
    echo "  cp .env.example .env"
    echo "  nano .env"
    exit 1
fi

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}Error: Docker is not running${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ Docker is running${NC}"
}

# Function to build images
build_images() {
    echo -e "\n${YELLOW}Building Docker images...${NC}"
    docker-compose -f docker-compose.prod.yml build --no-cache
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Images built successfully${NC}"
    else
        echo -e "${RED}Error: Failed to build images${NC}"
        exit 1
    fi
}

# Function to start services
start_services() {
    echo -e "\n${YELLOW}Starting services...${NC}"
    docker-compose -f docker-compose.prod.yml up -d
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Services started successfully${NC}"
    else
        echo -e "${RED}Error: Failed to start services${NC}"
        exit 1
    fi
}

# Function to check service health
check_health() {
    echo -e "\n${YELLOW}Checking service health...${NC}"
    sleep 5
    
    # Check API
    echo -n "Checking API... "
    if curl -s http://localhost:8080/health > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC}"
    else
        echo -e "${YELLOW}Starting (wait a moment)${NC}"
    fi
    
    # Check Database
    echo -n "Checking Database... "
    if docker-compose -f docker-compose.prod.yml exec -T db pg_isready -U postgres > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC}"
    else
        echo -e "${RED}✗${NC}"
    fi
}

# Function to run migrations
run_migrations() {
    echo -e "\n${YELLOW}Running database migrations...${NC}"
    docker-compose -f docker-compose.prod.yml exec api dotnet ef database update
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Migrations completed${NC}"
    else
        echo -e "${RED}Warning: Migration check/execution completed with issues${NC}"
    fi
}

# Function to pull Ollama model
pull_ollama_model() {
    echo -e "\n${YELLOW}Pulling Ollama model...${NC}"
    docker-compose -f docker-compose.prod.yml exec ollama ollama pull llama3.2
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Model pulled successfully${NC}"
    else
        echo -e "${RED}Error: Failed to pull model${NC}"
    fi
}

# Function to show status
show_status() {
    echo -e "\n${YELLOW}Service Status:${NC}"
    docker-compose -f docker-compose.prod.yml ps
}

# Function to show logs
show_logs() {
    echo -e "\n${YELLOW}Service Logs (Ctrl+C to exit):${NC}"
    docker-compose -f docker-compose.prod.yml logs -f
}

# Function to stop services
stop_services() {
    echo -e "\n${YELLOW}Stopping services...${NC}"
    docker-compose -f docker-compose.prod.yml down
    echo -e "${GREEN}✓ Services stopped${NC}"
}

# Main menu
show_menu() {
    echo -e "\n${YELLOW}Select an option:${NC}"
    echo "1) Full deployment (build + start + migrate)"
    echo "2) Build images"
    echo "3) Start services"
    echo "4) Stop services"
    echo "5) Check health"
    echo "6) View logs"
    echo "7) View status"
    echo "8) Pull Ollama model"
    echo "9) Run migrations"
    echo "0) Exit"
    echo -n "Enter your choice: "
}

# Check Docker first
check_docker

# If argument provided, execute that command
if [ $# -eq 0 ]; then
    # Interactive mode
    while true; do
        show_menu
        read choice
        case $choice in
            1)
                build_images
                start_services
                check_health
                run_migrations
                pull_ollama_model
                show_status
                ;;
            2) build_images ;;
            3) start_services ;;
            4) stop_services ;;
            5) check_health ;;
            6) show_logs ;;
            7) show_status ;;
            8) pull_ollama_model ;;
            9) run_migrations ;;
            0) exit 0 ;;
            *) echo -e "${RED}Invalid option${NC}" ;;
        esac
    done
else
    # Command mode
    case $1 in
        build) build_images ;;
        start) start_services ;;
        stop) stop_services ;;
        health) check_health ;;
        logs) show_logs ;;
        status) show_status ;;
        migrate) run_migrations ;;
        ollama) pull_ollama_model ;;
        deploy) 
            build_images
            start_services
            check_health
            run_migrations
            pull_ollama_model
            show_status
            ;;
        *)
            echo "Usage: $0 [build|start|stop|health|logs|status|migrate|ollama|deploy]"
            exit 1
            ;;
    esac
fi
