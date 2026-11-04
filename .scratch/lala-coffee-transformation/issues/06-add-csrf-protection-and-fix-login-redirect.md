# 06: Bổ sung bảo vệ CSRF [ValidateAntiForgeryToken] và sửa chuyển hướng đăng nhập

**What to build:** Toàn bộ các biểu mẫu gửi dữ liệu quan trọng của hệ thống (đăng ký, đăng nhập, đặt hàng, quản lý sản phẩm và danh mục) được bảo vệ bằng mã chống giả mạo yêu cầu (CSRF Token). Khi người dùng chưa xác thực cố truy cập các trang đòi hỏi quyền hạn, hệ thống chuyển hướng chính xác về trang đăng nhập của cửa hàng kèm thông báo cần thiết thay vì đẩy về trang chủ.

**Blocked by:** 02 (Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện)

**Status:** ready-for-agent

- [ ] Tất cả các hành động tiếp nhận dữ liệu HTTP POST trong toàn bộ hệ thống đều áp dụng bộ lọc xác thực token chống CSRF.
- [ ] Các biểu mẫu tương ứng trên giao diện Razor Views sinh mã token hợp lệ và gửi kèm dữ liệu biểu mẫu.
- [ ] Các yêu cầu POST giả mạo thiếu token hoặc sai lệch token bị từ chối với mã lỗi bảo mật phù hợp.
- [ ] Cấu hình xác thực người dùng chuyển hướng các yêu cầu chưa có quyền về đúng trang đăng nhập `/Account/Login`.
