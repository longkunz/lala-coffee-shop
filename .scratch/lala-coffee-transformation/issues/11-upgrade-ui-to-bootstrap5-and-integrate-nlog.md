# 11: Nâng cấp giao diện người dùng lên Bootstrap 5 qua LibMan và tích hợp NLog

**What to build:** Giao diện khách hàng được nâng cấp lên Bootstrap 5 hiện đại, tối ưu responsive trên mọi kích thước màn hình và quản lý thư viện client-side gọn gàng thông qua tập tin cấu hình LibMan. Toàn bộ các sự cố và ngoại lệ trong quá trình vận hành được hệ thống NLog ghi nhận an toàn vào các tệp tin nhật ký xoay vòng theo ngày; người dùng cuối nhận được thông báo lỗi thân thiện, bảo vệ toàn diện các thông tin chi tiết nhạy cảm của hệ thống.

**Blocked by:** 10 (Tách tầng Service Layer và tích hợp Unity Dependency Injection)

**Status:** ready-for-agent

- [ ] Tập tin cấu hình quản lý thư viện (`libman.json`) tải và quản lý các thư viện giao diện Bootstrap 5 mới nhất vào dự án.
- [ ] Giao diện người dùng trên các màn hình chính (Trang chủ, Menu, Chi tiết đồ uống, Giỏ hàng, Đặt hàng) tương thích hoàn toàn với Bootstrap 5 và hiển thị mượt mà trên thiết bị di động.
- [ ] Thư viện NLog được tích hợp vào ứng dụng và cấu hình ghi nhật ký lỗi tự động ra tệp tin xoay vòng (`Rolling File`).
- [ ] Các thông báo lỗi hiển thị cho khách hàng được chuẩn hóa thân thiện; các thông tin nhạy cảm về cơ sở dữ liệu và stack trace hệ thống được ẩn giấu tuyệt đối.
- [ ] Cấu hình chế độ debug của ứng dụng được tối ưu hóa cho môi trường vận hành thực tế.
