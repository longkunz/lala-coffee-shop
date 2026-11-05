# Ticket 07: Khắc phục rò rỉ DbContext và tối ưu hóa truy vấn N+1

Status: resolved
Type: task

## Vấn đề
- Tại [`BaseController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/BaseController.cs#L20-L27), `Context = new ApplicationDbContext();` được khởi tạo cho mỗi request nhưng không hề override hàm `Dispose(bool disposing)` để giải phóng tài nguyên.
- Tại [`MenuHelper.cs`](file:///D:/UniCafe/LalaCoffee/Helpers/MenuHelper.cs#L21-L45), `new ApplicationDbContext()` được mở trực tiếp bên trong View Helper và chạy vòng lặp lồng nhau truy vấn SQL cho từng category cha trên mọi trang hiển thị.
- Tại [`OrderController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/OrderController.cs#L183), `Context.SaveChanges()` bị gọi bên trong vòng lặp `foreach (var item in cart)` thay vì lưu 1 lần sau khi kết thúc vòng lặp.
- Tại [`ManageRoleController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageRoleController.cs#L76-L94), lặp qua danh sách roles và gọi 2 câu truy vấn riêng biệt cho mỗi role bên trong vòng lặp.

## Yêu cầu kỹ thuật
1. Trong [`BaseController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/BaseController.cs), bổ sung:
   ```csharp
   protected override void Dispose(bool disposing)
   {
       if (disposing)
       {
           Context?.Dispose();
           UnitOfWork?.Dispose();
           UserManager?.Dispose();
           RoleManager?.Dispose();
       }
       base.Dispose(disposing);
   }
   ```
2. Đưa `Context.SaveChanges()` ra khỏi vòng lặp `foreach` của `OrderDetail`.
3. Tối ưu truy vấn danh mục trong `MenuHelper.cs`: Truy vấn toàn bộ danh mục một lần (`var allCategories = context.Categories.ToList()`), sau đó phân cấp cha - con trên bộ nhớ (In-memory LINQ).
4. Tối ưu hóa truy vấn gán quyền trong `ManageRoleController.SetRole` bằng LINQ `Join`.

## Kết quả (Answer)
- Đã khắc phục triệt để nguy cơ rò rỉ kết nối và bộ nhớ (DbContext & Service Leaks):
  - Override `Dispose(bool disposing)` trong [`BaseController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/BaseController.cs) để giải phóng an toàn `Context`, `UnitOfWork`, `UserManager`, và `RoleManager` theo chu kỳ sống của controller MVC.
  - Override `Dispose(bool disposing)` trong [`AccountController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/AccountController.cs) để giải phóng trường `_context` nội bộ.
- Khắc phục vòng lặp lưu dữ liệu (N+1 SaveChanges) trong [`OrderController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/OrderController.cs):
  - Gom toàn bộ chi tiết đơn hàng vào danh sách `List<OrderDetail>` và nạp hàng loạt bằng `Context.OrderDetails.AddRange(orderDetails)`.
  - Chỉ gọi duy nhất một lần `Context.SaveChanges()` sau khi kết thúc toàn bộ vòng lặp nạp chi tiết đơn hàng.
- Khắc phục N+1 query lồng nhau trong View Helper [`MenuHelper.cs`](file:///D:/UniCafe/LalaCoffee/Helpers/MenuHelper.cs):
  - Loại bỏ hoàn toàn việc truy vấn cơ sở dữ liệu con lặp đi lặp lại cho từng danh mục cha.
  - Thực hiện duy nhất 1 câu truy vấn `context.Categories.AsNoTracking().Where(c => c.ShowHome == true).ToList()`.
  - Phân tách và nhóm cấu trúc phân cấp cha - con hoàn toàn trên bộ nhớ bằng LINQ `ToLookup(c => c.ParentId)`.
  - Tách logic render HTML thành các phương thức thuần (`BuildMenuHtml`, `RenderMenuHtml`) hỗ trợ nhận dữ liệu từ ngoài phục vụ unit testing không phụ thuộc database.
- Tối ưu hóa truy vấn phân quyền trong [`ManageRoleController.cs`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Controllers/ManageRoleController.cs):
  - Loại bỏ 2 câu truy vấn SQL độc lập bên trong mỗi vòng lặp `foreach (var role in roles)`.
  - Thay thế bằng việc truy vấn trước các liên kết quyền và role IDs cần thiết, kết hợp lọc in-memory với `HashSet<string>` và `Dictionary`/`Join`, giảm thiểu số lượt round-trip tới DB.
- Bổ sung bộ kiểm thử tự động [`DbContextOptimizationTests.cs`](file:///D:/UniCafe/LalaCoffee.Tests/DbContextOptimizationTests.cs) (7 test cases):
  - `BaseController_OverridesDispose_WithDisposingParameter`: Kiểm tra `BaseController<T>` có khai báo override `Dispose(bool)`.
  - `AccountController_OverridesDispose_WithDisposingParameter`: Kiểm tra `AccountController` có khai báo override `Dispose(bool)`.
  - `OrderController_CheckOut_DoesNotCallSaveChangesInsideForeach`: Static source audit đảm bảo `SaveChanges()` không nằm trong vòng lặp `foreach`.
  - `ManageRoleController_SetRole_DoesNotQueryDatabaseInsideLoop`: Static source audit đảm bảo `SetRole()` không gọi DB trong vòng lặp.
  - `MenuHelper_RenderMenuHtml_BuildsHierarchicalHtml_WithoutDatabaseQuery`: Xác thực khả năng render phân cấp danh mục chính xác in-memory.
  - `MenuHelper_RenderMenuHtml_HandlesEmptyAndNullGracefully`: Kiểm tra xử lý ngoại lệ an toàn khi dữ liệu danh mục rỗng hoặc null.
  - `MenuHelper_RenderMenuHtml_HandlesOrphanedSubCategoriesGracefully`: Kiểm tra xử lý an toàn cho danh mục con mồ côi (không có cha).
- Toàn bộ 115/115 unit tests trong `LalaCoffee.Tests` đều vượt qua thành công (100% pass), solution biên dịch không có lỗi (0 errors).

## Comments
- Toàn bộ các yêu cầu của Ticket 07 đã được hoàn thiện và kiểm thử tự động đầy đủ.
