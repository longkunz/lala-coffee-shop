# 10: Tách tầng Service Layer và tích hợp Unity Dependency Injection

**What to build:** Toàn bộ logic nghiệp vụ cốt lõi về xử lý đơn hàng, giỏ hàng, kết nối cổng thanh toán qua HTTP client chuẩn và gửi thư thông báo được chuyển từ Controller sang các dịch vụ chuyên biệt (`IOrderService`, `IPaymentService`, `ICartService`, `IEmailService`). Hệ thống tiêm phụ thuộc Unity DI được kích hoạt chính thức để tự động cấp phát tài nguyên cho Controller, biến Controller thành tầng điều phối mỏng nhẹ và dễ dàng kiểm thử tự động. Gửi email thông báo được chuyển sang chế độ bất đồng bộ và cô lập lỗi hoàn toàn.

**Blocked by:** 04 (Bảo mật chức năng tải ảnh sản phẩm và vá lỗ hổng RCE), 05 (Đóng các endpoint CRUD công khai và chuẩn hóa phân quyền Admin Area), 08 (Xác thực chữ ký số bảo mật hai chiều cho cổng thanh toán MoMo và VNPay), 09 (Giải phóng kết nối cơ sở dữ liệu và tối ưu hóa truy vấn lặp N+1)

**Status:** claimed

- [ ] Các giao diện và lớp triển khai dịch vụ nghiệp vụ được xây dựng tách biệt khỏi Controller.
- [ ] Trình giải quyết phụ thuộc Unity được cấu hình làm Dependency Resolver chính thức cho ứng dụng ASP.NET MVC 5.
- [ ] Dịch vụ thanh toán sử dụng đối tượng HTTP client hiện đại thay thế cho các thư viện gửi HTTP request cổ điển.
- [ ] Dịch vụ gửi email thông báo hoạt động theo cơ chế bất đồng bộ (`async/await`); sự cố gián đoạn từ máy chủ gửi mail được cô lập và ghi nhận lại mà không làm gián đoạn hay rollback đơn hàng của khách.
- [ ] Các controller tiếp nhận phụ thuộc thông qua hàm khởi tạo (Constructor Injection) và không còn tự ý khởi tạo đối tượng trực tiếp.
