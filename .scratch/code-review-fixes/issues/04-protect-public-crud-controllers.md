# Ticket 04: Bảo vệ hoặc loại bỏ các endpoint CRUD công khai ngoài Admin Area

Status: claimed
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

## Comments
