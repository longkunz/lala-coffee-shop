# 02: Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện

**What to build:** Toàn bộ dự án từ cấp độ solution, thư mục mã nguồn, tập tin project, tên assembly và không gian tên (namespace) trong tất cả các file mã nguồn C# và các giao diện Razor Views được chuyển đổi đồng bộ từ UniCafe sang LalaCoffee. Ứng dụng khởi động và định tuyến đúng dưới nhận diện thương hiệu mới.

**Blocked by:** 01 (Nâng cấp Target Framework lên .NET 4.8 và Thiết lập Git Repository mới)

**Status:** resolved

- [x] Tập tin Solution được chuẩn hóa thành `LalaCoffee.sln`.
- [x] Thư mục mã nguồn chính và file Project được đổi tên thành `LalaCoffee` / `LalaCoffee.csproj`.
- [x] Tên Assembly và Root Namespace của dự án được cập nhật thành `LalaCoffee`.
- [x] Toàn bộ không gian tên (namespace) trong tất cả các lớp C# (Controllers, Models, Data, Services, Helpers) được đổi sang `LalaCoffee.*`.
- [x] Toàn bộ các tham chiếu model và namespace trong các file giao diện Razor Views (`.cshtml`) và cấu hình view được cập nhật đồng bộ sang `LalaCoffee`.
- [x] Toàn bộ giải pháp biên dịch thành công không còn lỗi thiếu namespace hoặc đứt gãy tham chiếu.

## Kết quả (Answer)

Đã hoàn thành trong commit [`f82c12c`](file:///D:/UniCafe) và [`d2d459b`](file:///D:/UniCafe). Solution đã được đổi tên thành `LalaCoffee.sln`, project đổi thành `LalaCoffee.csproj`, toàn bộ namespace chuyển sang `LalaCoffee.*` và Views được cập nhật đồng bộ. Biên dịch thành công 0 lỗi.
