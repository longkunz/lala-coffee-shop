# Ticket 03: Xác thực chữ ký số Checksum cho cổng thanh toán MoMo và VNPay

Status: claimed
Type: task

## Vấn đề
- Tại [`PayController.cs`](file:///D:/UniCafe/UniCafe/Controllers/PayController.cs#L132-L179) trong `MomoProcessing` và [`PayController.cs`](file:///D:/UniCafe/UniCafe/Controllers/PayController.cs#L272-L321) trong `VNPayProcessing`:
  - Hệ thống chỉ kiểm tra `result.resultCode == 0` hoặc `vnp_ResponseCode == 0` mà **hoàn toàn không xác thực chữ ký (Checksum/Signature)**.
  - Bất kỳ ai cũng có thể giả mạo callback bằng cách gọi trực tiếp `/Pay/VNPayProcessing?vnp_ResponseCode=0` để kích hoạt hoàn tất đơn hàng và gán `order.Paid = 1`.

## Yêu cầu kỹ thuật
1. Đối với **VNPay**:
   - Thu thập toàn bộ các tham số bắt đầu bằng `vnp_` từ `Request.QueryString` (trừ `vnp_SecureHash` và `vnp_SecureHashType`).
   - Sắp xếp các tham số theo thứ tự alphabet (ASCII order) và URL-encode giá trị.
   - Tính toán mã băm HMAC SHA512 với khóa bí mật `vnp_HashSecret`.
   - So sánh chuỗi hash tính được với `vnp_SecureHash`. Nếu khớp mới tiến hành xử lý cập nhật trạng thái đơn hàng.
2. Đối với **MoMo**:
   - Thu thập các trường trả về theo chuẩn tài liệu MoMo (partnerCode, orderId, requestId, amount, orderInfo, orderType, transId, resultCode, message, payType, responseTime, extraData).
   - Tạo rawHash và băm HMAC SHA256 với `secretKey_Momo`.
   - So sánh với `signature` từ MoMo gửi sang.

## Comments
