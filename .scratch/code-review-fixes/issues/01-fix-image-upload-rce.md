# Ticket 01: Vá lỗ hổng Arbitrary File Upload / RCE trong ImageController

Status: resolved
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

## Kết quả (Answer)
- Đã bổ sung thuộc tính `[Authorize(Roles = "Admin")]` cho `ImageController`, ngăn chặn triệt để người dùng chưa xác thực hoặc không có quyền Admin upload file.
- Đã tạo lớp tiện ích `LalaCoffee.Helpers.ImageUploadValidator` hỗ trợ:
  - Whitelist các đuôi mở rộng hợp lệ: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`.
  - Whitelist các Content-Type hợp lệ: `image/jpeg`, `image/pjpeg`, `image/png`, `image/x-png`, `image/gif`, `image/webp`.
  - Giới hạn dung lượng upload tối đa: 5MB (`MaxFileSizeBytes = 5 * 1024 * 1024`).
  - Hàm `GenerateSafeFileName()` sinh tên file ngẫu nhiên bằng `Guid.NewGuid():N` kèm phần mở rộng viết thường, loại bỏ nguy cơ trùng lặp và Path Traversal.
- Cập nhật action `ProcessUpload` trong `ImageController`:
  - Kiểm tra `file == null || file.ContentLength <= 0`.
  - Dùng `Path.GetFileName(file.FileName)` để loại bỏ path traversal trước khi kiểm tra extension.
  - Kiểm tra dung lượng hợp lệ và tự động khởi tạo thư mục lưu trữ nếu chưa tồn tại.
  - Lưu file an toàn bằng tên GUID ngẫu nhiên và trả về mã lỗi HTTP 400 kèm thông báo rõ ràng nếu vi phạm.
- Đã xây dựng bộ unit test gồm 47 test cases trong `LalaCoffee.Tests.ImageUploadValidatorTests`, xác minh 100% các kịch bản upload hợp lệ và bất hợp pháp (file exe, aspx, bat, js, cshtml, rỗng, vượt kích thước, v.v.). Toàn bộ test đều PASS.
