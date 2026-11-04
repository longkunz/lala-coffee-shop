# Đặc Tả Kỹ Thuật: Chuyển Đổi Toàn Diện LalaCoffee (.NET 4.8) & Khắc Phục Lỗi (Spec)

Status: ready-for-agent

## Problem Statement

Hiện tại, người quản trị và khách hàng của hệ thống cửa hàng cà phê trực tuyến đang đối mặt với những vấn đề nghiêm trọng:
1. **Rủi ro an ninh mạng cao**: Hệ thống chứa lỗ hổng nghiêm trọng cho phép kẻ gian tải lên file tùy ý (tiềm ẩn nguy cơ thực thi mã độc từ xa - RCE qua bộ tải ảnh), mật khẩu email quản trị bị để lộ công khai, các cổng thanh toán điện tử (MoMo, VNPay) không xác thực chữ ký số dẫn đến nguy cơ bị tạo đơn hàng ảo mà không thực thanh toán, và các chức năng xóa/sửa dữ liệu danh mục hàng hóa bị phơi bày công khai không cần đăng nhập.
2. **Thất thoát tài chính từ lỗi nghiệp vụ**: Khách hàng đặt mua nhiều sản phẩm (ví dụ 10 ly cà phê) chỉ bị tính tiền cho 1 ly duy nhất do công thức tính tiền bỏ quên số lượng đặt mua (`Quantity`). Đơn hàng thanh toán trực tuyến bị mất trắng nếu khách hàng vô tình đóng trình duyệt hoặc mạng chập chờn do dữ liệu chỉ tạm lưu trong phiên làm việc (Session) thay vì ghi nhận vào cơ sở dữ liệu.
3. **Thương hiệu và nền tảng cũ kỹ**: Dự án đang mang tên cũ (UniCafe), lịch sử mã nguồn Git chứa nhiều tàn dư xung đột và file rác, nền tảng dừng ở .NET Framework 4.7.2 thiếu các bản vá tối ưu hiệu năng và bảo mật mới nhất của .NET Framework 4.8.
4. **Trải nghiệm người dùng và hiệu năng suy giảm**: Hệ thống thường xuyên bị đơ nghẽn khi gửi email đơn hàng, tải trang chậm do truy vấn cơ sở dữ liệu trùng lặp (N+1) trên thanh điều hướng menu, và giao diện nền tảng dùng phiên bản cũ không tối ưu trên thiết bị di động.

## Solution

