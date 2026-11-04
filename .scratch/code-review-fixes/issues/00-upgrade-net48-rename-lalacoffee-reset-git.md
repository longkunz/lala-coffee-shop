# Ticket 00: Nâng lên .NET 4.8, Đổi tên toàn diện sang LalaCoffee và Reset Git sang repo mới

Status: resolved
Type: task

## Vấn đề & Bối cảnh
- Dự án ban đầu mang tên UniCafe với target framework .NET 4.7.2.
- Người dùng yêu cầu chuyển đổi chính thức sang dự án **LalaCoffee**, nâng target framework lên **.NET Framework 4.8**, xóa bỏ toàn bộ lịch sử Git cũ và trỏ sang repository mới: `https://github.com/longkunz/lala-coffee-shop.git`.

## Các bước thực hiện chi tiết

### 1. Nâng cấp Target Framework lên .NET 4.8
- Mở file `.csproj` và cập nhật:
  `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>`
- Cập nhật `Web.config`:
  `<compilation debug="false" targetFramework="4.8" />`
  `<httpRuntime targetFramework="4.8" ... />`
- Cập nhật `packages.config` các thuộc tính `targetFramework="net48"`.

### 2. Đổi tên toàn diện dự án sang LalaCoffee
- Đổi tên solution thành `LalaCoffee.sln`.
- Đổi tên thư mục `UniCafe/` thành `LalaCoffee/` (hoặc đồng bộ đường dẫn tương ứng trong Solution).
- Đổi tên file project thành `LalaCoffee.csproj`.
- Cập nhật thông tin trong project file:
  `<RootNamespace>LalaCoffee</RootNamespace>`
  `<AssemblyName>LalaCoffee</AssemblyName>`
- Refactor đổi tên toàn bộ namespace từ `UniCafe` thành `LalaCoffee`:
  - Toàn bộ các file trong `Controllers/`, `Areas/Admin/Controllers/`
  - Toàn bộ các file trong `Models/`, `Data/`, `Services/`, `Helpers/`, `App_Start/`
  - File `Global.asax`, `Global.asax.cs`, `Startup.cs`
- Cập nhật toàn bộ các file Views `.cshtml` và `Views/web.config`:
  - Thay thế `@using UniCafe...` và `@model UniCafe...` thành `@using LalaCoffee...` và `@model LalaCoffee...`
  - Thay thế các namespace khai báo trong `Views/web.config` và `Areas/Admin/Views/web.config`.

### 3. Làm mới lịch sử Git & Cấu hình Remote mới
- Xóa bỏ hoàn toàn thư mục `.git/`.
- Khởi tạo Git mới với nhánh mặc định là `main`:
  `git init -b main`
- Thiết lập remote origin:
  `git remote add origin https://github.com/longkunz/lala-coffee-shop.git`
- Kiểm tra và đảm bảo `.gitignore` hoàn chỉnh (chặn `.vs`, `bin/`, `obj/`, `packages/`, `*.swp`, `*.user`).
- Tạo commit ban đầu: `git add .` và `git commit -m "feat: initial commit for LalaCoffee (.NET 4.8)"`.

## Comments

## Kết quả (Answer)
- Đã nâng cấp Target Framework lên .NET 4.8 trong `LalaCoffee.csproj`, `Web.config`, và toàn bộ `packages.config` (`net48`).
- Đã đổi tên Solution thành `LalaCoffee.sln`, đổi tên thư mục sang `LalaCoffee/`, đổi tên project thành `LalaCoffee.csproj`.
- Đã cập nhật RootNamespace và AssemblyName thành `LalaCoffee`.
- Đã refactor toàn bộ namespace `UniCafe` thành `LalaCoffee` trên 133 file C#, Razor views, `Global.asax`, `Startup.cs`, `Views/web.config`, `Areas/Admin/Views/web.config`.
- Đã xóa sạch `.git/`, khởi tạo git mới với nhánh `main`, cấu hình origin `https://github.com/longkunz/lala-coffee-shop.git`.
- Cấu hình `.gitignore` đầy đủ cho Visual Studio và bảo vệ `secrets.config`.
- Build MSBuild thành công 0 lỗi. Đã tạo initial commit trên nhánh `main`.
