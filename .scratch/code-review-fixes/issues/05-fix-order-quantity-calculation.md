# Ticket 05: Sửa lỗi tính tổng tiền đơn hàng bị bỏ quên số lượng (Quantity) và lưu đơn hàng vào DB

Status: ready-for-agent
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

## Comments
