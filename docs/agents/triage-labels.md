# Nhãn phân loại vấn đề (Triage Labels)

Các kỹ năng làm việc dựa trên 5 vai trò phân loại tiêu chuẩn. Bảng dưới đây ánh xạ 5 vai trò đó với các chuỗi nhãn thực tế được sử dụng trong hệ thống theo dõi của dự án này:

| Nhãn trong mattpocock/skills | Nhãn trong hệ thống dự án | Ý nghĩa & Mục đích sử dụng |
| :--- | :--- | :--- |
| `needs-triage` | `needs-triage` | Issue mới tạo, cần người bảo trì/lead đánh giá và phân loại |
| `needs-info` | `needs-info` | Đang chờ người báo cáo bổ sung thêm thông tin chi tiết |
| `ready-for-agent` | `ready-for-agent` | Đã đặc tả đầy đủ, rõ ràng; sẵn sàng để AI Agent tự động thực hiện |
| `ready-for-human` | `ready-for-human` | Cần con người trực tiếp đánh giá hoặc thực hiện thủ công |
| `wontfix` | `wontfix` | Không xử lý / Không áp dụng trong phạm vi dự án |

## Hướng dẫn áp dụng

Khi một kỹ năng đề cập đến một vai trò (ví dụ: *"áp dụng nhãn sẵn sàng cho agent (ready-for-agent)"*), hãy sử dụng đúng chuỗi nhãn tương ứng từ bảng trên vào trường `Status:` của issue.
