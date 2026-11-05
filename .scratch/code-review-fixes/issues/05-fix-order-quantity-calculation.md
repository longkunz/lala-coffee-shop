# Ticket 05: Sửa lỗi tính tổng tiền đơn hàng bị bỏ quên số lượng (Quantity) và lưu đơn hàng vào DB

Status: resolved
Type: task

## Vấn đề
- Tại [`OrderController.cs`](file:///D:/UniCafe/UniCafe/Controllers/OrderController.cs#L140-L149) và [`PayController.cs`](file:///D:/UniCafe/UniCafe/Controllers/PayController.cs#L141-L165, L281-L305):
  - Khi duyệt qua `cart`, code tính `itemTotal` (giá sản phẩm + size + topping) của 1 ly nước và gán `orderDetail.Total = itemTotal` cùng `totalOrder += itemTotal` mà **hoàn toàn không nhân với `item.Quantity`**.
  - Khách mua số lượng lớn (ví dụ: 10 ly) chỉ phải trả tiền cho 1 ly duy nhất.
- Tại luồng thanh toán MoMo và VNPay:
  - Đơn hàng chỉ được lưu vào `Session["order"]` thay vì ghi vào Database lúc Checkout.
  - Nếu khách hàng đóng trình duyệt, Session timeout hoặc thanh toán qua IPN callback, đơn hàng sẽ mất vĩnh viễn dù tiền đã bị trừ.

## Yêu cầu kỹ thuật
1. Sử dụng phương thức chuẩn từ [`CartManager.cs`](file:///D:/UniCafe/UniCafe/Services/CartManager.cs#L89) (`GetTotal()`) hoặc đảm bảo công thức:
   `var lineTotal = itemTotal * item.Quantity;`
   `orderDetail.Total = lineTotal;`
   `totalOrder += lineTotal;`
2. Lưu đơn hàng vào cơ sở dữ liệu ngay khi Submit Checkout:
   - Trạng thái ban đầu: `Paid = 0, Status = "Pending"`.
   - Lưu `orderCode` vào Database và truyền mã đơn hàng sang cổng thanh toán.
3. Khi MoMo / VNPay callback thành công và xác thực chữ ký hợp lệ:
   - Tìm đơn hàng trong DB theo `orderCode` (hoặc `vnp_TxnRef` / `orderId`).
   - Cập nhật `order.Paid = 1, order.Status = "Paid"`.
4. Không phụ thuộc vào `Session["order"]` để tạo đơn hàng trong callback.

## Kết quả (Answer)
- Đã trích xuất và triển khai service tính toán độc lập [`OrderCalculator.cs`](file:///D:/UniCafe/LalaCoffee/Services/OrderCalculator.cs):
  - Phương thức `CalculateUnitPrice(basePrice, propertyPrice, optionPrices)` tính đơn giá 1 sản phẩm kèm size và toppings.
  - Phương thức `CalculateLineTotal(basePrice, propertyPrice, optionPrices, quantity)` tính chính xác tổng tiền từng dòng hàng nhân với `quantity`.
  - Phương thức `CalculateCartTotal(cart)` tính tổng tiền toàn bộ giỏ hàng, xử lý null-safety cho thuộc tính và tùy chọn.
  - Cập nhật [`CartManager.cs`](file:///D:/UniCafe/LalaCoffee/Services/CartManager.cs) để `GetTotal()` và `GetCartTotal()` cùng sử dụng `OrderCalculator.CalculateCartTotal()`.
- Đã tái cấu trúc luồng Checkout tại [`OrderController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/OrderController.cs):
  - Tính toán `lineTotal = OrderCalculator.CalculateLineTotal(...)` và gán `orderDetail.Total = lineTotal`, `totalOrder += lineTotal`.
  - Tạo `Order` với trạng thái ban đầu: `Paid = 0`, `Status = "Pending"`, `CreatedAt = DateTime.Now`, `UpdatedAt = DateTime.Now`.
  - Lưu ngay `order` và toàn bộ danh sách `orderDetail` vào Database trước khi chuyển hướng sang cổng thanh toán.
  - Truyền `order.Code` sang cổng thanh toán trực tuyến (`MomoPay` và `VNPay`).
  - Đối với thanh toán tiền mặt (cash/COD): cập nhật trạng thái `Status = "Confirmed"`, xóa giỏ hàng và gửi email xác nhận.
- Đã tái cấu trúc luồng thanh toán và callback tại [`PayController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/PayController.cs):
  - `MomoPay`: Truyền `order.Code` làm `orderId` sang MoMo.
  - `VNPay`: Truyền `order.Code` làm `vnp_TxnRef` sang VNPay.
  - `MomoProcessing` & `ProcessVNPay`:
    - Truy vấn đơn hàng đã lưu trong cơ sở dữ liệu theo `orderCode` (từ `result.orderId` hoặc `vnp_TxnRef`), không phụ thuộc vào `Session["order"]`.
    - Sau khi xác thực chữ ký và cổng thanh toán báo thành công: cập nhật `order.Paid = 1`, `order.Status = "Paid"`, `order.UpdatedAt = DateTime.Now`.
    - Gọi `_cartManager.ClearCart()` và gửi email xác nhận cho khách hàng.
    - Sửa toàn bộ fallback code tính chi tiết đơn hàng đảm bảo nhân với `item.Quantity`.
- Cập nhật view quản trị:
  - [`ManageOrder/Index.cshtml`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Views/ManageOrder/Index.cshtml): Hiển thị trạng thái linh hoạt theo nhãn màu (PENDING, PAID, CONFIRMED, ON, OFF).
  - [`ManageOrder/Edit.cshtml`](file:///D:/UniCafe/LalaCoffee/Areas/Admin/Views/ManageOrder/Edit.cshtml): Loại bỏ lỗi ép kiểu chuỗi `Convert.ToInt32(Model.Status)`.
  - [`Order/CheckOut.cshtml`](file:///D:/UniCafe/LalaCoffee/Views/Order/CheckOut.cshtml): Thống nhất hiển thị tổng tiền qua `OrderCalculator.CalculateCartTotal(carts)`.
- Bổ sung bộ unit test toàn diện [`OrderCalculationTests.cs`](file:///D:/UniCafe/LalaCoffee.Tests/OrderCalculationTests.cs) (13 tests):
  - Kiểm tra tính đơn giá cơ bản và kèm topping/size.
  - Kiểm tra tính line total với số lượng 1, nhiều số lượng và số lượng 0.
  - Kiểm tra tính tổng giỏ hàng với nhiều món, nhiều mức số lượng khác nhau và so sánh loại trừ lỗi thiếu nhân số lượng.
  - Kiểm tra tính nhất quán giữa `CartManager.GetCartTotal` và `OrderCalculator`.
  - Kiểm tra trạng thái khởi tạo `Order`.
  - Kiểm tra mã nguồn qua static source audit đảm bảo không còn dòng gán thiếu `Quantity` hay comment out lệnh lưu DB.
- Toàn bộ 108/108 unit tests trong `LalaCoffee.Tests` đều vượt qua thành công (100% pass).

## Comments
