# LalaCoffee

Experience the rich and aromatic world of coffee at our online store.

LalaCoffee (trước đây là UniCafe) is a full-featured e-commerce web application for a coffee shop, built with ASP.NET MVC 5. It includes product browsing, shopping cart, cash-on-delivery (COD) checkout, blog management, and a complete admin dashboard.

![Homepage](screenshots/homepage.png)

## Tech Stack

- **Framework:** ASP.NET MVC 5 (.NET Framework 4.8)
- **ORM:** Entity Framework 6 (Code First)
- **Database:** SQL Server
- **Authentication:** ASP.NET Identity with OWIN Cookie Authentication
- **Admin Dashboard:** AdminLTE 3
- **Frontend:** Bootstrap 3, jQuery 3.6
- **Payment:** COD (Cash on Delivery)
- **DI Container:** Unity 5

## Prerequisites

- Visual Studio 2017 or later (recommended: Visual Studio 2022)
- .NET Framework 4.8 Developer Pack
- SQL Server (LocalDB, SQL Server Express, or Docker)

## Getting Started

### Step 1: Clone the repository

```bash
git clone https://github.com/longkunz/lala-coffee-shop.git
cd lala-coffee-shop
```

### Step 2: Open the solution

Open `LalaCoffee.sln` with Visual Studio.

### Step 3: Restore NuGet packages

Visual Studio should automatically restore packages on open. If not:

1. Right-click the **Solution** in Solution Explorer
2. Select **Restore NuGet Packages**

### Step 4: Configure the database connection

Open `LalaCoffee/Web.config` and update the connection string to match your SQL Server instance:

**Option A: Using SQL Server Express (local)**

```xml
<add name="DefaultConnection"
     connectionString="Server=.\SQLEXPRESS;Initial Catalog=UniCafe;Integrated Security=True;"
     providerName="System.Data.SqlClient" />
```

**Option B: Using LocalDB**

```xml
<add name="DefaultConnection"
     connectionString="Server=(localdb)\MSSQLLocalDB;Initial Catalog=UniCafe;Integrated Security=True;"
     providerName="System.Data.SqlClient" />
```

**Option C: Using Docker SQL Server**

```xml
<add name="DefaultConnection"
     connectionString="Server=localhost,1433;Initial Catalog=UniCafe;User Id=sa;Password=YourPassword;"
     providerName="System.Data.SqlClient" />
```

### Step 5: Create the database

Run the deployment script `Database_Deploy.sql` at the repository root with SQL Server Management Studio (SSMS) or sqlcmd. The script creates the `UniCafe` database, the full schema and all seed data (categories, products, blogs and the default accounts below):

```
sqlcmd -S .\SQLEXPRESS -E -i Database_Deploy.sql
```

> **Do NOT use `Update-Database` (EF migrations) to create the database.** The existing migrations do not include the `FullName`/`Address` columns of `ApplicationUser`, so a migration-built database throws `Invalid column name` on login. `Database_Deploy.sql` already contains the final schema plus those columns. The script also refuses to run against a non-empty database — drop the database first if you need to re-create it.

### Step 6: Run the application

Press **F5** (Debug) or **Ctrl+F5** (Run without debugging).

The application will launch in your default browser using IIS Express.

## Default Accounts / Tài khoản mặc định

Sau khi khởi tạo database bằng script `Database_Deploy.sql` (xem Step 5), bạn có thể sử dụng các tài khoản có sẵn dưới đây:

| Vai trò | Tên đăng nhập (Username) | Email | Mật khẩu | Quyền (Role) | Khu vực truy cập |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin` | `admin@lalacoffee.vn` | `Admin@123` | `Admin` | `/Account/Login` hoặc trực tiếp `/Admin` |
| **Khách hàng (User)** | `user` | `user@lalacoffee.vn` | `User@123` | `User` | `/Account/Login` (đặt hàng, xem đơn cá nhân) |


## Project Structure

```
lala-coffee-shop/
├── LalaCoffee.sln                 # Solution file
└── LalaCoffee/                    # Main project
    ├── App_Start/                 # App configuration
    │   ├── BundleConfig.cs        # CSS/JS bundling
    │   ├── FilterConfig.cs        # Global filters
    │   └── RouteConfig.cs         # URL routing
    ├── Areas/
    │   └── Admin/                 # Admin area
    │       └── Controllers/       # Admin controllers
    │           ├── ManageProductController.cs
    │           ├── ManageCategoryController.cs
    │           ├── ManageOrderController.cs
    │           ├── ManageBlogController.cs
    │           ├── ManageRoleController.cs
    │           ├── ManageUserController.cs
    │           └── StatsController.cs
    ├── Controllers/               # Public controllers
    │   ├── HomeController.cs
    │   ├── ProductController.cs
    │   ├── CartController.cs
    │   ├── OrderController.cs
    │   ├── AccountController.cs
    │   ├── BlogController.cs
    │   └── CategoryController.cs
    ├── Models/                    # Data models
    │   ├── Product.cs
    │   ├── Category.cs
    │   ├── Order.cs
    │   ├── OrderDetail.cs
    │   ├── Blog.cs
    │   ├── CartItem.cs
    ├── Data/                      # Data access layer
    │   ├── ApplicationDbContext.cs
    │   ├── ApplicationUser.cs
    │   ├── Repository.cs          # Generic repository
    │   └── UnitOfWork.cs
    ├── Services/                  # Business services
    │   ├── CartManager.cs
    ├── Views/                     # Razor views
    ├── Content/                   # Static assets
    │   ├── images/                # Product images
    │   └── Layout/Admin/          # AdminLTE template
    ├── Migrations/                # EF Code First migrations
    ├── Startup.cs                 # OWIN startup
    ├── Global.asax.cs             # Application entry point
    └── Web.config                 # App configuration
```

## Features

**Customer-facing:**
- Browse products by category
- Product detail pages
- Shopping cart (add, update, remove items)
- Checkout with Cash on Delivery (COD)
- User registration and login
- Blog/news section

**Admin dashboard:**
- Product management (CRUD)
- Category management
- Order management
- Blog management
- User and role management
- Statistics/reports

## Payment Method

The application supports **Cash on Delivery (COD)** payment.

## License

This project is for educational purposes.
