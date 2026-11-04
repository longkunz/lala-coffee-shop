# 07: Sửa lỗi tính tiền đơn hàng theo số lượng và lưu trữ đơn hàng bền vững trong Database

**What to build:** Khách hàng đặt mua đồ uống với số lượng bất kỳ (1 hay nhiều ly) cùng các tùy chọn size và topping kèm theo được tính toán tổng số tiền chính xác tuyệt đối theo công thức: tổng tiền món nhân với số lượng. Ngay khi khách hàng bấm xác nhận đặt đơn, thông tin đơn hàng được lưu trữ bền vững vào cơ sở dữ liệu với trạng thái Chờ thanh toán (`Pending`), xóa bỏ hoàn toàn rủi ro mất đơn hàng khi tắt trình duyệt hoặc mất kết nối trong quá trình thanh toán trực tuyến.

**Blocked by:** 02 (Tái cấu trúc chuyển đổi tên thương hiệu sang LalaCoffee toàn diện)

**Status:** ready-for-agent

- [ ] Tổng tiền của từng dòng sản phẩm trong đơn hàng được tính đúng bằng đơn giá tổng (sản phẩm + size + các topping) nhân với số lượng đặt mua (`Quantity`).
- [ ] Tổng tiền của toàn bộ đơn hàng phản ánh chính xác tổng giá trị của toàn bộ các dòng sản phẩm nhân với số lượng tương ứng.
- [ ] Khi khách hàng đặt đơn (cả thanh toán COD lẫn chuyển sang cổng thanh toán online), bản ghi đơn hàng và chi tiết đơn hàng được ghi nhận ngay vào cơ sở dữ liệu với trạng thái ban đầu là Chờ thanh toán (`Pending`).
- [ ] Dữ liệu đơn hàng có mã tham chiếu duy nhất được tạo ra để đồng bộ với các cổng thanh toán.
- [ ] Người dùng tắt trình duyệt sau khi gửi đơn vẫn có thể tra cứu thấy đơn hàng của mình trong hệ thống bằng mã đơn hàng.
