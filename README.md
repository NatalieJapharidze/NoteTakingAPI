# Note Taking API 📝

A modern, production-ready REST API for managing personal notes, built with .NET 9 and Vertical Slice Architecture.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🚀 Features

- **🔐 JWT Authentication** - Secure user registration, login, and token refresh
- **📄 Notes Management** - Full CRUD operations with rich content support
- **🏷️ Tag System** - Organize notes with tags and filtering
- **🔍 Search & Filter** - Full-text search across notes with pagination
- **🗑️ Soft Delete** - Safe note deletion with recovery options
- **📊 Health Monitoring** - Built-in health checks and logging
- **📚 API Documentation** - Interactive Scalar API documentation
- **🐳 Docker Ready** - Complete containerization with PostgreSQL
- **🔒 Production Security** - BCrypt password hashing, secure JWT handling

## 🏗️ Architecture

Built with **Vertical Slice Architecture** - each feature is self-contained with its own:
- Request/Response models
- Validation rules
- Business logic
- Database operations
- Endpoint mapping

### Why Vertical Slice?
- ✅ **Easy to find code** - Everything for a feature in one place
- ✅ **Fast development** - Add features without touching multiple layers
- ✅ **Team-friendly** - Clear ownership boundaries
- ✅ **Highly testable** - Each feature can be tested independently

## 🛠️ Tech Stack

- **Backend**: .NET 9, Minimal APIs
- **Database**: PostgreSQL 15 with Entity Framework Core
- **Authentication**: JWT Bearer tokens with refresh token support
- **Validation**: FluentValidation
- **Logging**: Serilog with file and console output
- **Documentation**: OpenAPI with Scalar UI
- **Security**: BCrypt password hashing
- **Containerization**: Docker & Docker Compose

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerized setup)

### Option 1: Docker (Recommended)

```bash
# Clone the repository
git clone <repository-url>
cd NoteTakingAPI

# Start the application and database
docker-compose up -d

# The API will be available at:
# 🌐 API: http://localhost:5800
# 📚 Documentation: http://localhost:5800/scalar/v1
# ❤️ Health Check: http://localhost:5800/health
```

### Option 2: Local Development

```bash
# Install dependencies
dotnet restore

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Setup PostgreSQL database
# Create database: noteapi_dev
# Update connection string in appsettings.json if needed

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run

# API will be available at:
# 🌐 HTTPS: https://localhost:7232
# 🌐 HTTP: http://localhost:5171
# 📚 Documentation: https://localhost:7232/scalar/v1
```

## 📚 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/auth/register` | Register a new user |
| `POST` | `/auth/login` | Login user |
| `POST` | `/auth/refresh` | Refresh JWT token |

### Notes
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/notes` | Get notes (with pagination, search, filtering) |
| `GET` | `/notes/{id}` | Get specific note |
| `POST` | `/notes` | Create new note |
| `PUT` | `/notes/{id}` | Update note |
| `DELETE` | `/notes/{id}` | Delete note (soft delete) |

### Tags
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/tags` | Get user's tags with note counts |

### System
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Health check |

## 🧪 API Usage Examples

### 1. Register a New User
```bash
curl -X POST http://localhost:5800/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123",
    "fullName": "John Doe"
  }'
```

### 2. Login
```bash
curl -X POST http://localhost:5800/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123"
  }'
```

### 3. Create a Note
```bash
curl -X POST http://localhost:5800/notes \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "title": "My First Note",
    "content": "This is the content of my note with **markdown** support!",
    "tags": ["personal", "important"]
  }'
```

