# Ticket 01: Vá lỗ hổng Arbitrary File Upload / RCE trong ImageController

Status: ready-for-agent
Type: task

## Vấn đề
Tại [`ImageController.cs`](file:///D:/UniCafe/UniCafe/Controllers/ImageController.cs#L13-L30), action `ProcessUpload` nhận `HttpPostedFileBase file` nhưng hoàn toàn không kiểm tra phần mở rộng của file, không kiểm tra quyền admin và không lọc đường dẫn:
- Kẻ tấn công có thể upload file `.aspx` (Web Shell) và thực thi mã độc từ xa với quyền hạn của IIS AppPool.
- Không có `[Authorize]`, người dùng ẩn danh cũng có thể upload file làm đầy ổ cứng.
- Không dùng `Path.GetFileName()`, có nguy cơ Path Traversal ghi đè file cấu hình hệ thống.

## Yêu cầu kỹ thuật
1. Bổ sung `[Authorize(Roles = "Admin")]` cho `ImageController` hoặc action `ProcessUpload`.
2. Tạo danh sách whitelist định dạng ảnh hợp lệ: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`.
3. Kiểm tra kích thước file tối đa (ví dụ <= 5MB) và kiểm tra ContentType.
4. Lọc tên file an toàn bằng `Path.GetFileName()`, sinh tên file ngẫu nhiên bằng `Guid.NewGuid()`.
5. Đảm bảo file được lưu an toàn vào `~/Content/images/`.

## Comments
