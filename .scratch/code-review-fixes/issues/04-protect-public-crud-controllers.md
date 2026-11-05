# Ticket 04: Bảo vệ hoặc loại bỏ các endpoint CRUD công khai ngoài Admin Area

Status: resolved
Type: task

## Vấn đề
- Tại [`CategoryController.cs`](file:///D:/UniCafe/UniCafe/Controllers/CategoryController.cs#L92-L119), [`OptionProductController.cs`](file:///D:/UniCafe/UniCafe/Controllers/OptionProductController.cs#L26-L75), [`PropertyProductController.cs`](file:///D:/UniCafe/UniCafe/Controllers/PropertyProductController.cs#L27-L75):
  - Các Controller này nằm ở thư mục gốc `Controllers/` nhưng vẫn giữ nguyên các action `Create`, `Edit`, `Delete` không có `[Authorize]`.
  - Bất kỳ người dùng ẩn danh nào cũng có thể gửi POST request để thêm/sửa/xóa danh mục, topping và thuộc tính sản phẩm.
  - Trong khi đó, các chức năng quản trị tương ứng đã tồn tại đầy đủ và được bảo vệ trong `Areas/Admin/Controllers/` (`ManageCategoryController`, `ManageOptionProductController`, `ManagePropertyProductController`).

## Yêu cầu kỹ thuật
1. Loại bỏ các action `Create`, `Edit`, `Delete` (cả GET và POST) trong các Controller công khai ở thư mục ngoài (`CategoryController`, `OptionProductController`, `PropertyProductController`).
2. Xóa các file Razor view tương ứng không sử dụng ngoài client (`Views/Category/Edit.cshtml`, `Views/Category/Delete.cshtml`, `Views/OptionProduct/*`, `Views/PropertyProduct/*`).
3. Đảm bảo toàn bộ thao tác quản trị chỉ được phép thực hiện bên trong Area Admin với quyền `[Authorize(Roles = "Admin")]`.

## Kết quả (Answer)
- Đã loại bỏ hoàn toàn các action CRUD nhạy cảm (`Create`, `Edit`, `Delete` cả GET và POST) khỏi các Controller công khai:
  - [`CategoryController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/CategoryController.cs)
  - [`OptionProductController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/OptionProductController.cs)
  - [`PropertyProductController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/PropertyProductController.cs)
- Đã xóa các file view Razor thừa không còn dùng ngoài client:
  - `Views/Category/Edit.cshtml`
  - `Views/Category/Delete.cshtml`
  - `Views/OptionProduct/Edit.cshtml`
  - `Views/OptionProduct/Delete.cshtml`
  - `Views/PropertyProduct/Edit.cshtml`
  - `Views/PropertyProduct/Delete.cshtml`
- Đã dọn dẹp các liên kết Create/Edit/Delete trong các view `Index.cshtml` của 3 controller trên.
- Đã cập nhật file dự án [`LalaCoffee.csproj`](file:///D:/UniCafe/LalaCoffee/LalaCoffee.csproj) để xóa các Item `Content` tương ứng.
- Đã bổ sung bộ kiểm thử tự động [`PublicEndpointsSecurityTests.cs`](file:///D:/UniCafe/LalaCoffee.Tests/PublicEndpointsSecurityTests.cs) để giám sát và bảo đảm 100% không phát sinh public CRUD method ngoài Area Admin.
- Toàn bộ thao tác CRUD quản trị danh mục, topping, thuộc tính sản phẩm chỉ được thực thi tại Area Admin (`ManageCategoryController`, `ManageOptionProductController`, `ManagePropertyProductController`) với quyền `[Authorize(Roles = "Admin")]`.

## Comments
