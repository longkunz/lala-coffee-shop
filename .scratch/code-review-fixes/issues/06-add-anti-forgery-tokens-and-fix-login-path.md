# Ticket 06: Bổ sung CSRF Protection [ValidateAntiForgeryToken] và sửa LoginPath

Status: ready-for-agent
Type: task

## Vấn đề
- Toàn bộ các action `[HttpPost]` trong dự án (`AccountController`, `ManageProductController`, `ManageOrderController`, `ManageCategoryController`,...) đều không khai báo thuộc tính `[ValidateAntiForgeryToken]`, dù các view `.cshtml` có tạo thẻ `@Html.AntiForgeryToken()`. Dẫn đến hệ thống không có khả năng chống tấn công CSRF.
- Tại [`Startup.cs`](file:///D:/UniCafe/UniCafe/Startup.cs#L19), cấu hình Cookie Authentication khai báo:
  `LoginPath = new PathString("/Home/Index")`.
  Khi người dùng chưa đăng nhập truy cập vào các trang Admin cần quyền `[Authorize]`, họ bị chuyển hướng về trang chủ thay vì trang đăng nhập `/Account/Login`.

## Yêu cầu kỹ thuật
1. Bổ sung `[ValidateAntiForgeryToken]` cho toàn bộ các action `[HttpPost]` có xử lý dữ liệu form (Login, Register, Create, Edit, Delete).
2. Sửa `LoginPath` trong [`Startup.cs`](file:///D:/UniCafe/UniCafe/Startup.cs#L19) thành `new PathString("/Account/Login")`.
3. Kiểm tra các form trong Razor view xem đã có `@Html.AntiForgeryToken()` đầy đủ chưa, nếu thiếu thì bổ sung vào bên trong form.

## Comments
