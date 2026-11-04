# Tài liệu miền nghiệp vụ (Domain Docs)

Tài liệu này hướng dẫn cách các kỹ năng kỹ thuật và AI Agent tiếp cận, sử dụng tài liệu miền nghiệp vụ khi khám phá và thay đổi mã nguồn trong dự án.

## Trước khi khám phá mã nguồn, hãy đọc các tài liệu sau

- **`GLOSSARY.md`** tại thư mục gốc của repo (nếu có): Chứa định nghĩa các thuật ngữ chuẩn của miền nghiệp vụ.
- **`docs/adr/`**: Đọc các bản ghi quyết định kiến trúc (Architecture Decision Records - ADR) có liên quan đến khu vực bạn chuẩn bị can thiệp.

> [!NOTE]
> Nếu các file trên chưa tồn tại trong repo, hãy **tiến hành công việc trong im lặng (proceed silently)**. Không cảnh báo về sự vắng mặt của chúng và không chủ động đề xuất tạo trước. Kỹ năng `/domain-modeling` sẽ tự động tạo chúng một cách lười (lazily) khi các khái niệm hoặc quyết định thực sự được giải quyết.

## Cấu trúc thư mục

Dự án này áp dụng cấu trúc đơn ngữ cảnh (Single-context repo):

```
UniCafe/
├── GLOSSARY.md                  # Thuật ngữ miền nghiệp vụ
├── docs/
│   ├── adr/                     # Các quyết định kiến trúc hệ thống
│   │   ├── 0001-aspnet-mvc-architecture.md
│   │   └── 0002-ef6-repository-pattern.md
│   └── agents/                  # Cấu hình kỹ năng cho AI Agent
│       ├── issue-tracker.md
│       ├── triage-labels.md
│       └── domain.md
├── .scratch/                    # Nơi lưu trữ issues, specs cục bộ
└── UniCafe/                     # Mã nguồn ứng dụng
```

## Sử dụng từ vựng chuẩn từ Glossary

Khi kết quả đầu ra của bạn đặt tên cho một khái niệm nghiệp vụ (trong tiêu đề issue, đề xuất tái cấu trúc, giả thuyết, tên hàm hoặc tên kiểm thử), hãy sử dụng đúng thuật ngữ đã được định nghĩa trong `GLOSSARY.md`. Tránh sử dụng các từ đồng nghĩa mà bảng thuật ngữ đã khuyến cáo không dùng.

## Báo hiệu xung đột với ADR

Nếu giải pháp hoặc kết quả của bạn mâu thuẫn với một ADR hiện có trong `docs/adr/`, hãy nêu rõ ràng vấn đề này lên thay vì âm thầm ghi đè quyết định:

> *"Đề xuất này mâu thuẫn với ADR-0002 (mô hình Repository hiện tại), tuy nhiên cần xem xét mở lại vì..."*
