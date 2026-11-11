# HỆ THỐNG WEBSITE BÁN CÀ PHÊ GIẢI KHÁT - LALACOFFEE

> **Tài liệu hướng dẫn cài đặt, cấu hình và triển khai**  
> **Nền tảng công nghệ:** C# (.NET Framework 4.8), ASP.NET MVC 5, Entity Framework 6, Microsoft SQL Server.

---

## 1. Yêu cầu môi trường &amp; Công cụ

Để cài đặt và vận hành hệ thống trên môi trường phát triển (Local Development), máy trạm cần đáp ứng các yêu cầu sau:


| Thành phần                      | Yêu cầu kỹ thuật                               | Ghi chú                                          |
| :------------------------------- | :---------------------------------------------- | :------------------------------------------------ |
| **Hệ điều hành**                | Windows 10 / Windows 11 (64-bit)               | Tương thích với .NET Framework 4.8               |
| **Môi trường phát triển (IDE)** | Visual Studio 2019 / 2022                      | Cần cài đặt workload ASP.NET and web development |
| **Nền tảng SDK/Runtime**        | .NET Framework 4.8 Developer Pack              | Cần thiết để biên dịch dự án                     |
| **Hệ quản trị CSDL**            | Microsoft SQL Server 2016+ / Express / LocalDB | Hỗ trợ lưu trữ và truy vấn dữ liệu               |
| **Công cụ quản trị CSDL**       | SQL Server Management Studio (SSMS) / `sqlcmd` | Dùng để thực thi script khởi tạo CSDL            |
| **Web Server cục bộ**           | IIS Express (tích hợp sẵn trong Visual Studio) | Chạy thử nghiệm trên localhost                   |


---

## 2. Kiến trúc &amp; Công nghệ sử dụng

- **Mô hình kiến trúc:** MVC (Model - View - Controller), áp dụng Repository và Unit of Work pattern.
- **Backend:** ASP.NET MVC 5, C# (.NET Framework 4.8).
- **ORM / Truy xuất dữ liệu:** Entity Framework 6.
- **Xác thực &amp; Phân quyền:** ASP.NET Identity 2.0 (OWIN Cookie Authentication).
- **Dependency Injection:** Unity Container 5.
- **Frontend &amp; Giao diện:** Razor View Engine, Bootstrap 3.4.1, jQuery 3.6, AdminLTE 3 (khu vực Admin).
- **Phương thức thanh toán:** COD (Cash on Delivery - Thanh toán khi nhận hàng).

---

## 3. Hướng dẫn cài đặt &amp; Triển khai từng bước

### Bước 1: Khởi tạo Cơ sở dữ liệu (Database)

Dự án sử dụng tệp kịch bản `Database_Deploy.sql` đặt tại thư mục gốc để khởi tạo toàn bộ cấu trúc bảng, ràng buộc khóa, chỉ mục và nạp dữ liệu mẫu (danh mục, sản phẩm, bài viết, tài khoản mặc định).

Thực hiện một trong hai cách sau: 

Sử dụng SQL Server Management Studio (SSMS)

1. Mở **SSMS** và kết nối tới SQL Server của bạn (ví dụ: `.\SQLEXPRESS` hoặc `(localdb)\MSSQLLocalDB`).
2. Mở tệp `Database_Deploy.sql` trong SSMS.
3. Nhấn **Execute** (hoặc phím **F5**) để chạy toàn bộ script.
4. Script sẽ tự động tạo database `LalaCoffee` và nạp toàn bộ dữ liệu khởi tạo.

---

### Bước 2: Mở giải pháp và phục hồi thư viện (NuGet Restore)

1. Mở tệp giải pháp `LalaCoffee.sln` bằng **Visual Studio**.
2. Phục hồi các gói thư viện phụ thuộc:
   - Nhấp chuột phải vào **Solution 'LalaCoffee'** trong cửa sổ *Solution Explorer*.
   - Chọn **Restore NuGet Packages**.
   - Hoặc hệ thống sẽ tự động phục hồi khi biên dịch dự án lần đầu.

---

### Bước 3: Cấu hình chuỗi kết nối CSDL (`Web.config`)

Mở tệp `LalaCoffee/Web.config`, tìm đến thẻ `<connectionStrings>` và cập nhật thông số kết nối khớp với máy chủ SQL Server của bạn:

