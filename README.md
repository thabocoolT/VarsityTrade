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

## Project Status

🚧 **Active development**

The application is still being developed and features may change as the architecture and requirements evolve.

## Repository

GitHub: https://github.com/thabocoolT/VarsityTrade

> This is a personal software-development project and is continuously improved as I learn new engineering practices.
