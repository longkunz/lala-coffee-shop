# 09: Giải phóng kết nối cơ sở dữ liệu và tối ưu hóa truy vấn lặp N+1

**What to build:** Toàn bộ các phiên làm việc và kết nối đến cơ sở dữ liệu được giải phóng triệt để ngay sau khi yêu cầu người dùng hoàn thành, giúp máy chủ duy trì khả năng phục vụ cao mà không bị cạn kiệt Connection Pool. Dữ liệu menu phân cấp được nạp thông minh 1 lần vào bộ nhớ, và các thao tác lưu dữ liệu được gom lại thành 1 lần gọi duy nhất giúp trang web phản hồi tức thì.

**Blocked by:** 07 (Sửa lỗi tính tiền đơn hàng theo số lượng và lưu trữ đơn hàng bền vững trong Database)

**Status:** resolved

- [x] Lớp Controller cơ sở và các controller độc lập thực thi phương thức dọn dẹp tài nguyên (`Dispose`) cho phiên làm việc cơ sở dữ liệu và các trình quản lý danh tính người dùng.
- [x] Dữ liệu cây danh mục cho thanh điều hướng menu được truy vấn toàn bộ trong 1 câu truy vấn và tự động dựng cây phân cấp trong bộ nhớ, loại bỏ hoàn toàn các câu truy vấn lặp lồng nhau trên mỗi lượt tải trang.
- [x] Thao tác lưu các dòng chi tiết đơn hàng được đưa ra ngoài vòng lặp duyệt giỏ hàng, chỉ thực hiện lưu cơ sở dữ liệu 1 lần duy nhất sau khi đã gom đủ dữ liệu.
- [x] Các truy vấn đọc dữ liệu chỉ để hiển thị áp dụng cờ không theo dõi trạng thái (`AsNoTracking`) để tăng tốc độ phản hồi và giảm chiếm dụng bộ nhớ.

## Kết quả (Answer)

Đã hoàn thành trong commit [`ecb9746`](file:///D:/UniCafe). `BaseController` và `AccountController` đã override `Dispose(disposing)` giải phóng `ApplicationDbContext` và `UserManager`. `MenuHelper` sử dụng `ToLookup` in-memory. `OrderController.CheckOut` gom `OrderDetail` và chỉ gọi `SaveChanges()` một lần duy nhất. Kiểm thử toàn diện tại `PerformanceAndQueryTests`.
