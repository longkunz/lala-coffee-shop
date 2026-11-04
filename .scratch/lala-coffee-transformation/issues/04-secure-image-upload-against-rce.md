# 04: Bảo mật chức năng tải ảnh sản phẩm và vá lỗ hổng RCE

**What to build:** Quản trị viên có thể tải ảnh đại diện cho các món đồ uống lên hệ thống một cách an toàn và tin cậy. Hệ thống áp dụng kiểm soát quyền nghiêm ngặt, chỉ cho phép các định dạng ảnh hợp lệ, giới hạn dung lượng và đổi tên tệp ngẫu nhiên nhằm loại bỏ hoàn toàn nguy cơ kẻ gian tải mã độc (Web Shell / RCE) hoặc tấn công ghi đè tệp tin qua đường dẫn (Path Traversal).

**Blocked by:** 02 (Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện)

**Status:** ready-for-agent

- [ ] Chức năng tải tệp tin yêu cầu quyền quản trị viên (`Admin`) mới được phép truy cập; người dùng chưa xác thực bị từ chối ngay lập tức.
- [ ] Chỉ các định dạng ảnh nằm trong danh sách an toàn (`.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`) mới được phép tải lên; các file thực thi hoặc script (`.aspx`, `.ashx`, `.exe`,...) bị chặn tuyệt đối.
- [ ] Tên file tải lên được trích xuất an toàn và sinh ngẫu nhiên bằng mã định danh duy nhất (GUID) để ngăn chặn tấn công duyệt thư mục hoặc ghi đè file hệ thống.
- [ ] Dung lượng file tải lên được giới hạn tối đa hợp lý (ví dụ: không vượt quá 5MB).
- [ ] Ảnh được lưu trữ vào thư mục tài sản tĩnh an toàn và trả về đường dẫn chính xác để hiển thị trên giao diện quản trị.
