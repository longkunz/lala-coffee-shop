# Bản đồ Nhiệm vụ: Chuyển Đổi Toàn Diện LalaCoffee (.NET 4.8)

Tài liệu này theo dõi tiến trình triển khai bản đặc tả `lala-coffee-transformation` theo quy ước wayfinding và issue tracker.

## Ghi chú (Notes)
- Nền tảng mục tiêu: .NET Framework 4.8, ASP.NET MVC 5, Entity Framework 6.
- Remote repository chính thức: `https://github.com/longkunz/lala-coffee-shop.git`.
- Giai đoạn 0 (Nền tảng & Thương hiệu) và Giai đoạn 1 (Khắc phục lỗi bảo mật & nghiệp vụ) đã được triển khai và kiểm chứng (118 test cases passed).
- Giai đoạn 2 (Hiện đại hóa kiến trúc & Giao diện) gồm Ticket 10 (Unity DI + Service Layer) và Ticket 11 (Bootstrap 5 qua LibMan + NLog).

## Quyết định đã chốt (Decisions Made)
- [x] **Ticket 01**: Nâng cấp Target Framework lên .NET 4.8 (`LalaCoffee.csproj`), đồng bộ NuGet packages lên `net48`, dọn sạch lịch sử và khởi tạo git trên nhánh `main`. Chi tiết: [`01-upgrade-net48-and-init-git.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/01-upgrade-net48-and-init-git.md).
- [x] **Ticket 02**: Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện (`LalaCoffee.sln`, `LalaCoffee.csproj`, root namespace `LalaCoffee.*`, Razor views). Chi tiết: [`02-rename-project-to-lalacoffee.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/02-rename-project-to-lalacoffee.md).
- [x] **Ticket 03**: Dọn dẹp tàn dư Git conflict (`*_BACKUP_*`, etc.), file swap (`.swp`), loại bỏ controller và model chết (`CollectionController.cs`, `User.cs`, `Role.cs`). Chi tiết: [`03-clean-conflict-artifacts-and-dead-code.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/03-clean-conflict-artifacts-and-dead-code.md).
- [x] **Ticket 04**: Bảo mật chức năng tải ảnh sản phẩm (`ImageController`), phân quyền Admin, whitelist định dạng, giới hạn 5MB, sinh tên GUID ngẫu nhiên ngăn RCE/Path Traversal. Chi tiết: [`04-secure-image-upload-against-rce.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/04-secure-image-upload-against-rce.md).
- [x] **Ticket 05**: Đóng các endpoint CRUD công khai ngoài client (`CategoryController`, `OptionProductController`, `PropertyProductController`), dọn view CRUD mồ côi, tập trung quyền quản trị tại Area Admin. Chi tiết: [`05-close-public-crud-endpoints.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/05-close-public-crud-endpoints.md).
- [x] **Ticket 06**: Kích hoạt `[ValidateAntiForgeryToken]` cho toàn bộ action POST xử lý form, cập nhật `LoginPath` chuẩn `/Account/Login`, đồng bộ token trong form và AJAX. Chi tiết: [`06-add-csrf-protection-and-fix-login-redirect.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/06-add-csrf-protection-and-fix-login-redirect.md).
- [x] **Ticket 07**: Sửa lỗi tính tiền đơn hàng theo `Quantity` qua `OrderCalculator`, lưu đơn hàng vào DB ngay lúc Checkout với trạng thái `Pending` (`Paid = 0`), gán mã `OrderCode` duy nhất. Chi tiết: [`07-fix-order-quantity-calculation-and-db-persistence.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/07-fix-order-quantity-calculation-and-db-persistence.md).
- [x] **Ticket 08**: Xác thực chữ ký số 2 chiều cho MoMo (HMAC-SHA256) và VNPay (HMAC-SHA512), tách secrets sang `secrets.config` và environment variables. Chi tiết: [`08-verify-payment-signatures-for-momo-and-vnpay.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/08-verify-payment-signatures-for-momo-and-vnpay.md).
- [x] **Ticket 09**: Khắc phục rò rỉ DbContext bằng `Dispose(disposing)` trong controllers, tối ưu hóa truy vấn N+1 menu bằng `ToLookup` in-memory, gom `SaveChanges()` đơn hàng 1 lần. Chi tiết: [`09-fix-dbcontext-leak-and-n-plus-one-queries.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/09-fix-dbcontext-leak-and-n-plus-one-queries.md).
- [x] **Ticket 10**: Tách tầng Service Layer (`IOrderService`, `ICartService`, `IPaymentService`, `IEmailService`), chuyển sang `HttpClient` hiện đại, cấu hình Unity DI DependencyResolver và Constructor Injection cho Controllers. Chi tiết: [`10-extract-service-layer-and-integrate-unity-di.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/10-extract-service-layer-and-integrate-unity-di.md).
- [x] **Ticket 11**: Nâng cấp UI sang Bootstrap 5 qua LibMan, cập nhật data attributes `data-bs-*`, tích hợp NLog rolling file logger và trang xử lý lỗi bảo mật không rò rỉ stack trace. Chi tiết: [`11-upgrade-ui-to-bootstrap5-and-integrate-nlog.md`](file:///D:/UniCafe/.scratch/lala-coffee-transformation/issues/11-upgrade-ui-to-bootstrap5-and-integrate-nlog.md).

## Danh sách Ticket & Trạng thái hiện tại
- [x] Ticket 01: Nâng cấp Target Framework lên .NET 4.8 và Thiết lập Git Repository mới (resolved)
- [x] Ticket 02: Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện (resolved)
- [x] Ticket 03: Xóa bỏ tàn dư Git conflict và loại bỏ mã nguồn chết (resolved)
- [x] Ticket 04: Bảo mật chức năng tải ảnh sản phẩm và vá lỗ hổng RCE (resolved)
- [x] Ticket 05: Đóng các endpoint CRUD công khai và chuẩn hóa phân quyền Admin Area (resolved)
- [x] Ticket 06: Bổ sung bảo vệ CSRF [ValidateAntiForgeryToken] và sửa chuyển hướng đăng nhập (resolved)
- [x] Ticket 07: Sửa lỗi tính tiền đơn hàng theo số lượng và lưu trữ đơn hàng bền vững trong Database (resolved)
- [x] Ticket 08: Xác thực chữ ký số bảo mật hai chiều cho cổng thanh toán MoMo và VNPay (resolved)
- [x] Ticket 09: Giải phóng kết nối cơ sở dữ liệu và tối ưu hóa truy vấn lặp N+1 (resolved)
- [x] Ticket 10: Tách tầng Service Layer và tích hợp Unity Dependency Injection (resolved)
- [x] Ticket 11: Nâng cấp giao diện người dùng lên Bootstrap 5 qua LibMan và tích hợp NLog (resolved)

## Vấn đề chưa rõ (Open Questions)
- Không có. Toàn bộ 11/11 tickets đã được hoàn thành và tích hợp trọn vẹn vào nhánh `integration/lala-coffee-transformation`.
