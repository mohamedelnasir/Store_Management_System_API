# Store Management System API

A RESTful API for managing products, inventory, sales, expenses, employees, and payroll, with JWT authentication and role-based access control.

Built with ASP.NET Core 8, Entity Framework Core, and SQL Server.

## Features

- **Authentication** — JWT-based login/register with BCrypt password hashing
- **User Management** — Admin-managed user accounts with role assignment
- **Categories & Products** — Full CRUD with search, filtering, and low-stock detection
- **Inventory** — Transaction history and manual stock adjustments (purchases, returns, damages)
- **Sales** — Invoice creation with automatic stock deduction and inventory logging
- **Expenses** — Categorized expense tracking with monthly summaries
- **Employees & Payroll** — Employee records and payroll generation (`Net Salary = Salary + Bonus - Deduction`)
- **Dashboard** — Key metrics at a glance (sales, expenses, profit, low stock, etc.)
- **Reports** — Sales, expense, inventory, payroll, and profit & loss reports with daily/weekly/monthly/custom date filters
- Swagger/OpenAPI documentation with built-in JWT auth support

## Roles & Permissions

| Role     | Access |
|----------|--------|
| Admin    | Full access to everything, including user management, employees, and payroll |
| Manager  | Manage categories, products, inventory, sales, expenses; view dashboard and most reports |
| Cashier  | Create sales; view and search products |

## Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core + SQL Server
- JWT Bearer Authentication
- BCrypt for password hashing
- Swagger / Swashbuckle

## Project Structure

```
StoreManagementSystem/
├── Controllers/        # API endpoints
├── Services/           # Business logic
├── Interfaces/         # Service contracts
├── Models/             # EF Core entities
│   └── Enums/
├── DTOs/                # Request/response models, grouped by feature
├── Data/                 # ApplicationDbContext
├── Configurations/       # Strongly-typed settings (JWT, etc.)
├── Middleware/           # Global exception handling
└── Program.cs            # App startup and dependency injection
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/StoreManagementSystem.git
   cd StoreManagementSystem
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure `appsettings.json`**
   - Set `ConnectionStrings:DefaultConnection` to your SQL Server instance
   - Set `JwtSettings:Key` to a long, random secret (32+ characters)

4. **Apply migrations**
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   ```
   Swagger UI is available at `https://localhost:{port}/swagger`.

### Quick Test

Register a user, then log in to get a JWT token:

```json
POST /api/auth/register
{
  "fullName": "Admin User",
  "email": "admin@store.com",
  "password": "Password123",
  "role": 0
}
```

`role`: `0` = Admin, `1` = Manager, `2` = Cashier

Use the token from `/api/auth/login` by clicking **Authorize** in Swagger and entering `Bearer {token}`.

## API Endpoints

```
POST   /api/auth/register
POST   /api/auth/login

GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}

GET    /api/categories
GET    /api/categories/{id}
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}

GET    /api/products                 ?search=&categoryId=&lowStockOnly=
GET    /api/products/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}

GET    /api/inventory/history        ?productId=
GET    /api/inventory/low-stock
POST   /api/inventory/adjust

POST   /api/sales
GET    /api/sales                    ?from=&to=&invoiceNumber=
GET    /api/sales/{id}

GET    /api/expenses                 ?from=&to=
GET    /api/expenses/{id}
GET    /api/expenses/monthly-summary ?year=&month=
POST   /api/expenses
PUT    /api/expenses/{id}
DELETE /api/expenses/{id}

GET    /api/employees                ?search=
GET    /api/employees/{id}
POST   /api/employees
PUT    /api/employees/{id}
DELETE /api/employees/{id}

GET    /api/payroll                  ?employeeId=&month=&year=
GET    /api/payroll/{id}
POST   /api/payroll/generate

GET    /api/dashboard

GET    /api/reports/sales            ?period=daily|weekly|monthly|custom&from=&to=
GET    /api/reports/expenses         ?period=...
GET    /api/reports/inventory        ?period=...
GET    /api/reports/payroll          ?period=...
GET    /api/reports/profit-and-loss  ?period=...
```

## Notes

- Two profit figures are exposed: the dashboard shows a quick Sales − Expenses number, while the
  profit & loss report calculates Revenue − Cost of Goods Sold − Expenses for a more accurate view.
- Deleting an employee with existing payroll history is blocked to preserve payroll records;
  deactivate the employee instead.
- Manual inventory adjustments cannot create sale-type transactions — those are generated
  automatically when a sale is made, keeping stock and its audit trail in sync.

## License

MIT