```xml
<connectionStrings>
  <!-- Sử dụng SQL Server Express -->
  <add name="DefaultConnection" 
       connectionString="Server=.\SQLEXPRESS;Initial Catalog=LalaCoffee;Integrated Security=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> **Lưu ý cấu hình `Initial Catalog`:**
>
> - Giá trị `Initial Catalog` phải khớp chính xác với tên Database đã tạo ở Bước 1 (`LalaCoffee`).
> - Nếu sử dụng **LocalDB**, cấu hình: `Server=(localdb)\MSSQLLocalDB;Initial Catalog=LalaCoffee;Integrated Security=True;`.
> - Nếu sử dụng tài khoản `sa` có mật khẩu: `Server=localhost,1433;Initial Catalog=LalaCoffee;User Id=sa;Password=YourPassword;`.

---

### Bước 4: (Tùy chọn) Cấu hình dịch vụ Email SMTP

Dự án tích hợp dịch vụ gửi email xác nhận đơn hàng qua giao thức SMTP. Trong `LalaCoffee/Web.config`:

```xml
<appSettings>
  <!-- Cấu hình dịch vụ SMTP -->
  <add key="Email_Sender" value="verified-sender@example.com" />
  <add key="Email_Username" value="your-smtp-username" />
  <add key="Email_Password" value="your-smtp-password" />
  <add key="Email_Host" value="smtp-relay.brevo.com" />
  <add key="Email_Port" value="587" />
  <add key="Email_EnableSsl" value="true" />
</appSettings>
```

*Ghi chú: Nếu giữ nguyên giá trị mặc định, hệ thống vẫn vận hành bình thường; chức năng gửi email sẽ ghi nhận thất bại an toàn mà không làm gián đoạn luồng đặt hàng.*

---

### Bước 5: Biên dịch và Khởi chạy ứng dụng

1. Trên thanh công cụ của Visual Studio, chọn cấu hình biên dịch **Debug** hoặc **Release**.
2. Nhấn **F5** (chạy kèm trình gỡ lỗi) hoặc tổ hợp phím **Ctrl + F5** (chạy không gỡ lỗi).
3. Ứng dụng sẽ tự động khởi chạy trên trình duyệt web mặc định thông qua máy chủ IIS Express tại đường dẫn:
   ```
    http://localhost:[Port]/
   ```

---

## 4. Tài khoản thử nghiệm mặc định

Cơ sở dữ liệu khởi tạo sẵn 02 tài khoản mẫu phục vụ việc kiểm thử các luồng chức năng:


| Vai trò (Role)            | Tên đăng nhập (Username) | Email                 | Mật khẩu mặc định | Phạm vi quyền hạn                                                                            |
| :------------------------- | :------------------------ | :--------------------- | :----------------- | :-------------------------------------------------------------------------------------------- |
| **Quản trị viên (Admin)** | `admin`                  | `admin@lalacoffee.vn` | `Admin@123`       | Quản lý danh mục, sản phẩm, đơn hàng, bài viết, tài khoản và phân quyền tại khu vực `/Admin` |
| **Khách hàng (User)**     | `user`                   | `user@lalacoffee.vn`  | `User@123`        | Duyệt thực đơn, thêm giỏ hàng, đặt hàng (COD), quản lý đơn cá nhân và hồ sơ                  |


---

## 5. Cấu trúc thư mục dự án

```
lala-coffee-shop/
├── Database_Deploy.sql        # Kịch bản triển khai schema và dữ liệu mẫu CSDL
├── LalaCoffee.sln             # Tệp giải pháp Visual Studio
└── LalaCoffee/                # Mã nguồn chính của ứng dụng web
    ├── App_Start/             # Cấu hình khởi tạo: Routing, Bundle, Filter, Unity DI
    ├── Areas/
    │   └── Admin/             # Khu vực quản trị hệ thống (Admin Panel)
    │       ├── Controllers/   # Bộ điều khiển quản lý: Sản phẩm, Danh mục, Đơn hàng...
    │       └── Views/         # Giao diện quản trị AdminLTE
    ├── Controllers/           # Bộ điều khiển phân hệ khách hàng (Home, Product, Cart...)
    ├── Data/                  # Tầng truy xuất dữ liệu (DbContext, Generic Repository, UoW)
    ├── Models/                # Các lớp thực thể (Entity Models) và ViewModels
    ├── Services/              # Tầng xử lý nghiệp vụ (CartService, OrderService, EmailService)
    ├── Views/                 # Giao diện Razor Views dành cho người dùng
    ├── Content/               # Tài nguyên tĩnh: CSS, JavaScript, hình ảnh sản phẩm
    ├── Web.config             # Tệp cấu hình ứng dụng (Connection strings, App settings)
    └── Global.asax.cs         # Điểm khởi nhập và cấu hình vòng đời ứng dụng
```

---

## 6. Mục đích sử dụng

Dự án được xây dựng phục vụ mục đích học thuật, nghiên cứu và báo cáo đồ án học phần môn Lập trình ASP.NET