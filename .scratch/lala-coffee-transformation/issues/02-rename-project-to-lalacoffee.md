# 02: Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện

**What to build:** Toàn bộ dự án từ cấp độ solution, thư mục mã nguồn, tập tin project, tên assembly và không gian tên (namespace) trong tất cả các file mã nguồn C# và các giao diện Razor Views được chuyển đổi đồng bộ từ UniCafe sang LalaCoffee. Ứng dụng khởi động và định tuyến đúng dưới nhận diện thương hiệu mới.

**Blocked by:** 01 (Nâng cấp Target Framework lên .NET 4.8 và Thiết lập Git Repository mới)

**Status:** ready-for-agent

- [ ] Tập tin Solution được chuẩn hóa thành `LalaCoffee.sln`.
- [ ] Thư mục mã nguồn chính và file Project được đổi tên thành `LalaCoffee` / `LalaCoffee.csproj`.
- [ ] Tên Assembly và Root Namespace của dự án được cập nhật thành `LalaCoffee`.
- [ ] Toàn bộ không gian tên (namespace) trong tất cả các lớp C# (Controllers, Models, Data, Services, Helpers) được đổi sang `LalaCoffee.*`.
- [ ] Toàn bộ các tham chiếu model và namespace trong các file giao diện Razor Views (`.cshtml`) và cấu hình view được cập nhật đồng bộ sang `LalaCoffee`.
- [ ] Toàn bộ giải pháp biên dịch thành công không còn lỗi thiếu namespace hoặc đứt gãy tham chiếu.
