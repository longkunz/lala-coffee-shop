# Ticket 08: Dọn dẹp tàn dư Git conflict, file swap và mã nguồn chết (Dead Code)

Status: ready-for-agent
Type: task

## Vấn đề
- Thư mục [`UniCafe/Areas/Admin/Controllers/`](file:///D:/UniCafe/UniCafe/Areas/Admin/Controllers/) chứa hàng loạt file conflict và file swap của Vim bị commit vào Git:
  - `ManageCategoryController_BACKUP_273.cs`
  - `ManageCategoryController_LOCAL_273.cs`
  - `ManageCategoryController_REMOTE_273.cs`
  - `ManageCategoryController_BASE_273.cs`
  - `.ManageCategoryController.cs.swp`
  - `.ManageCategoryController_LOCAL_273.cs.swp`
  - `.ManageCategoryController_REMOTE_273.cs.swp`
  - `.ManageCategoryController_BASE_273.cs.swp`
- File [`CollectionController.cs`](file:///D:/UniCafe/UniCafe/Controllers/CollectionController.cs) bị comment out 100% (dead code).
- Trùng lặp class vô nghĩa: `ApplicationUser` và `User`, `ApplicationRole` và `Role`.
- Lộn xộn namespace: `AccountController.cs` mang namespace `UniCafe.Data`.
- Cấu hình `compilation debug="true"` trong `Web.config` chưa được tắt cho môi trường Production.

## Yêu cầu kỹ thuật
1. Xóa toàn bộ các file `*_273.cs` và `*.swp` trong `UniCafe/Areas/Admin/Controllers/`.
2. Xóa hoặc xử lý file controller chết `CollectionController.cs`.
3. Chuẩn hóa namespace của `AccountController` về `UniCafe.Controllers`.
4. Đảm bảo file `.gitignore` bỏ qua các định dạng file tạm của Vim/Visual Studio (`*.swp`, `*.suo`, `*.user`).

## Comments
