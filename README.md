# Order Management API

A production-style RESTful API built with ASP.NET Core and PostgreSQL, implementing a scalable order management system with JWT authentication.

## Tech Stack

- **Backend:** ASP.NET Core (.NET 10), C#
- **Database:** PostgreSQL (via Entity Framework Core)
- **Auth:** JWT Bearer Tokens
- **Logging:** Serilog
- **Containerization:** Docker
- **API Docs:** Swagger / OpenAPI

## Features

- JWT-based user registration and login
- Full order lifecycle management (Pending → Processing → Shipped → Delivered → Cancelled)
- Product inventory management with stock tracking
- Database indexing on high-query columns for optimized performance
- Structured logging on all requests
- Global exception handling
- Async/await throughout for non-blocking I/O

## Project Structure
```
OrderManagementAPI/
├── Controllers/        # API endpoints
├── Data/              # DbContext and database configuration
├── DTOs/              # Request and response models
├── Models/            # Database entities
├── Services/          # Business logic
└── Middleware/        # Custom middleware
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Docker

### Run the database
```bash
docker run --name orderapi-db \
  -e POSTGRES_USER=orderapi \
  -e POSTGRES_PASSWORD=orderapi123 \
  -e POSTGRES_DB=orderapidb \
  -p 5432:5432 \
  -d postgres
```

### Run the API
```bash
dotnet run
```

### Open Swagger
```
http://localhost:5017/swagger
```

## API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/Auth/register | Register a new user |
| POST | /api/Auth/login | Login and get JWT token |

### Products
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/Products | Get all products |
| GET | /api/Products/{id} | Get product by ID |
| POST | /api/Products | Create a product |
| PUT | /api/Products/{id} | Update a product |
| DELETE | /api/Products/{id} | Delete a product |

### Orders
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/Orders | Get all orders for logged-in user |
| GET | /api/Orders/{id} | Get order by ID |
| POST | /api/Orders | Create a new order |
| PATCH | /api/Orders/{id}/status | Update order status |

## Database Design

- **Users** — unique index on Email
- **Orders** — unique index on OrderNumber, index on Status and UserId for fast filtering
- **Products** — index on Name for search performance
- **OrderItems** — links orders to products with quantity and price snapshot