Giải pháp chuyển đổi toàn diện dự án qua 3 giai đoạn chặt chẽ:
1. **Làm mới nền tảng & Nhận diện thương hiệu (Giai đoạn 0)**:
   - Nâng cấp Target Framework lên **.NET Framework 4.8**.
   - Tái cấu trúc đổi tên toàn bộ dự án sang **LalaCoffee** (thư mục, solution, project, namespace C# và toàn bộ Razor views).
   - Làm mới lịch sử Git, khởi tạo nhánh `main` sạch sẽ và kết nối tới repository mới: `https://github.com/longkunz/lala-coffee-shop.git`.
2. **Khắc phục triệt để toàn bộ lỗi bảo mật & nghiệp vụ (Giai đoạn 1)**:
   - Khóa chặt chức năng tải ảnh: áp dụng phân quyền Admin, whitelist định dạng ảnh an toàn, đổi tên file ngẫu nhiên và chặn tấn công đường dẫn.
   - Bảo mật thông tin cấu hình, loại bỏ mật khẩu hardcode trong mã nguồn.
   - Xác thực chữ ký số HMAC 2 chiều cho MoMo và VNPay (cả Return URL và Webhook IPN).
   - Đóng toàn bộ các chức năng thêm/sửa/xóa ngoài khu vực công khai, chỉ cho phép quản trị viên thao tác trong Admin Area.
   - Sửa dứt điểm công thức tính tiền theo đúng số lượng sản phẩm, lưu đơn hàng vào Database ngay lúc bấm đặt hàng với trạng thái Chờ thanh toán (`Pending`).
   - Kích hoạt cơ chế chống giả mạo request CSRF (`[ValidateAntiForgeryToken]`) trên toàn bộ hệ thống và sửa đường dẫn đăng nhập.
   - Giải phóng tài nguyên kết nối cơ sở dữ liệu (`Dispose`), tối ưu hóa truy vấn menu thành 1 lần đọc và đưa thao tác lưu dữ liệu ra ngoài vòng lặp.
   - Dọn dẹp sạch sẽ các file xung đột Git cũ và mã nguồn chết.
3. **Hiện đại hóa kiến trúc & Giao diện (Giai đoạn 2)**:
   - Áp dụng Clean Architecture: Tách biệt nghiệp vụ vào tầng Service độc lập (`IOrderService`, `IPaymentService`, `IEmailService`, `ICartService`).
   - Kích hoạt Unity 5 Dependency Injection container chính thức.
   - Nâng cấp giao diện người dùng lên Bootstrap 5 thông qua LibMan, tinh gọn và tối ưu responsive.
   - Tích hợp NLog ghi log xoay vòng chuyên nghiệp và gửi email bất đồng bộ không chặn luồng.

## User Stories

### Nhóm Khách Hàng (Customer / Shopper)
1. Là một khách hàng, tôi muốn duyệt xem menu các loại đồ uống theo danh mục với tốc độ tải trang nhanh chóng, để tôi có thể chọn món mà không phải chờ đợi lâu.
2. Là một khách hàng, tôi muốn tùy chọn kích thước (size) và các loại topping kèm theo cho từng ly đồ uống, để nhận được sản phẩm đúng với khẩu vị cá nhân.
3. Là một khách hàng, tôi muốn thêm nhiều ly đồ uống cùng loại hoặc khác loại vào giỏ hàng, để tổng tiền trong giỏ phản ánh chính xác đơn giá nhân với số lượng đặt mua.
4. Là một khách hàng, tôi muốn cập nhật số lượng hoặc xóa món khỏi giỏ hàng một cách mượt mà, để đơn hàng luôn khớp với mong muốn trước khi thanh toán.
5. Là một khách hàng, tôi muốn nhập thông tin nhận hàng (tên, số điện thoại, địa chỉ) với thông báo lỗi rõ ràng nếu nhập sai định dạng, để nhân viên giao hàng liên hệ được chính xác.
6. Là một khách hàng, tôi muốn chọn thanh toán khi nhận hàng (COD), để có thể trả tiền mặt linh hoạt khi nhận sản phẩm.
7. Là một khách hàng, tôi muốn chọn thanh toán qua Ví điện tử MoMo, để có thể quét mã QR tiện lợi trên ứng dụng di động.
8. Là một khách hàng, tôi muốn chọn thanh toán qua Cổng VNPay, để có thể thanh toán trực tiếp từ tài khoản ngân hàng nội địa.
9. Là một khách hàng, tôi muốn thông tin đơn hàng của mình được lưu trữ an toàn ngay khi bấm thanh toán, để nếu lỡ mất kết nối mạng tôi vẫn không bị mất đơn hàng đã đặt.
10. Là một khách hàng, tôi muốn nhận được email xác nhận đơn hàng chi tiết ngay sau khi hoàn tất đặt hàng, để tôi có thể kiểm tra lại thông tin và xem liên kết theo dõi đơn hàng.
11. Là một khách hàng, tôi muốn tra cứu trạng thái đơn hàng của mình bằng mã đơn hàng trên website, để biết khi nào đơn hàng được chuẩn bị và giao đến nơi.
12. Là một khách hàng, tôi muốn dữ liệu phiên đăng nhập và giao dịch của mình được bảo vệ an toàn khỏi các cuộc tấn công CSRF, để tránh nguy cơ bị kẻ gian lợi dụng phiên đăng nhập.
13. Là một khách hàng, tôi muốn trải nghiệm giao diện cửa hàng hiển thị đẹp mắt, trực quan và chuẩn xác trên cả màn hình điện thoại lẫn máy tính để bàn.

### Nhóm Quản Trị Viên (Store Admin)
14. Là một quản trị viên, tôi muốn truy cập bảng điều khiển quản trị bằng tài khoản có quyền Admin, để bảo vệ hệ thống khỏi những truy cập trái phép.
15. Là một quản trị viên, tôi muốn khi chưa đăng nhập mà truy cập trang quản trị thì hệ thống tự động đưa tôi về trang đăng nhập của cửa hàng, để tôi biết cần xác thực danh tính.
16. Là một quản trị viên, tôi muốn thêm mới sản phẩm đồ uống với đầy đủ hình ảnh, mô tả, danh mục, giá bán và tùy chọn size/topping, để làm phong phú thực đơn bán lẻ.
17. Là một quản trị viên, tôi muốn tải ảnh đại diện cho đồ uống một cách an toàn mà không lo lắng về các file độc hại, để giữ cho máy chủ luôn an toàn.
18. Là một quản trị viên, tôi muốn chỉnh sửa hoặc tạm dừng kinh doanh một món đồ uống, để cập nhật kịp thời tình trạng tồn kho của quán.
19. Là một quản trị viên, tôi muốn quản lý danh sách danh mục đồ uống bên trong khu vực quản trị an toàn, để đảm bảo người ngoài không thể tự ý thay đổi dữ liệu của quán.
20. Là một quản trị viên, tôi muốn theo dõi danh sách tất cả các đơn hàng mới phát sinh theo thời gian thực, với đầy đủ thông tin khách hàng, chi tiết món và trạng thái thanh toán.
21. Là một quản trị viên, tôi muốn cập nhật trạng thái đơn hàng (Đã xác nhận, Đang pha chế, Đang giao hàng, Hoàn thành, Hủy), để điều phối hoạt động quán hiệu quả.
22. Là một quản trị viên, tôi muốn xem thống kê doanh thu theo từng tháng và tỷ lệ các phương thức thanh toán dạng biểu đồ trực quan, để nắm bắt tình hình kinh doanh.
23. Là một quản trị viên, tôi muốn quản lý tài khoản nhân viên và phân quyền rõ ràng, để mỗi nhân sự thao tác đúng nhiệm vụ được giao.

### Nhóm Kỹ Sư & Vận Hành Hệ Thống (Developer & DevOps)
24. Là một kỹ sư phần mềm, tôi muốn dự án chạy trên nền tảng .NET Framework 4.8 mới nhất của dòng 4.x, để tận dụng các cải tiến về hiệu năng runtime và bảo mật TLS.
25. Là một kỹ sư phần mềm, tôi muốn toàn bộ mã nguồn sử dụng đồng nhất tên thương hiệu LalaCoffee trong solution, project và namespace, để mã nguồn chuyên nghiệp, dễ bảo trì.
26. Là một kỹ sư phần mềm, tôi muốn làm việc trên một repository Git mới tinh, không còn tàn dư file rác hay xung đột cũ, để việc quản lý phiên bản được chuẩn chỉ.
27. Là một kỹ sư phần mềm, tôi muốn toàn bộ logic nghiệp vụ được đóng gói trong tầng Service độc lập thay vì nhồi nhét trong Controller, để dễ dàng kiểm thử tự động (Unit Test).
28. Là một kỹ sư phần mềm, tôi muốn các phụ thuộc (Dependencies) được inject tự động qua Unity DI Container, để mã nguồn đạt tính kết dính lỏng lẻo (Loosely Coupled).
29. Là một kỹ sư phần mềm, tôi muốn dữ liệu truyền từ view lên controller được định kiểu tường minh qua ViewModel và validate bằng DataAnnotations, để loại bỏ lỗi ép kiểu chuỗi thủ công.
30. Là một kỹ sư phần mềm, tôi muốn các sự cố ngoại lệ của hệ thống được ghi log đầy đủ qua NLog với stack trace chi tiết vào file riêng, để có thể chẩn đoán và khắc phục lỗi nhanh chóng mà không làm lộ thông tin nhạy cảm cho người dùng cuối.

## Implementation Decisions

### 1. Nâng cấp Target Framework và Đổi tên Thương hiệu (Giai đoạn 0)
- Nâng cấp phiên bản framework mục tiêu của dự án từ .NET Framework 4.7.2 lên **.NET Framework 4.8**.
- Thống nhất tên dự án và namespace gốc là **LalaCoffee**. Toàn bộ mã nguồn C#, Controller, Model, Service, Helper, và các cấu hình trong Razor Views đều được chuyển đổi đồng bộ từ `UniCafe` sang `LalaCoffee`.
- Xóa bỏ lịch sử Git cũ và khởi tạo lại repository mới trên nhánh chính `main`, trỏ kết nối remote về `https://github.com/longkunz/lala-coffee-shop.git`.

### 2. Chuẩn hóa Bảo mật & Ngăn chặn Lỗ hổng (Giai đoạn 1)
- **Tải tệp tin an toàn**: Chỉ cho phép người dùng có vai trò `Admin` tải lên file. Áp dụng cơ chế kiểm tra whitelist phần mở rộng (`.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`), giới hạn dung lượng tải lên tối đa 5MB, chuẩn hóa tên file bằng mã định danh duy nhất (GUID) kết hợp với `Path.GetFileName()` để triệt tiêu hoàn toàn nguy cơ Path Traversal và Web Shell RCE.
- **Bảo mật cấu hình**: Mật khẩu ứng dụng của hòm thư gửi thông báo và các mã khóa bí mật được quản lý tập trung trong file cấu hình, loại bỏ toàn bộ mật khẩu cứng trong các file code C#.
- **Xác thực chữ ký cổng thanh toán**: Bắt buộc tính toán và đối chiếu mã kiểm tra tính toàn vẹn (Checksum HMAC SHA256 đối với MoMo và HMAC SHA512 đối với VNPay) trên toàn bộ dữ liệu phản hồi trả về từ cổng thanh toán trước khi chấp nhận thay đổi trạng thái đơn hàng sang Đã thanh toán.
- **Kiểm soát quyền truy cập**: Toàn bộ thao tác thêm, sửa, xóa danh mục, thuộc tính và tùy chọn đồ uống được loại bỏ khỏi khu vực truy cập công cộng và chỉ được vận hành trong phân vùng quản trị (`Admin Area`) có chỉ định `[Authorize(Roles = "Admin")]`.
- **Chống tấn công CSRF**: Bổ sung bộ lọc xác thực mã chống giả mạo request trên toàn bộ các phương thức tiếp nhận dữ liệu HTTP POST của hệ thống.
- **Điều hướng đăng nhập**: Sửa đổi cấu hình xác thực người dùng để tự động chuyển hướng các truy cập trái phép về trang đăng nhập chính thức thay vì trang chủ.

### 3. Chuẩn hóa Nghiệp vụ Đơn hàng & Tính toán Tiền tệ (Giai đoạn 1)
- **Công thức tính tiền chuẩn**: Giá trị một dòng sản phẩm trong đơn hàng bắt buộc phải tuân theo công thức: `(Đơn giá sản phẩm gốc + Giá thuộc tính size + Tổng giá các tùy chọn topping) * Số lượng đặt mua`. Tổng tiền đơn hàng là tổng giá trị của toàn bộ các dòng sản phẩm.
- **Quy trình lưu trữ đơn hàng bền vững**: Ngay khi khách hàng nhấn xác nhận đặt đơn tại trang Thanh toán, hệ thống lập tức khởi tạo bản ghi đơn hàng trong cơ sở dữ liệu với trạng thái Chờ thanh toán (`Pending`) và lưu giữ mã đơn hàng duy nhất. Cổng thanh toán trực tuyến sẽ làm việc dựa trên mã đơn hàng này; khi thanh toán thành công, hệ thống cập nhật bản ghi đã có thay vì phụ thuộc vào Session trình duyệt.
- **Tối ưu hóa thao tác cơ sở dữ liệu**: Dữ liệu chi tiết đơn hàng được thêm hàng loạt vào tập hợp dữ liệu và chỉ thực thi ghi nhận thay đổi xuống cơ sở dữ liệu đúng một lần sau khi hoàn tất vòng lặp, thay vì thực thi lặp đi lặp lại trong từng bước lặp.

### 4. Tối ưu Hiệu năng & Quản lý Tài nguyên (Giai đoạn 1 & 2)
- **Giải phóng kết nối cơ sở dữ liệu**: Lớp Controller cơ sở cài đặt phương thức dọn dẹp tài nguyên nhằm đảm bảo các phiên làm việc với cơ sở dữ liệu và trình quản lý người dùng luôn được giải phóng ngay khi yêu cầu HTTP kết thúc.
- **Triệt tiêu vấn đề truy vấn lặp (N+1 Query)**: Dữ liệu cây danh mục cho thanh điều hướng menu được nạp một lần duy nhất từ cơ sở dữ liệu vào bộ nhớ và tiến hành phân cấp cha - con ngay trên bộ nhớ, loại bỏ các câu truy vấn lặp lồng nhau.
- **Gửi thư bất đồng bộ**: Hoạt động gửi email thông báo đơn hàng được chuyển sang phương thức gửi bất đồng bộ, tách biệt khỏi luồng xử lý chính để đảm bảo sự cố gián đoạn từ máy chủ gửi thư không gây ảnh hưởng hay làm gián đoạn giao dịch của khách hàng.
- **Ghi vết hệ thống**: Tích hợp thư viện NLog để tự động ghi nhận các sự kiện lỗi vào tệp tin lưu trữ theo ngày, ẩn toàn bộ chi tiết kỹ thuật nhạy cảm đối với người dùng cuối.

### 5. Kiến trúc Hệ thống & Giao diện (Giai đoạn 2)
- Tách biệt rõ ràng các tầng: Tầng Giao diện (Controllers/Views) chỉ đảm nhiệm hiển thị và nhận request; Tầng Nghiệp vụ (Services) đảm nhiệm xử lý logic đơn hàng, giỏ hàng, thanh toán và gửi thư; Tầng Dữ liệu (Repositories/UnitOfWork) đảm nhiệm thao tác cơ sở dữ liệu.
- Kích hoạt trình giải quyết phụ thuộc Unity để tiêm các dịch vụ vào Controller.
- Sử dụng đối tượng truyền tải dữ liệu (DTO/ViewModel) có định kiểu rõ ràng và ràng buộc kiểm tra hợp lệ thay thế hoàn toàn việc đọc chuỗi dữ liệu tự do.
- Hiện đại hóa giao diện người dùng bằng Bootstrap 5 quản lý thông qua công cụ quản lý thư viện client-side (LibMan).

## Testing Decisions

### 1. Triết lý & Tiêu chuẩn Kiểm thử
- **Kiểm thử hành vi bên ngoài (External Behavior Testing)**: Các bài kiểm thử tập trung đánh giá kết quả trả về của hệ thống theo hành vi mong đợi của người dùng và hợp đồng nghiệp vụ, không ràng buộc vào các chi tiết cài đặt nội bộ.
- **Tính độc lập & Lặp lại**: Mọi kịch bản kiểm thử phải có khả năng chạy độc lập, tự làm sạch dữ liệu kiểm thử và cho ra cùng một kết quả trên mọi môi trường chạy.

### 2. Các điểm nối kiểm thử (Testing Seams)
Để đảm bảo kiến trúc tinh gọn và hạn chế tối đa số lượng điểm nối phân mảnh, hệ thống thiết lập **2 điểm nối kiểm thử trọng yếu**:
1. **Điểm nối cấp cao nhất (Primary / Highest Seam) - HTTP Controller Action Boundary**:
   - Kiểm thử tại ranh giới tiếp nhận và phản hồi của Controller.
   - Truyền dữ liệu đầu vào tương đương HTTP Request và xác nhận kết quả đầu ra (mã phản hồi HTTP, chuyển hướng Redirect, dữ liệu trả về View/JSON, và trạng thái cập nhật trong cơ sở dữ liệu).
   - Điểm nối này bao phủ toàn diện từ quá trình xác thực dữ liệu đầu vào, xử lý nghiệp vụ, cho đến phản hồi của hệ thống.
2. **Điểm nối tầng dịch vụ (Service Layer Seam)**:
   - Kiểm thử trực tiếp tại ranh giới của các interface dịch vụ: `IOrderService`, `IPaymentService`, `ICartManager`, `IEmailService`.
   - Phục vụ kiểm thử sâu các thuật toán tính tiền với nhiều tổ hợp số lượng / topping khác nhau, kiểm tra logic băm và giải mã chữ ký số bảo mật của MoMo/VNPay, và kiểm tra cơ chế cô lập lỗi khi gửi email.

### 3. Các kịch bản kiểm thử trọng tâm
- **Kiểm thử Tính tiền đơn hàng**: Đặt hàng với số lượng 1, nhiều sản phẩm, có và không có size/topping; xác nhận tổng tiền đơn hàng bằng chính xác tổng tiền từng món nhân với số lượng.
- **Kiểm thử Xác thực Chữ ký Thanh toán**: Gửi dữ liệu phản hồi giả mạo với chữ ký sai; xác nhận hệ thống từ chối cập nhật trạng thái đơn hàng. Gửi dữ liệu có chữ ký hợp lệ; xác nhận hệ thống chuyển trạng thái sang Đã thanh toán.
- **Kiểm thử Tải ảnh an toàn**: Thử tải lên file có đuôi `.aspx`, `.php`, `.exe`; xác nhận hệ thống từ chối. Thử tải file `.png`, `.jpg` hợp lệ; xác nhận file được lưu với tên mới an toàn.
- **Kiểm thử Kiểm soát truy cập**: Người dùng chưa đăng nhập gọi action Admin; xác nhận bị từ chối và chuyển hướng về trang đăng nhập.

## Out of Scope

Các hạng mục sau **không nằm trong phạm vi** của bản đặc tả này:
1. Chuyển đổi toàn bộ kiến trúc sang ASP.NET Core (.NET 8/9): Dự án giữ nguyên nền tảng .NET Framework 4.8 theo quyết định kiến trúc đã thống nhất.
2. Tách ứng dụng thành Single Page Application (SPA) viết bằng React/Next.js hoặc Vue: Hệ thống tiếp tục sử dụng kiến trúc Monolith với Razor Views Server-side Rendering.
3. Tích hợp thêm các cổng thanh toán mới ngoài MoMo và VNPay (như ZaloPay, PayPal, Stripe).
4. Xây dựng ứng dụng di động native (iOS / Android).

## Further Notes

- Kế hoạch triển khai được thực hiện lần lượt theo 3 giai đoạn: Giai đoạn 0 (Nền tảng & Git mới) -> Giai đoạn 1 (Khắc phục triệt để lỗi và bảo mật) -> Giai đoạn 2 (Hiện đại hóa kiến trúc và giao diện).
- Khi Giai đoạn 0 hoàn tất, toàn bộ mã nguồn sẽ thuộc repository `https://github.com/longkunz/lala-coffee-shop.git` với nhánh mặc định là `main`.
