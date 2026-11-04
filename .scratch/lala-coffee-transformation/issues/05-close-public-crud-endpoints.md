# 05: Đóng các endpoint CRUD công khai và chuẩn hóa phân quyền Admin Area

**What to build:** Toàn bộ các thao tác quản trị nhạy cảm liên quan đến việc tạo mới, sửa đổi hoặc xóa bỏ danh mục, thuộc tính size và tùy chọn topping của đồ uống được thu hồi khỏi các endpoint công khai ngoài client. Tất cả các chức năng quản trị chỉ được phép truy cập và thực thi bên trong phân vùng quản trị (Admin Area) với đầy đủ xác thực quyền Admin.

**Blocked by:** 02 (Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện)

**Status:** ready-for-agent

- [ ] Các hành động tạo mới, chỉnh sửa, xóa bỏ dữ liệu trong các controller phía ngoài khách hàng bị xóa bỏ hoàn toàn.
- [ ] Các giao diện chỉnh sửa/xóa thừa thãi ngoài client được dọn dẹp để không gây hiểu nhầm về bảo mật.
- [ ] Mọi chức năng quản lý danh mục, size và tùy chọn món chỉ có thể truy cập qua phân vùng Admin với quyền quản trị viên (`Admin`).
- [ ] Khách hàng vãng lai cố tình gọi trực tiếp các URL chỉnh sửa/xóa trước đây đều nhận phản hồi từ chối hoặc chuyển hướng an toàn.
