# 08: Xác thực chữ ký số bảo mật hai chiều cho cổng thanh toán MoMo và VNPay

**What to build:** Toàn bộ dữ liệu phản hồi từ các cổng thanh toán trực tuyến (MoMo và VNPay) khi khách hàng hoàn tất giao dịch và chuyển hướng quay lại website, hoặc qua kênh thông báo ngầm từ máy chủ tới máy chủ (IPN Webhook), bắt buộc phải được kiểm tra và xác thực chữ ký số an toàn (Checksum HMAC SHA256 cho MoMo và HMAC SHA512 cho VNPay). Chỉ các giao dịch có chữ ký số hợp lệ và đúng số tiền mới được phép cập nhật trạng thái đơn hàng sang Đã thanh toán (`Paid`), ngăn chặn hoàn toàn các hành vi gian lận URL để chiếm đoạt sản phẩm.

**Blocked by:** 07 (Sửa lỗi tính tiền đơn hàng theo số lượng và lưu trữ đơn hàng bền vững trong Database)

**Status:** ready-for-agent

- [ ] Hệ thống băm lại dữ liệu phản hồi từ VNPay và đối chiếu với mã kiểm tra chữ ký an toàn (`vnp_SecureHash`); từ chối mọi yêu cầu có chữ ký không trùng khớp.
- [ ] Hệ thống băm lại dữ liệu phản hồi từ MoMo và đối chiếu với mã chữ ký (`signature`); từ chối mọi yêu cầu có chữ ký không trùng khớp.
- [ ] Hành vi giả mạo truy cập trực tiếp vào URL hoàn tất thanh toán bị phát hiện và chuyển hướng đến trang thông báo lỗi thanh toán.
- [ ] Khi chữ ký số hợp lệ và cổng thanh toán xác nhận thành công, đơn hàng tương ứng trong cơ sở dữ liệu được tìm thấy qua mã đơn và cập nhật trạng thái `Paid = 1`, `Status = "Paid"`.
- [ ] Thông tin cấu hình khóa bí mật của các cổng thanh toán được chuyển sang quản lý an toàn và không còn hardcode trong mã nguồn.
