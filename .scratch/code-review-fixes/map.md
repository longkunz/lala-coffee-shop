# Bản đồ triển khai: code-review-fixes (Map)

## Ghi chú (Notes)
- Nhánh tích hợp: `integration/code-review-fixes`
- Bản ghi chú thám hiểm mã nguồn: `C:\Users\longhv\.gemini\antigravity-cli\brain\c97fa97b-7d5f-440f-8edd-da1d8916a8d7\scratch\exploration-notes.md`
- Solution: `LalaCoffee.sln` (.NET Framework 4.8)

## Quyết định đã chốt (Decisions)
- [x] **Ticket 00**: Nâng cấp .NET 4.8, đổi tên toàn diện sang `LalaCoffee`, reset Git sang `main` tại remote `https://github.com/longkunz/lala-coffee-shop.git`. Chi tiết: [`00-upgrade-net48-rename-lalacoffee-reset-git.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/00-upgrade-net48-rename-lalacoffee-reset-git.md).
- [x] **Ticket 01**: Vá lỗ hổng Arbitrary File Upload / RCE trong ImageController (Bổ sung `[Authorize(Roles = "Admin")]`, Whitelist định dạng/kích thước ảnh, sinh tên file GUID ngẫu nhiên). Chi tiết: [`01-fix-image-upload-rce.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/01-fix-image-upload-rce.md).
- [x] **Ticket 02**: Tách mật khẩu ứng dụng Gmail và Payment Keys sang `secrets.config` & biến môi trường, xây dựng `EmailService` tập trung, xóa bỏ hoàn toàn hardcoded credentials. Chi tiết: [`02-revoke-and-secure-hardcoded-credentials.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/02-revoke-and-secure-hardcoded-credentials.md).
- [x] **Ticket 03**: Xác thực chữ ký Checksum cho MoMo (HMAC-SHA256) và VNPay (HMAC-SHA512) trong PayController, ngăn chặn giả mạo callback thanh toán. Chi tiết: [`03-verify-payment-signatures.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/03-verify-payment-signatures.md).
- [x] **Ticket 04**: Loại bỏ các action và view CRUD (Create, Edit, Delete) công khai ngoài Admin Area tại `CategoryController`, `OptionProductController`, `PropertyProductController`. Toàn quyền quản trị được bảo vệ tại Area Admin. Chi tiết: [`04-protect-public-crud-controllers.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/04-protect-public-crud-controllers.md).
- [x] **Ticket 05**: Khắc phục lỗi bỏ quên `Quantity` khi tính tiền đơn hàng, triển khai `OrderCalculator` độc lập, lưu `Order` và `OrderDetail` vào DB lúc Checkout (Paid = 0, Status = "Pending"), cập nhật trạng thái khi callback thanh toán thành công theo `orderCode` độc lập với session. Chi tiết: [`05-fix-order-quantity-calculation.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/05-fix-order-quantity-calculation.md).
- [x] **Ticket 06**: Bổ sung `[ValidateAntiForgeryToken]` cho toàn bộ action POST xử lý form (Account, Order CheckOut, Admin CRUD controllers), cập nhật Cookie Authentication `LoginPath` thành `/Account/Login`, đồng bộ token trong Ajax & Razor views. Chi tiết: [`06-add-anti-forgery-tokens-and-fix-login-path.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/06-add-anti-forgery-tokens-and-fix-login-path.md).
- [x] **Ticket 08**: Dọn dẹp tàn dư Git conflict (*_273.cs), controller chết (CollectionController), model dư thừa (User, Role), chuẩn hóa namespace AccountController. Chi tiết: [`08-clean-git-conflict-artifacts-and-dead-code.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/08-clean-git-conflict-artifacts-and-dead-code.md).

## Danh sách công việc (Task Graph)
- [x] Ticket 00: Nâng lên .NET 4.8, Đổi tên toàn diện sang LalaCoffee và Reset Git sang repo mới (Đã hoàn thành)
- [x] Ticket 01: Vá lỗ hổng Arbitrary File Upload / RCE trong ImageController (Đã hoàn thành)
- [x] Ticket 02: Thu hồi và bảo vệ thông tin mật nhạy cảm (Email App Password & Payment Keys) (Đã hoàn thành)
- [x] Ticket 03: Xác thực chữ ký số Checksum cho cổng thanh toán MoMo và VNPay (Đã hoàn thành)
- [x] Ticket 04: Bảo vệ hoặc loại bỏ các endpoint CRUD công khai ngoài Admin Area (Đã hoàn thành)
- [x] Ticket 05: Sửa lỗi tính tổng tiền đơn hàng bị bỏ quên số lượng (Quantity) và lưu đơn hàng vào DB (Đã hoàn thành)
- [x] Ticket 06: Bổ sung CSRF Protection [ValidateAntiForgeryToken] và sửa LoginPath (Đã hoàn thành)
- [ ] Ticket 07: Khắc phục rò rỉ DbContext và tối ưu hóa truy vấn N+1 (Ready for agent)
- [x] Ticket 08: Dọn dẹp tàn dư Git conflict, file swap và mã nguồn chết (Dead Code) (Đã hoàn thành)

## Vấn đề chưa rõ (Open Questions)
- Không có. Tất cả các ticket đã có đặc tả và phương án kỹ thuật rõ ràng.
