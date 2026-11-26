# Varsity Trade

Varsity Trade is a full-stack student marketplace web application being built for South African university students. The goal is to make buying, selling and trading within a university community simpler and safer by keeping marketplace activity tied to a student's registered university.

## Why I built it

I wanted to build something beyond a small academic exercise: a real software product based on a problem students can actually experience. The project has helped me practise backend development, database design, API development, authentication, debugging, architecture and Git-based development.

## Technology Stack

- **C# / .NET** — primary development language and platform
- **ASP.NET Core Web API** — REST API backend
- **ASP.NET MVC / Razor** — web application frontend
- **Entity Framework Core** — ORM and data access
- **SQL Server** — relational database
- **JWT + refresh tokens** — authentication
- **Git / GitHub** — version control
- **Postman / Swagger/OpenAPI** — API testing and documentation
- **.NET MAUI** — planned mobile application phase

## Current Architecture

The solution is organised into separate projects to keep responsibilities clear:

- `VarsityTrade.API` — controllers, HTTP endpoints and application startup
- `VarsityTrade.Application` — application services and business operations
- `VarsityTrade.Core` — core models, DTOs and domain-level definitions
- `VarsityTrade.Infrastructure` — database and external infrastructure concerns

## Key Features

- University-locked marketplace
- Buyer and seller functionality
- Seller profiles
- Listings and categories
- In-app buyer/seller messaging
- Cash and trade offers
- Transactions
- Reviews linked to completed transactions
- Notifications
- Admin moderation and platform management
- Hero banner management
- JWT authentication with refresh-token support

## What I am learning through the project

This project has pushed me beyond simply writing code that works. I am practising how to:

- Break a large application into maintainable components
- Design relationships between users, listings, offers and transactions
- Build and test REST APIs
- Validate user input and protect application resources
- Debug backend and database problems
- Use Git and feature-based development workflows
- Think about security, data integrity and maintainability

## Local setup

### Requirements
- .NET 10 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio or another .NET IDE

### Configure secrets
The API reads its database connection string and JWT signing key from .NET configuration. Do not commit real credentials to `appsettings.json`.

From the repository root, configure Development User Secrets for the API project:

```powershell
dotnet user-secrets --project VarsityTrade.API init
dotnet user-secrets --project VarsityTrade.API set "ConnectionStrings:DefaultConnection" "Server=localhost\\SQLEXPRESS;Database=VarsityTradeDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets --project VarsityTrade.API set "JwtSettings:SecretKey" "<generate-a-new-random-secret-at-least-32-characters-long>"
```

Use your own SQL Server instance and database settings. Generate a new JWT signing secret before running the application; never reuse a secret that was previously committed. In production, provide these values through your hosting environment's secret manager or environment variables.

### Run the solution
1. Restore packages: `dotnet restore VarsityTrade.slnx`
2. Apply database migrations using your configured SQL Server connection.
3. Start `VarsityTrade.API`.
4. Start `VarsityTrade` (the MVC web project) and ensure its `ApiSettings:BaseUrl` points to the API's local HTTPS address.

## Project Status

🚧 **Active development**

The application is still being developed and features may change as the architecture and requirements evolve.

## Repository

GitHub: https://github.com/thabocoolT/VarsityTrade

> This is a personal software-development project and is continuously improved as I learn new engineering practices.
