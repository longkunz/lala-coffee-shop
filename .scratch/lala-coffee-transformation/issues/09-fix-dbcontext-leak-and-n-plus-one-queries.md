# 09: Giải phóng kết nối cơ sở dữ liệu và tối ưu hóa truy vấn lặp N+1

**What to build:** Toàn bộ các phiên làm việc và kết nối đến cơ sở dữ liệu được giải phóng triệt để ngay sau khi yêu cầu người dùng hoàn thành, giúp máy chủ duy trì khả năng phục vụ cao mà không bị cạn kiệt Connection Pool. Dữ liệu menu phân cấp được nạp thông minh 1 lần vào bộ nhớ, và các thao tác lưu dữ liệu được gom lại thành 1 lần gọi duy nhất giúp trang web phản hồi tức thì.

**Blocked by:** 07 (Sửa lỗi tính tiền đơn hàng theo số lượng và lưu trữ đơn hàng bền vững trong Database)

**Status:** ready-for-agent

- [ ] Lớp Controller cơ sở và các controller độc lập thực thi phương thức dọn dẹp tài nguyên (`Dispose`) cho phiên làm việc cơ sở dữ liệu và các trình quản lý danh tính người dùng.
- [ ] Dữ liệu cây danh mục cho thanh điều hướng menu được truy vấn toàn bộ trong 1 câu truy vấn và tự động dựng cây phân cấp trong bộ nhớ, loại bỏ hoàn toàn các câu truy vấn lặp lồng nhau trên mỗi lượt tải trang.
- [ ] Thao tác lưu các dòng chi tiết đơn hàng được đưa ra ngoài vòng lặp duyệt giỏ hàng, chỉ thực hiện lưu cơ sở dữ liệu 1 lần duy nhất sau khi đã gom đủ dữ liệu.
- [ ] Các truy vấn đọc dữ liệu chỉ để hiển thị áp dụng cờ không theo dõi trạng thái (`AsNoTracking`) để tăng tốc độ phản hồi và giảm chiếm dụng bộ nhớ.
