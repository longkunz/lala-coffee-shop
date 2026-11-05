# Ticket 03: Xác thực chữ ký số Checksum cho cổng thanh toán MoMo và VNPay

Status: resolved
Type: task

## Vấn đề
- Tại [`PayController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/PayController.cs#L132-L179) trong `MomoProcessing` và [`PayController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/PayController.cs#L272-L321) trong `VNPayProcessing`:
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

## Kết quả (Answer)
- Đã triển khai các helper xác thực chữ ký chuẩn xác cho cả hai cổng thanh toán:
  - [`VnPaySecurityHelper.cs`](file:///D:/UniCafe/LalaCoffee/Services/Payment/VnPaySecurityHelper.cs):
    - Tự động trích xuất các tham số `vnp_*` từ `NameValueCollection` hoặc `IDictionary<string, string>`, loại trừ `vnp_SecureHash` và `vnp_SecureHashType`.
    - Sắp xếp key theo thứ tự ASCII, chuẩn hóa URL encoding cho key và value.
    - Tính toán chữ ký HMAC-SHA512 với `vnp_HashSecret` (tự động đọc từ `WebConfigurationManager.AppSettings["vnp_HashSecret"]` hoặc biến môi trường).
    - So sánh hash không phân biệt chữ hoa/thường (`StringComparison.OrdinalIgnoreCase`).
  - [`MomoSecurityHelper.cs`](file:///D:/UniCafe/LalaCoffee/Services/Payment/MomoSecurityHelper.cs) & [`MomoSecurity.cs`](file:///D:/UniCafe/LalaCoffee/Services/Payment/Momo/MomoSecurity.cs):
    - Xây dựng raw signature theo đúng chuẩn tài liệu MoMo: `accessKey={0}&amount={1}&extraData={2}&message={3}&orderId={4}&orderInfo={5}&orderType={6}&partnerCode={7}&payType={8}&requestId={9}&responseTime={10}&resultCode={11}&transId={12}`.
    - Tính HMAC-SHA256 với `secretKey_Momo`.
    - Hỗ trợ xác thực từ `MomoResult` model, `NameValueCollection` và `IDictionary<string, string>`.
- Đã tích hợp kiểm tra chữ ký nghiêm ngặt tại [`PayController.cs`](file:///D:/UniCafe/LalaCoffee/Controllers/PayController.cs):
  - Trong `MomoProcessing`: Kiểm tra `MomoSecurityHelper.ValidateResponseSignature(result, secretKey, accessKey)`. Nếu không hợp lệ hoặc dữ liệu trống, chuyển hướng ngay về `ErrorPayment` và không ghi nhận thanh toán.
  - Trong `VNPayProcessing`: Kiểm tra `VnPaySecurityHelper.ValidateResponseSignature(Request.QueryString, hashSecret)`. Nếu chữ ký không hợp lệ, giả mạo hoặc thiếu `vnp_SecureHash`, chuyển hướng ngay về `ErrorPayment`.
- Đã bổ sung bộ unit test toàn diện [`PaymentSecurityTests.cs`](file:///D:/UniCafe/LalaCoffee.Tests/PaymentSecurityTests.cs) (25 test cases):
  - Kiểm tra test vector chuẩn HMAC SHA256 và HMAC SHA512.
  - Kiểm tra xử lý chuỗi query string VNPay (sắp xếp key ASCII, bỏ qua các key không phải `vnp_`, bỏ qua hash keys, phát hiện sửa đổi `vnp_Amount` hoặc `vnp_ResponseCode`).
  - Kiểm tra xử lý chữ ký MoMo (model `MomoResult`, dictionary, phát hiện sửa đổi `amount` hoặc `resultCode`).
  - Kiểm tra các trường hợp controller redirect về `ErrorPayment` khi chữ ký bị thiếu hoặc không khớp.

## Comments
