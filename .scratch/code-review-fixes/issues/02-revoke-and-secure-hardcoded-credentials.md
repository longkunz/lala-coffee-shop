# Ticket 02: Thu hồi và bảo vệ thông tin mật nhạy cảm (Email App Password & Payment Keys)

Status: resolved
Type: task

## Vấn đề
- Tại [`OrderController.cs`](file:///D:/UniCafe/UniCafe/Controllers/OrderController.cs#L41-L43) và [`PayController.cs`](file:///D:/UniCafe/UniCafe/Controllers/PayController.cs#L35-L37), mật khẩu ứng dụng Gmail `"phhrqqdawnyalkpr"` và email `"hphcplnh7@gmail.com"` bị hardcode trực tiếp vào mã nguồn C# và commit lên Git công khai.
- Tại [`Web.config`](file:///D:/UniCafe/UniCafe/Web.config#L20-L30), các khóa `secretKey_Momo`, `accessKey_Momo`, `vnp_HashSecret` đều nằm trong source control.

## Yêu cầu kỹ thuật
1. **Con người thực hiện (Human action)**:
   - Thu hồi (Revoke) ngay mật khẩu ứng dụng `"phhrqqdawnyalkpr"` trong tài khoản Google.
   - Sinh lại các khóa bí mật sandbox mới nếu cần.
2. **Kỹ thuật**:
   - Tách cấu hình nhạy cảm sang file riêng `secrets.config` và thêm vào `.gitignore` (sử dụng `<appSettings file="secrets.config">` trong `Web.config`), hoặc đọc từ biến môi trường máy chủ `Environment.GetEnvironmentVariable`.
   - Viết một service gửi mail tập trung (`EmailService.cs`) thay vì copy-paste code SMTP ở cả `OrderController` và `PayController`.

## Kết quả (Answer)
- Đã cô lập toàn bộ credentials nhạy cảm khỏi source control:
  - Cập nhật [`Web.config`](file:///D:/UniCafe/LalaCoffee/Web.config) sử dụng `<appSettings file="secrets.config">`. Thay thế toàn bộ secret keys bị rò rỉ (`secretKey_Momo`, `accessKey_Momo`, `vnp_HashSecret`) bằng các giá trị dummy an toàn cho môi trường test/development.
  - Tạo file mẫu [`secrets.config.example`](file:///D:/UniCafe/LalaCoffee/secrets.config.example) hướng dẫn cấu hình chi tiết cho môi trường production/staging.
  - File `secrets.config` đã được đưa vào `.gitignore` để tránh vô tình commit lên repository.
- Xây dựng dịch vụ gửi email tập trung chuẩn mực [`IEmailService`](file:///D:/UniCafe/LalaCoffee/Services/Email/IEmailService.cs) và [`EmailService`](file:///D:/UniCafe/LalaCoffee/Services/Email/EmailService.cs):
  - Hỗ trợ linh hoạt cấu hình từ `secrets.config`, `Web.config`, hoặc fallback sang biến môi trường (`Environment.GetEnvironmentVariable`).
  - Hỗ trợ cả gửi đồng bộ `SendEmail`, `SendOrderConfirmation` và bất đồng bộ `SendEmailAsync`.
  - Hỗ trợ DI / Mocking / Delegate injection để phục vụ kiểm thử đơn vị độc lập mà không gửi mail thật ra ngoài.
- Tái cấu trúc Controller:
  - Loại bỏ hoàn toàn hardcoded Gmail App Password (`phhrqqdawnyalkpr`) và email cá nhân (`hphcplnh7@gmail.com`) khỏi [`OrderController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/OrderController.cs) và [`PayController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/PayController.cs).
  - Cả hai Controller được tái cấu trúc để tiêm và sử dụng `IEmailService`, loại bỏ hoàn toàn mã nguồn gửi mail trùng lặp bằng `SmtpClient`.
- Kiểm thử tự động:
  - Xây dựng 10 unit test cases trong [`EmailServiceTests.cs`](file:///D:/UniCafe/LalaCoffee.Tests/EmailServiceTests.cs) kiểm tra toàn diện cấu hình mặc định, custom resolver, biến môi trường fallback, template email đơn hàng, validation tham số, cũng như quét mã nguồn để đảm bảo tuyệt đối không còn hardcoded password hay leaked keys trong dự án.
  - 100% các bài test đều vượt qua.

## Comments
