# Ticket 08: Dọn dẹp tàn dư Git conflict, file swap và mã nguồn chết (Dead Code)

Status: resolved
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
- Đã xóa 4 file conflict `ManageCategoryController_*_273.cs` trong `LalaCoffee/Areas/Admin/Controllers/`.
- Đã xóa controller chết `LalaCoffee/Controllers/CollectionController.cs` và thư mục view `LalaCoffee/Views/Collection/`.
- Đã loại bỏ 2 model dư thừa `LalaCoffee/Models/User.cs` và `LalaCoffee/Models/Role.cs`. Chuyển `ManageRoleController` sang kế thừa `BaseController<IdentityRole>` và cập nhật view liên quan.
- Đã chuẩn hóa namespace `AccountController` về `LalaCoffee.Controllers`.
- Đã xóa các tham chiếu tương ứng trong `LalaCoffee.csproj`.
- Đã bổ sung bộ test tự động `LalaCoffee.Tests/CleanupTests.cs` xác nhận không còn file rác và `AccountController` thuộc đúng namespace.

## Kết quả (Answer)
- Đã xóa triệt để 4 file Git conflict sót lại trong thư mục `LalaCoffee/Areas/Admin/Controllers/`: `ManageCategoryController_BACKUP_273.cs`, `ManageCategoryController_BASE_273.cs`, `ManageCategoryController_LOCAL_273.cs`, `ManageCategoryController_REMOTE_273.cs`.
- Đã xóa dead controller `CollectionController.cs` (bị comment out 100%) và view đi kèm `LalaCoffee/Views/Collection/Index.cshtml`.
- Đã dọn dẹp các class model trùng lặp không sử dụng `LalaCoffee/Models/User.cs` và `LalaCoffee/Models/Role.cs`. Chuẩn hóa `ManageRoleController` sử dụng `IdentityRole` từ ASP.NET Identity và cập nhật `ManageRole/Index.cshtml`.
- Đã chuẩn hóa namespace của `AccountController` từ `UniCafe.Data` về `LalaCoffee.Controllers`.
- Đã gỡ bỏ toàn bộ tham chiếu file chết/conflict khỏi project file `LalaCoffee.csproj`.
- Đã viết unit test tự động `LalaCoffee.Tests/CleanupTests.cs` kiểm tra đảm bảo không còn file rác/conflict trên disk và xác minh `AccountController` nằm đúng namespace `LalaCoffee.Controllers`. Toàn bộ 49/49 test cases trong solution đều PASS.
