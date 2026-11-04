# Trình theo dõi vấn đề: Markdown cục bộ (Local Markdown)

Các issue, ticket và bản đặc tả yêu cầu (specs) của dự án này được lưu trữ dưới dạng các file Markdown bên trong thư mục `.scratch/`.

## Các quy ước cấu trúc

- **Mỗi tính năng một thư mục**: `.scratch/<feature-slug>/`
- **Bản đặc tả yêu cầu (Spec)**: `.scratch/<feature-slug>/spec.md`
- **Các ticket triển khai**: Mỗi ticket là một file riêng biệt tại `.scratch/<feature-slug>/issues/<NN>-<slug>.md`, đánh số từ `01` trở đi. Tuyệt đối không gộp chung vào một file duy nhất.
- **Trạng thái phân loại**: Được ghi tại dòng `Status:` ở gần đầu mỗi file issue (xem [`triage-labels.md`](file:///D:/UniCafe/docs/agents/triage-labels.md) để biết các nhãn vai trò).
- **Lịch sử trao đổi**: Bình luận và ghi chú thảo luận được bổ sung ở cuối file dưới tiêu đề `## Bình luận (Comments)`.

## Khi một kỹ năng yêu cầu "xuất bản lên trình theo dõi vấn đề" (publish to the issue tracker)

Tạo file mới bên dưới thư mục `.scratch/<feature-slug>/` (tự động tạo thư mục nếu chưa tồn tại).

## Khi một kỹ năng yêu cầu "lấy ticket liên quan" (fetch the relevant ticket)

Đọc nội dung file tại đường dẫn được tham chiếu. Thông thường người dùng sẽ truyền trực tiếp đường dẫn file hoặc số thứ tự issue.

## Các thao tác định hướng (Wayfinding operations)

Được sử dụng bởi kỹ năng `/wayfinder`. File **bản đồ (map)** chứa danh sách con trỏ đến từng ticket con.

- **Bản đồ (Map)**: `.scratch/<effort>/map.md` (chứa các phần: Ghi chú / Quyết định đã chốt / Vấn đề chưa rõ).
- **Ticket con**: `.scratch/<effort>/issues/NN-<slug>.md`, đánh số từ `01`, nội dung câu hỏi/yêu cầu nằm ở phần thân. Dòng `Type:` ghi loại ticket (`research` / `prototype` / `grilling` / `task`); dòng `Status:` ghi `claimed` hoặc `resolved`.
- **Ràng buộc chặn (Blocking)**: Dòng `Blocked by: NN, NN` ở gần đầu. Một ticket chỉ được xem là mở khóa khi tất cả các ticket chặn nó đã có trạng thái `resolved`.
- **Tìm việc tiếp theo (Frontier)**: Quét thư mục `.scratch/<effort>/issues/` tìm các file đang mở (open), không bị chặn (unblocked), và chưa có ai nhận (unclaimed); ưu tiên chọn file có số thứ tự nhỏ nhất.
- **Nhận việc (Claim)**: Đặt `Status: claimed` và lưu file trước khi bắt đầu triển khai.
- **Hoàn thành (Resolve)**: Bổ sung kết quả bên dưới tiêu đề `## Kết quả (Answer)`, cập nhật `Status: resolved`, sau đó thêm con trỏ ngữ cảnh (tóm tắt kết quả + liên kết file) vào phần Quyết định đã chốt trong file `map.md`.
