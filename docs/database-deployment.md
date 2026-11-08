# Import database LalaCoffee

## Triển khai mới: chỉ cần một file SQL

File **`Database_Deploy.sql`** ở thư mục gốc chứa schema, khóa chính/khóa ngoại,
index, lịch sử migration EF6 và toàn bộ dữ liệu mẫu. Không cần chạy
`Update-Database` hoặc chạy thêm `Database_Seed.sql` sau khi import.

1. Mở file bằng SQL Server Management Studio (SSMS), kết nối SQL Server.
2. Tên database mặc định là `UniCafe`. Nếu dùng tên khác, đổi tên trong khối
   `CREATE DATABASE` và dòng `USE [UniCafe]` ở đầu file.
3. Nếu hosting đã cấp database trống, bỏ đoạn từ `USE [master]` đến hết khối
   `IF DB_ID ... CREATE DATABASE` và giữ dòng `USE` trỏ đúng database được cấp.
4. Chạy **toàn bộ file**. Script chỉ chấp nhận database trống; schema và seed
   chạy trong cùng transaction, có lỗi sẽ rollback.
5. Sửa `DefaultConnection` trong `LalaCoffee/Web.config` theo server, database
   và thông tin đăng nhập được cấp. Khởi động web, không chạy migration.

Ví dụ import bằng `sqlcmd` với Windows Authentication:

```powershell
sqlcmd -S .\SQLEXPRESS -E -b -f 65001 -i .\Database_Deploy.sql
```

`GO` là dấu phân batch của SSMS/sqlcmd; nếu dùng công cụ khác, công cụ đó cần
hỗ trợ `GO`. File dùng UTF-8 và chuỗi Unicode `N'...'` để giữ tiếng Việt.

## Dữ liệu có sẵn

- 6 danh mục có mô tả chi tiết.
- 24 sản phẩm: cà phê, trà trái cây, trà sữa, đá xay, bánh mì và mochi.
- 40 lựa chọn size/cách phục vụ, 50 lựa chọn thêm topping theo từng sản phẩm.
- 8 bài viết có nội dung dài, chia mục và gợi ý thưởng thức/đặt món.
- 2 vai trò và 2 tài khoản mẫu:

| Tên đăng nhập | Mật khẩu ban đầu | Vai trò |
| --- | --- | --- |
| `admin` | `Admin@123` | Admin |
| `user` | `User@123` | User |

Đổi mật khẩu các tài khoản mẫu trước khi mở web công khai. Database triển khai
không chứa đơn hàng, tài khoản cá nhân hay các bản ghi thử trên máy phát triển.
Ảnh lấy từ `LalaCoffee/Content` đã có trong repository; cần triển khai cả thư mục
này cùng ứng dụng, vì file SQL chỉ lưu đường dẫn ảnh.

## Cập nhật seed trên database đã có schema

Chọn đúng database trong SSMS rồi chạy **`Database_Seed.sql`**, hoặc:

```powershell
sqlcmd -S .\SQLEXPRESS -E -d UniCafe -b -f 65001 -i .\Database_Seed.sql
```

Script cập nhật sản phẩm/danh mục theo slug, bài viết theo tiêu đề, tùy chọn theo
sản phẩm và tên. Chạy lại không nhân đôi bộ seed. ID hiện có, ngày tạo, trạng thái
hiển thị/xuất bản, tài khoản, mật khẩu và đơn hàng được giữ lại. Giá, nội dung,
ảnh và phân loại của các món thuộc bộ seed sẽ được cập nhật. Bài viết thuộc bộ
seed được cập nhật nội dung và ảnh. Bản ghi ngoài bộ seed được giữ nguyên.

Tài khoản mẫu chỉ được tạo nếu chưa có tên đăng nhập hoặc email tương ứng;
script không đặt lại mật khẩu và không tự cấp quyền cho tài khoản đã tồn tại.
Nếu có nhiều danh mục/sản phẩm cùng slug hoặc nhiều bài cùng tiêu đề trong bộ
seed, script dừng và rollback để bạn xử lý dữ liệu trùng trước.

`Database_Deploy.sql` là bản độc lập chứa cùng nội dung seed. Khi thay đổi bộ
seed về sau, cập nhật cả hai file để bản triển khai mới có nội dung tương ứng.
