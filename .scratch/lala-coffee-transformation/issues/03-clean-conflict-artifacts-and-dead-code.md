# 03: Xóa bỏ tàn dư Git conflict và loại bỏ mã nguồn chết

**What to build:** Thư mục dự án được làm sạch hoàn toàn khỏi các file tàn dư từ quá trình giải quyết xung đột Git trong quá khứ, các file tạm của trình soạn thảo và các controller/lớp bị vô hiệu hóa hoàn toàn, giúp cây mã nguồn tinh gọn và minh bạch.

**Blocked by:** 02 (Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện)

**Status:** ready-for-agent

- [ ] Toàn bộ các file xung đột Git cũ (`_BACKUP`, `_LOCAL`, `_REMOTE`, `_BASE`) trong khu vực quản trị bị xóa bỏ.
- [ ] Toàn bộ các file tạm (`.swp`) của trình soạn thảo bị dọn dẹp sạch sẽ.
- [ ] Các controller bị comment out hoàn toàn không sử dụng bị loại bỏ khỏi dự án.
- [ ] Các lớp mô hình trùng lặp không có giá trị phân biệt được tinh gọn và chuẩn hóa.
- [ ] Cây mã nguồn được tổ chức gọn gàng và giải pháp biên dịch sạch sẽ không cảnh báo rác.
