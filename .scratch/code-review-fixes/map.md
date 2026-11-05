# Bản đồ triển khai: code-review-fixes (Map)

## Ghi chú (Notes)
- Nhánh tích hợp: `integration/code-review-fixes`
- Bản ghi chú thám hiểm mã nguồn: `C:\Users\longhv\.gemini\antigravity-cli\brain\c97fa97b-7d5f-440f-8edd-da1d8916a8d7\scratch\exploration-notes.md`
- Solution: `LalaCoffee.sln` (.NET Framework 4.8)

## Quyết định đã chốt (Decisions)
- [x] **Ticket 00**: Nâng cấp .NET 4.8, đổi tên toàn diện sang `LalaCoffee`, reset Git sang `main` tại remote `https://github.com/longkunz/lala-coffee-shop.git`. Chi tiết: [`00-upgrade-net48-rename-lalacoffee-reset-git.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/00-upgrade-net48-rename-lalacoffee-reset-git.md).
- [x] **Ticket 01**: Vá lỗ hổng Arbitrary File Upload / RCE trong ImageController (Bổ sung `[Authorize(Roles = "Admin")]`, Whitelist định dạng/kích thước ảnh, sinh tên file GUID ngẫu nhiên). Chi tiết: [`01-fix-image-upload-rce.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/01-fix-image-upload-rce.md).
- [x] **Ticket 08**: Dọn dẹp tàn dư Git conflict (*_273.cs), controller chết (CollectionController), model dư thừa (User, Role), chuẩn hóa namespace AccountController. Chi tiết: [`08-clean-git-conflict-artifacts-and-dead-code.md`](file:///D:/UniCafe/.scratch/code-review-fixes/issues/08-clean-git-conflict-artifacts-and-dead-code.md).

## Danh sách công việc (Task Graph)
- [x] Ticket 00: Nâng lên .NET 4.8, Đổi tên toàn diện sang LalaCoffee và Reset Git sang repo mới (Đã hoàn thành)
- [x] Ticket 01: Vá lỗ hổng Arbitrary File Upload / RCE trong ImageController (Đã hoàn thành)
- [ ] Ticket 02: Thu hồi và bảo vệ thông tin mật nhạy cảm (Email App Password & Payment Keys)
- [ ] Ticket 03: Xác thực chữ ký số Checksum cho cổng thanh toán MoMo và VNPay (Blocked by: 02)
- [ ] Ticket 04: Bảo vệ hoặc loại bỏ các endpoint CRUD công khai ngoài Admin Area
- [ ] Ticket 05: Sửa lỗi tính tổng tiền đơn hàng bị bỏ quên số lượng (Quantity) và lưu đơn hàng vào DB (Blocked by: 03)
- [ ] Ticket 06: Bổ sung CSRF Protection [ValidateAntiForgeryToken] và sửa LoginPath
- [ ] Ticket 07: Khắc phục rò rỉ DbContext và tối ưu hóa truy vấn N+1 (Blocked by: 05)
- [x] Ticket 08: Dọn dẹp tàn dư Git conflict, file swap và mã nguồn chết (Dead Code) (Đã hoàn thành)

## Vấn đề chưa rõ (Open Questions)
- Không có. Tất cả các ticket đã có đặc tả và phương án kỹ thuật rõ ràng.
