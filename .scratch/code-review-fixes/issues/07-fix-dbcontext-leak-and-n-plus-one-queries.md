# Ticket 07: Khắc phục rò rỉ DbContext và tối ưu hóa truy vấn N+1

Status: ready-for-agent
Type: task

## Vấn đề
- Tại [`BaseController.cs`](file:///D:/UniCafe/UniCafe/Controllers/BaseController.cs#L20-L27), `Context = new ApplicationDbContext();` được khởi tạo cho mỗi request nhưng không hề override hàm `Dispose(bool disposing)` để giải phóng tài nguyên.
- Tại [`MenuHelper.cs`](file:///D:/UniCafe/UniCafe/Helpers/MenuHelper.cs#L21-L45), `new ApplicationDbContext()` được mở trực tiếp bên trong View Helper và chạy vòng lặp lồng nhau truy vấn SQL cho từng category cha trên mọi trang hiển thị.
- Tại [`OrderController.cs`](file:///D:/UniCafe/UniCafe/Controllers/OrderController.cs#L183), `Context.SaveChanges()` bị gọi bên trong vòng lặp `foreach (var item in cart)` thay vì lưu 1 lần sau khi kết thúc vòng lặp.
- Tại [`ManageRoleController.cs`](file:///D:/UniCafe/UniCafe/Areas/Admin/Controllers/ManageRoleController.cs#L76-L94), lặp qua danh sách roles và gọi 2 câu truy vấn riêng biệt cho mỗi role bên trong vòng lặp.

## Yêu cầu kỹ thuật
1. Trong [`BaseController.cs`](file:///D:/UniCafe/UniCafe/Controllers/BaseController.cs), bổ sung:
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

## Comments
