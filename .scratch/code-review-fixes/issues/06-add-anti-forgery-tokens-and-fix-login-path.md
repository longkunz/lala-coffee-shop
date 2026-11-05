# Ticket 06: Bổ sung CSRF Protection [ValidateAntiForgeryToken] và sửa LoginPath

Status: resolved
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

## Kết quả (Answer)
- Đã bổ sung thuộc tính `[ValidateAntiForgeryToken]` cho toàn bộ các action `[HttpPost]` xử lý form:
  - Client Account: [`AccountController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/AccountController.cs) (`Login`, `Register`, `LogOff`, `ExternalLogin`, `ExternalLoginConfirmation`).
  - Checkout Form: [`OrderController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/OrderController.cs) (`CheckOut` POST).
  - Admin Controllers:
    - [`ManageCategoryController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageCategoryController.cs) (`Create`, `Edit`, `Delete`)
    - [`ManageOptionProductController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageOptionProductController.cs) (`Create`, `Edit`, `Delete`)
    - [`ManagePropertyProductController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManagePropertyProductController.cs) (`Create`, `Edit`, `Delete`)
    - [`ManageProductController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageProductController.cs) (`Create`, `Edit`, `Delete`)
    - [`ManageBlogController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageBlogController.cs) (`Create`, `Edit`, `Delete`)
    - [`ManageOrderController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageOrderController.cs) (`EditStatus`)
    - [`ManageRoleController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageRoleController.cs) (`SetRole`)
- Đã cập nhật Cookie Authentication `LoginPath = new PathString("/Account/Login")` trong [`Startup.cs`](file:///D:/UniCafe/LalaCoffee/Startup.cs) để chuyển hướng đúng khi người dùng chưa đăng nhập.
- Đã đồng bộ AntiForgeryToken trong view và Ajax:
  - Bổ sung `@Html.AntiForgeryToken()` vào `_Layout_Admin.cshtml` và các view modal / form của Admin.
  - Cấu hình `ajax.js` trong Admin để tự động đọc `__RequestVerificationToken` và chèn vào form data / request header khi thực hiện POST.
  - Bổ sung `@Html.AntiForgeryToken()` vào `Views/Order/CheckOut.cshtml`.
- Ngoại lệ an toàn: Các endpoint webhook/callback thanh toán bên thứ ba (VNPay, MoMo) trong `OrderController` không yêu cầu AntiForgeryToken (xác thực qua chữ ký số Checksum HMAC).
- Đã bổ sung bộ unit test toàn diện [`CsrfProtectionTests.cs`](file:///D:/UniCafe/LalaCoffee.Tests/CsrfProtectionTests.cs) để kiểm tra tính hiện diện của `[ValidateAntiForgeryToken]` và kiểm tra cấu hình `LoginPath`.

## Comments
