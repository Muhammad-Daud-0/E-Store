<!-- @format -->

# EStore - E-Commerce Platform

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-green)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Latest-red)

A modern, full-featured e-commerce web application built with **ASP.NET Core**, **Entity Framework Core**, and **Bootstrap**. Features a complete shopping experience with product catalog, shopping cart, checkout, and admin dashboard.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Development](#development)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

## Features

- 🛍️ **Product Catalog** - Browse products with detailed information
- 📂 **Category Management** - Hierarchical category organization
- 🛒 **Shopping Cart** - Persistent database-backed shopping cart with real-time updates
- 💳 **Checkout System** - Complete order processing with shipping details
- 👤 **User Authentication** - Secure registration and login using ASP.NET Core Identity
- 🔐 **Authorization Roles** - Admin and customer role-based access control
- 📊 **Admin Dashboard** - Manage products, categories, and view orders
- 📦 **Order Management** - Track orders and shipping information
- 👥 **User Profiles** - Customer profile management with account settings

## Tech Stack

| Component          | Technology                            |
| ------------------ | ------------------------------------- |
| **Backend**        | ASP.NET Core 8.0                      |
| **Database**       | SQL Server with Entity Framework Core |
| **Authentication** | ASP.NET Core Identity                 |
| **Frontend**       | Bootstrap 5, HTML5, CSS3, JavaScript  |
| **ORM**            | Entity Framework Core                 |

## Prerequisites

- **.NET SDK**: 8.0 or later (download from [dotnet.microsoft.com](https://dotnet.microsoft.com))
- **SQL Server**: LocalDB (included with Visual Studio) or SQL Server Express/Developer Edition
- **Git**: For cloning the repository
- **IDE**: Visual Studio 2022, Visual Studio Code, or JetBrains Rider (optional)

## Quick Start

```bash
# Clone the repository
git clone https://github.com/yourusername/EStore.git
cd EStore

# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

The application will be available at `https://localhost:7227` or `http://localhost:5280`

**Demo Credentials:**

- Username: `admin@example.com`
- Password: `AdminPass123!` (if seeded)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/EStore.git
cd EStore
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Database Setup

**Option A: Using Entity Framework Migrations (Recommended)**

```bash
# Install EF Core CLI if not already installed
dotnet tool install --global dotnet-ef

# Apply migrations to create database
dotnet ef database update
```

**Option B: Manual Connection String Configuration**

- Edit `appsettings.Development.json` or `appsettings.json`
- Update the `DefaultConnection` connection string to point to your SQL Server instance:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EStore;Trusted_Connection=true;"
  }
  ```

### 4. Seed Sample Data (Optional)

- The application automatically seeds initial data from `seed-data.json` on first run if the database is empty
- Users and products will be created automatically

### 5. Build the Project

```bash
dotnet build
```

### 6. Run the Application

```bash
dotnet run
```

## Project Structure

```
EStore/
├── Controllers/                 # MVC controllers handling requests
│   ├── AccountController.cs     # Authentication & registration
│   ├── AdminController.cs       # Admin dashboard operations
│   ├── CartController.cs        # Shopping cart operations
│   ├── CheckoutController.cs    # Order checkout flow
│   ├── HomeController.cs        # Product listing & details
│   └── UserController.cs        # User profile management
│
├── Models/                      # Data models and DbContext
│   ├── AppDbContext.cs          # Entity Framework DbContext
│   ├── ApplicationUser.cs       # Extended Identity user model
│   ├── Product.cs               # Product entity
│   ├── Category.cs              # Category entity
│   ├── Cart.cs                  # Shopping cart entity
│   ├── Order.cs                 # Order entity
│   ├── AuthViewModels.cs        # Auth view models
│   └── ViewModels.cs            # Other view models
│
├── Views/                       # Razor view templates (.cshtml)
│   ├── Account/                 # Login & registration pages
│   ├── Admin/                   # Admin management pages
│   ├── Cart/                    # Shopping cart view
│   ├── Checkout/                # Checkout & order confirmation
│   ├── Home/                    # Product catalog & details
│   ├── User/                    # User dashboard
│   └── Shared/                  # Layouts & shared components
│
├── Services/                    # Business logic layer
│   ├── DatabaseCartService.cs   # Cart persistence logic
│   ├── DataSeedingService.cs    # Database seeding
│   └── ICartService.cs          # Cart service interface
│
├── Migrations/                  # EF Core migrations
├── wwwroot/                     # Static assets
│   ├── css/                     # Stylesheets
│   ├── js/                      # Client-side scripts
│   └── lib/                     # Third-party libraries
│
├── Properties/                  # Application configuration
├── Program.cs                   # Application entry point
├── appsettings.json             # Configuration (production)
└── appsettings.Development.json # Configuration (development)
```

## Configuration

### appsettings.json

Main configuration file with production defaults:

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EStore;Trusted_Connection=true;"
	},
	"Logging": {
		"LogLevel": {
			"Default": "Information"
		}
	}
}
```

## Development

### Running in Development Mode

```bash
dotnet run
```

The application runs with debug logging and hot-reload enabled.

### Building for Release

```bash
dotnet build -c Release
```

### Running Tests (if implemented)

```bash
dotnet test
```

### Database Migrations

**Create a new migration:**

```bash
dotnet ef migrations add MigrationName
```

**Update database with pending migrations:**

```bash
dotnet ef database update
```

**Revert to previous migration:**

```bash
dotnet ef database update PreviousMigrationName
```

## Deployment

### Publishing for Production

```bash
dotnet publish -c Release -o ./publish
```

### IIS Deployment

1. Install .NET Hosting Bundle on server
2. Publish the application
3. Configure IIS application pool and virtual directory
4. Set connection string to production database

### Docker Deployment (optional)

Create a Dockerfile and deploy as container

## Troubleshooting

### Database Connection Issues

- Verify SQL Server service is running
- Check connection string in `appsettings.json`
- Ensure LocalDB is installed: `sqllocaldb info mssqllocaldb`

### Migrations Not Applying

```bash
# Ensure EF Core tools are installed
dotnet tool install --global dotnet-ef

# Recreate migrations if needed
dotnet ef database drop
dotnet ef database update
```

### Port Already in Use

If ports 5000/5001 are in use:

```bash
dotnet run --urls "https://localhost:5555"
```

### Authentication Issues

- Ensure user exists in database
- Check user role assignments in admin panel
- Clear browser cookies and try again

## Database Schema Overview

| Entity              | Purpose                                           |
| ------------------- | ------------------------------------------------- |
| **ApplicationUser** | User accounts with roles and profile information  |
| **Product**         | Product catalog with pricing and inventory        |
| **Category**        | Product categories with hierarchical structure    |
| **Cart**            | Shopping cart items per user (database-backed)    |
| **Order**           | Customer orders with shipping and payment details |

---

For more information or support, please open an issue on GitHub.
