# 11: Nâng cấp giao diện người dùng lên Bootstrap 5 qua LibMan và tích hợp NLog

**What to build:** Giao diện khách hàng được nâng cấp lên Bootstrap 5 hiện đại, tối ưu responsive trên mọi kích thước màn hình và quản lý thư viện client-side gọn gàng thông qua tập tin cấu hình LibMan. Toàn bộ các sự cố và ngoại lệ trong quá trình vận hành được hệ thống NLog ghi nhận an toàn vào các tệp tin nhật ký xoay vòng theo ngày; người dùng cuối nhận được thông báo lỗi thân thiện, bảo vệ toàn diện các thông tin chi tiết nhạy cảm của hệ thống.

**Blocked by:** 10 (Tách tầng Service Layer và tích hợp Unity Dependency Injection)

**Status:** resolved

- [x] Tập tin cấu hình quản lý thư viện (`libman.json`) tải và quản lý các thư viện giao diện Bootstrap 5 mới nhất vào dự án.
- [x] Giao diện người dùng trên các màn hình chính (Trang chủ, Menu, Chi tiết đồ uống, Giỏ hàng, Đặt hàng) tương thích hoàn toàn với Bootstrap 5 và hiển thị mượt mà trên thiết bị di động.
- [x] Thư viện NLog được tích hợp vào ứng dụng và cấu hình ghi nhật ký lỗi tự động ra tệp tin xoay vòng (`Rolling File`).
- [x] Các thông báo lỗi hiển thị cho khách hàng được chuẩn hóa thân thiện; các thông tin nhạy cảm về cơ sở dữ liệu và stack trace hệ thống được ẩn giấu tuyệt đối.
- [x] Cấu hình chế độ debug của ứng dụng được tối ưu hóa cho môi trường vận hành thực tế.

## Kết quả (Answer)

Đã hoàn thành trong commit [`4357a60`](file:///D:/UniCafe) và tích hợp vào nhánh chính trong [`a7a5e92`](file:///D:/UniCafe).
1. Client-side LibMan & Bootstrap 5:
   - Cấu hình `libman.json` quản lý `bootstrap@5.3.3` và đặt các tệp `bootstrap.min.css`, `bootstrap.bundle.min.js` vào `Content/lib/bootstrap/dist/`.
   - Cập nhật `BundleConfig.cs`, `_Layout_User.cshtml` và `_Layout.cshtml` để sử dụng Bootstrap 5.
   - Chuyển đổi toàn bộ cú pháp data attributes từ Bootstrap 3/4 sang Bootstrap 5 (`data-bs-toggle`, `data-bs-target`, `data-bs-dismiss`).
2. NLog Logging & An toàn lỗi:
   - Tích hợp thư viện `NLog` 5.3.4, cấu hình `NLog.config` ghi log dạng rolling file ra `logs/app-${shortdate}.log` và `logs/error-${shortdate}.log`.
   - Cài đặt `AiExceptionFilter` trong `FilterConfig.cs` và xử lý trong `Global.asax.cs Application_Error` để bắt toàn bộ exception và ghi log kèm stack trace chi tiết.
   - Trang lỗi thân thiện `Views/Shared/Error.cshtml` ẩn giấu 100% chi tiết lỗi kỹ thuật và stack trace trước người dùng cuối.
   - Cấu hình `Web.config` chuẩn production: `<customErrors mode="RemoteOnly" defaultRedirect="~/Shared/Error" />` và `<compilation debug="false" />`.
3. Kiểm thử tự động:
   - Thêm 16 bài test mới trong `BootstrapUiConfigTests.cs` và `NLogLoggingTests.cs`. Toàn bộ 166/166 bài test đều vượt qua thành công.
