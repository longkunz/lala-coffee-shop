# 01: Nâng cấp Target Framework lên .NET 4.8 và Thiết lập Git Repository mới

**What to build:** Toàn bộ ứng dụng được chuyển đổi nền tảng mục tiêu sang .NET Framework 4.8 mới nhất của dòng 4.x, cập nhật đầy đủ các cấu hình biên dịch và gói phụ thuộc. Lịch sử Git cũ được dọn dẹp sạch sẽ, khởi tạo nhánh chính `main` và kết nối với remote repository chính thức `https://github.com/longkunz/lala-coffee-shop.git` với commit ban đầu sạch.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] Cấu hình dự án được nâng cấp lên .NET Framework 4.8 (`v4.8`) và ứng dụng biên dịch thành công.
- [ ] Cấu hình runtime trong web configuration trỏ chính xác về .NET 4.8.
- [ ] Các gói thư viện phụ thuộc được đồng bộ target framework `net48`.
- [ ] Toàn bộ lịch sử commit và file Git cũ được xóa sạch.
- [ ] Git mới được khởi tạo với nhánh mặc định là `main`.
- [ ] Remote origin được kết nối chuẩn xác tới `https://github.com/longkunz/lala-coffee-shop.git`.
- [ ] File bỏ qua (.gitignore) chặn hoàn toàn các file tạm và thư mục build không cần thiết.
- [ ] Commit khởi tạo ban đầu được tạo thành công trên nhánh `main`.
