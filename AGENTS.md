# Hướng Dẫn Dành Cho AI Agent (AGENTS.md)

Tài liệu này cung cấp các quy tắc, công cụ và quy chuẩn vận hành bắt buộc dành cho các AI Coding Agent làm việc trên dự án **LalaCoffee** (nguyên bản UniCafe, chuyển đổi sang .NET Framework 4.8). Repository: `https://github.com/longkunz/lala-coffee-shop.git`.

---

<!-- codebase-memory-mcp:start -->
## Bộ nhớ đồ thị mã nguồn (Codebase Knowledge Graph - codebase-memory-mcp)

Dự án này sử dụng công cụ MCP `codebase-memory-mcp` để duy trì đồ thị tri thức mã nguồn.
**LUÔN LUÔN ưu tiên các công cụ đồ thị MCP thay vì grep/glob/tìm kiếm file thông thường để khám phá mã nguồn.**

### Thứ tự ưu tiên công cụ:
1. `search_graph`: Tìm kiếm hàm, lớp (class), route, biến theo pattern.
2. `trace_path`: Truy vết luồng gọi hàm (ai gọi hàm này, hoặc hàm này gọi những hàm nào).
3. `get_code_snippet`: Đọc mã nguồn cụ thể của một hàm hoặc class.
4. `check_index_coverage`: Kiểm tra phạm vi index của các đường dẫn để đảm bảo không bỏ sót mã nguồn trước khi đưa ra kết luận.
5. `query_graph`: Thực thi các truy vấn Cypher phức tạp trên đồ thị.
6. `get_architecture`: Xem tổng quan kiến trúc, phân lớp và các điểm nóng (hotspots) của dự án.

### Các cấp độ bằng chứng (Evidence tiers):
- **Scout (Tier 1):** Tìm kiếm nhanh với số lần gọi ít và kiểm tra nguồn có mục tiêu. Đánh dấu kết quả là tạm thời; không đưa ra kết luận phủ định tuyệt đối.
- **Verify (Tier 2, mặc định):** Thu thập bằng chứng đồ thị theo nhiệm vụ, kiểm tra hướng trace liên quan, đọc chính xác snippet mã nguồn.
- **Auditor (Tier 3):** Xác minh toàn diện trong phạm vi giới hạn, phân trang đầy đủ, kiểm tra cả hai chiều gọi hàm và quan hệ rộng hơn.

### Khi nào dùng grep/glob truyền thống:
- Tìm kiếm chuỗi ký tự cố định (string literals), thông báo lỗi, giá trị trong file cấu hình (`Web.config`).
- Tìm kiếm các file không phải code (file cấu hình, `.sln`, `.csproj`, tài liệu).
- Khi công cụ MCP không trả về kết quả đầy đủ.
<!-- codebase-memory-mcp:end -->

---

## Kỹ năng Agent (Agent skills)

### Trình theo dõi vấn đề (Issue tracker)

Các issue, ticket và bản đặc tả yêu cầu (specs) được lưu trữ dưới dạng các file Markdown cục bộ tại thư mục `.scratch/`. Xem chi tiết tại [`docs/agents/issue-tracker.md`](file:///D:/UniCafe/docs/agents/issue-tracker.md).

### Nhãn phân loại (Triage labels)

Hệ thống sử dụng 5 vai trò phân loại tiêu chuẩn được ánh xạ 1:1 (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). Xem chi tiết tại [`docs/agents/triage-labels.md`](file:///D:/UniCafe/docs/agents/triage-labels.md).

### Tài liệu miền nghiệp vụ (Domain docs)

Dự án áp dụng cấu trúc đơn ngữ cảnh (single-context: `GLOSSARY.md` + `docs/adr/` ở thư mục gốc). Xem chi tiết tại [`docs/agents/domain.md`](file:///D:/UniCafe/docs/agents/domain.md).
