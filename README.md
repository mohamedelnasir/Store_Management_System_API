# Store Management System — API

ASP.NET Core 8 Web API for the Store Management System (products, categories, inventory,
sales, expenses, employees, payroll). Built per the project spec, following the
"Development Order": Models → DbContext → Migration → Auth (JWT) → Users → Categories →
Products → ... (remaining modules to follow).

## What's included (complete)

Every module from the spec is now built:

- **Auth**: Register / Login with JWT + BCrypt password hashing
- **User Management**: full CRUD — Admin-only
- **Category Management**: full CRUD — Admin/Manager manage, everyone can view
- **Product Management**: full CRUD with search/filter/low-stock — Admin/Manager manage, Cashier can view/search only (no delete)
- **Inventory Management**: transaction history (filterable by product), manual stock movements
  (Purchase/Return/Adjustment/Damaged), low-stock list — Admin/Manager only
- **Sales Management**: create a sale (check stock → calculate total → save invoice + items →
  reduce stock → log inventory transaction, one atomic save), view history/invoices —
  Cashier creates sales, Admin/Manager can also view history
- **Expense Management**: full CRUD + monthly summary by category — Admin/Manager
- **Employee Management**: full CRUD — **Admin-only** (see note below)
- **Payroll**: generate payroll per employee/month (`NetSalary = Salary + Bonus - Deduction`),
  view history — **Admin-only**
- **Dashboard**: total sales/expenses/profit, product & employee counts, today's/monthly sales,
  low-stock list — Admin/Manager
- **Reports**: Sales, Expense, Inventory, Payroll, and Profit & Loss reports, each filterable by
  `period=daily|weekly|monthly|custom` (custom needs `from`/`to`) — Admin/Manager, except the
  Payroll report which is Admin-only
- Global exception-handling middleware → consistent JSON error responses
- Role-based authorization matching the permission matrix in the spec throughout
- Swagger/OpenAPI with JWT "Bearer" auth support built in

## Judgment calls worth knowing about

- **Employee & Payroll management are Admin-only.** The spec's permission matrix lists "Manage
  employees" and "Manage payroll" under Admin but doesn't list either under Manager — so both
  modules are locked to Admin. If you want Managers to view (not edit) employee records, that's
  a small change to one `[Authorize]` attribute.
- **Deleting an employee with payroll history is blocked** (409 Conflict) rather than cascading
  the delete, so past payroll records are never silently destroyed. Deactivate the employee
  (`isActive: false`) instead.
- **`POST /api/inventory/adjust` can't create `Sale`-type transactions** — those only come from
  `POST /api/sales`, so stock levels and their audit trail can't drift out of sync.
- **Two different "profit" numbers exist on purpose:**
  - `GET /api/dashboard` → `TotalProfit` = Total Sales − Total Expenses (quick, at-a-glance number)
  - `GET /api/reports/profit-and-loss` → proper Revenue − COGS = Gross Profit, then − Expenses =
    Net Profit. COGS uses each product's **current** buy price as an approximation, since the
    buy price isn't snapshotted onto the sale item at the time of sale. Worth adding if you need
    fully accurate historical P&L (store `BuyPriceAtSale` on `SaleItem`).
- **Invoice numbers** are `INV-{timestamp}-{random}` rather than a strict sequential counter —
  fine at this scale; a high-concurrency production system would want an atomic per-day/store
  sequence instead.
- **`POST /api/auth/register` is open** and lets the caller pick their own role, per the spec's
  Authentication section. Before going live, either restrict it to Admin-only via `/api/users`,
  or force self-registrations to always land as `Cashier`.

## All endpoints