### 4. Search Notes
```bash
curl -X GET "http://localhost:5800/notes?search=first&tag=personal&page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🗂️ Project Structure

```
NoteTakingAPI/
├── Features/                     # 🎯 Vertical Slices
│   ├── Auth/                     # Authentication features
│   │   ├── Login.cs              # Complete login functionality
│   │   ├── Register.cs           # Complete registration functionality
│   │   └── RefreshToken.cs       # Complete token refresh functionality
│   ├── Notes/                    # Notes management features
│   │   ├── CreateNote.cs         # Complete note creation
│   │   ├── DeleteNote.cs         # Complete note deletion (soft delete)
│   │   ├── GetNoteById.cs        # Complete single note retrieval
│   │   ├── GetNotes.cs           # Complete notes listing with search/pagination
│   │   └── UpdateNote.cs         # Complete note updating
│   └── Tags/                     # Tag management features
│       └── GetTags.cs            # Complete tags retrieval
├── Infrastructure/               # 🏗️ Technical Infrastructure
│   ├── Database/
│   │   ├── Entities/             # Database entities
│   │   │   ├── User.cs
│   │   │   ├── Note.cs
│   │   │   ├── Tag.cs
│   │   │   └── NoteTag.cs
│   │   ├── Migrations/           # EF Core migrations
│   │   └── AppDbContext.cs       # Database context
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs # Global error handling
│   └── Services/
│       └── JwtService.cs         # JWT token management
├── Common/                       # 🔧 Shared Utilities
│   ├── Extensions/
│   │   └── ClaimsPrincipalExtensions.cs # JWT claims helpers
│   ├── Constants/                # Shared constants (ready for use)
│   └── Models/                   # Shared DTOs (ready for use)
├── logs/                         # 📝 Application logs (auto-generated)
├── docker-compose.yml            # 🐳 Multi-container setup
├── Dockerfile                    # 🐳 Container definition
├── Program.cs                    # 🚀 Application entry point
└── appsettings.json             # ⚙️ Configuration
```

## 🔧 Development

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName -o Infrastructure/Database/Migrations

# Apply migrations
dotnet ef database update

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Generate migration script
dotnet ef migrations script
```

### Docker Development

```bash
# Build and start services
docker-compose up -d --build

# View logs
docker-compose logs -f noteapi

# Stop services
docker-compose down

# Clean restart (removes volumes/data)
docker-compose down -v && docker-compose up -d --build
```

## 📊 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development` |
| `ConnectionStrings__DefaultConnection` | Database connection | See appsettings.json |
| `JwtSettings__Secret` | JWT signing secret | Required (32+ chars) |
| `JwtSettings__Issuer` | JWT issuer | `NoteApi` |
| `JwtSettings__Audience` | JWT audience | `NoteApiUsers` |
| `JwtSettings__ExpirationInMinutes` | Token lifetime | `60` |

### Database Configuration

**Local Development:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5700;Database=noteapi_dev;Username=postgres;Password=password123"
  }
}
```

**Docker:**
- PostgreSQL runs on port 5700 (external)
- API connects to service name `postgres`

## 🔒 Security Features

- **🔐 Secure Password Hashing** - BCrypt with salt (12 rounds)
- **🎫 JWT Authentication** - Secure token-based authentication
- **🔄 Token Refresh** - Automatic token renewal
- **🛡️ Authorization** - Route-level security
- **🚫 Soft Delete** - Data recovery capabilities
- **📝 Audit Logging** - Comprehensive request/response logging
- **🧮 Input Validation** - FluentValidation rules
- **🚨 Error Handling** - Secure error responses

## 📈 Performance Features

- **⚡ Minimal APIs** - Lightweight, fast endpoints
- **📄 Pagination** - Efficient data loading
- **🔍 Indexed Search** - Fast full-text search
- **💾 Connection Pooling** - Optimized database connections
- **📊 Health Checks** - System monitoring
- **📝 Structured Logging** - Performance insights

## 🚀 Deployment

## 🧪 Testing the API

### Using the Interactive Documentation

1. Start the application (Docker or local)
2. Open your browser to the documentation URL
3. Click the **🔒 Authorize** button
4. Register a new user or login
5. Copy the JWT token from the response
6. Paste it in the authorization dialog
7. Test all endpoints interactively!

### Using cURL

See the [API Usage Examples](#-api-usage-examples) section above.

### Using Postman

Import the OpenAPI specification from `/openapi/v1.json` for a complete Postman collection.


**Built with 🚀 by [Nato Japharidze]**

*A production-ready API showcasing modern .NET development practices*
