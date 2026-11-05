# Ticket 02: Thu hồi và bảo vệ thông tin mật nhạy cảm (Email App Password & Payment Keys)

Status: claimed
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

## Comments