```
POST   /api/auth/register
POST   /api/auth/login

GET    /api/users                      (Admin)
GET    /api/users/{id}                 (Admin)
POST   /api/users                      (Admin)
PUT    /api/users/{id}                 (Admin)
DELETE /api/users/{id}                 (Admin)

GET    /api/categories                 (all roles)
GET    /api/categories/{id}            (all roles)
POST   /api/categories                 (Admin, Manager)
PUT    /api/categories/{id}            (Admin, Manager)
DELETE /api/categories/{id}            (Admin, Manager)

GET    /api/products                   (all roles)   ?search=&categoryId=&lowStockOnly=
GET    /api/products/{id}              (all roles)
POST   /api/products                   (Admin, Manager)
PUT    /api/products/{id}              (Admin, Manager)
DELETE /api/products/{id}              (Admin, Manager)

GET    /api/inventory/history          (Admin, Manager)  ?productId=
GET    /api/inventory/low-stock        (Admin, Manager)
POST   /api/inventory/adjust           (Admin, Manager)

POST   /api/sales                      (Admin, Manager, Cashier)
GET    /api/sales                      (Admin, Manager)  ?from=&to=&invoiceNumber=
GET    /api/sales/{id}                 (Admin, Manager)

GET    /api/expenses                   (Admin, Manager)  ?from=&to=
GET    /api/expenses/{id}              (Admin, Manager)
GET    /api/expenses/monthly-summary   (Admin, Manager)  ?year=&month=
POST   /api/expenses                   (Admin, Manager)
PUT    /api/expenses/{id}              (Admin, Manager)
DELETE /api/expenses/{id}              (Admin, Manager)

GET    /api/employees                  (Admin)  ?search=
GET    /api/employees/{id}             (Admin)
POST   /api/employees                  (Admin)
PUT    /api/employees/{id}             (Admin)
DELETE /api/employees/{id}             (Admin)

GET    /api/payroll                    (Admin)  ?employeeId=&month=&year=
GET    /api/payroll/{id}               (Admin)
POST   /api/payroll/generate           (Admin)

GET    /api/dashboard                  (Admin, Manager)

GET    /api/reports/sales              (Admin, Manager)  ?period=daily|weekly|monthly|custom&from=&to=
GET    /api/reports/expenses           (Admin, Manager)  ?period=...
GET    /api/reports/inventory          (Admin, Manager)  ?period=...
GET    /api/reports/payroll            (Admin)           ?period=...
GET    /api/reports/profit-and-loss    (Admin, Manager)  ?period=...
```

## Requirements

- .NET 8 SDK
- SQL Server (LocalDB, SQL Express, or full SQL Server) — or swap the provider if you'd rather use PostgreSQL/SQLite

## Setup

1. **Restore packages**
   ```bash
   dotnet restore
   ```

2. **Configure the connection string**
   Edit `appsettings.json` → `ConnectionStrings:DefaultConnection` to point at your SQL Server instance.
   Also change `JwtSettings:Key` to a long random secret (32+ characters) before deploying anywhere real.

3. **Create the database**
   ```bash
   dotnet tool install --global dotnet-ef   # if you don't have it already
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the API**
   ```bash
   dotnet run
   ```
   Swagger UI will open at `https://localhost:{port}/swagger` (Development environment).

5. **Try it out**
   - `POST /api/auth/register` with a body like:
     ```json
     {
       "fullName": "Admin User",
       "email": "admin@store.com",
       "password": "Password123",
       "role": 0
     }
     ```
     (`role`: `0` = Admin, `1` = Manager, `2` = Cashier)
   - `POST /api/auth/login` to get a JWT token.
   - Click "Authorize" in Swagger and paste `Bearer {token}` to call the protected endpoints.

## Security note

`POST /api/auth/register` is open (per the spec's Authentication section) and lets a caller
pick their own role — that's fine for early development/testing, but before going live you'll
likely want to either restrict it to Admin-only creation via `/api/users`, or force new
self-registrations to always land as `Cashier` and have an Admin promote them.

## Project structure

```
StoreManagementSystem/
├── Controllers/       # API endpoints
├── Services/           # Business logic
├── Interfaces/         # Service contracts (for DI/testability)
├── Models/              # EF Core entities
│   └── Enums/
├── DTOs/                 # Request/response shapes, grouped by feature
├── Data/                  # ApplicationDbContext
├── Configurations/        # Strongly-typed settings (JwtSettings)
├── Middleware/             # Global exception handling
└── Program.cs              # App startup / DI wiring
```

## Build verification note

This project couldn't be `dotnet restore`-verified in the sandbox it was built in (no access
to nuget.org from that environment), so run `dotnet restore && dotnet build` as your first
step to confirm everything compiles cleanly in your environment. The code was written and
reviewed carefully, but a first-run compile check is still worth doing.
