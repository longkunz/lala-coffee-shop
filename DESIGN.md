---
name: LalaCoffee Design System
description: Hệ thống thiết kế chuẩn thủ công tôn vinh hương vị cà phê mộc Tây Nguyên và không gian thư thái.
colors:
  primary: "#8b572a"
  primary-dark: "#5c3716"
  primary-light: "#c19942"
  accent-gold: "#e4b95b"
  surface-cream: "#faf7f2"
  surface-white: "#ffffff"
  surface-dark: "#20202f"
  surface-espresso: "#1d1313"
  text-primary: "#2b1e16"
  text-secondary: "#5f5b57"
  text-muted: "#7a756f"
  text-light: "#ffffff"
  status-success: "#2e7d32"
  status-danger: "#c93b2b"
  status-warning: "#ed6c02"
  border-subtle: "#e8dfd5"
typography:
  display:
    fontFamily: "Montserrat, Quicksand, sans-serif"
    fontSize: "2.625rem"
    fontWeight: 800
    lineHeight: 1.25
    letterSpacing: "-0.01em"
  headline:
    fontFamily: "Montserrat, Quicksand, sans-serif"
    fontSize: "2rem"
    fontWeight: 700
    lineHeight: 1.3
  title:
    fontFamily: "Quicksand, -apple-system, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 700
    lineHeight: 1.4
  body:
    fontFamily: "Quicksand, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "0.9375rem"
    fontWeight: 500
    lineHeight: 1.6
  label:
    fontFamily: "Quicksand, sans-serif"
    fontSize: "0.8125rem"
    fontWeight: 700
    letterSpacing: "0.04em"
  script:
    fontFamily: "Pacifico, cursive"
  secondary:
    fontFamily: "Source Sans 3, Source Sans Pro, sans-serif"
  icons:
    fontFamily: "Fontawesome, Icomoon, Fontello"
rounded:
  sm: "4px"
  md: "6px"
  lg: "8px"
  xl: "12px"
  card: "16px"
  card-lg: "20px"
  pill: "9999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "16px"
  lg: "24px"
  xl: "32px"
  xxl: "48px"
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.text-light}"
    rounded: "{rounded.md}"
    padding: "12px 28px"
  button-primary-hover:
    backgroundColor: "{colors.primary-dark}"
  button-gold:
    backgroundColor: "{colors.primary-light}"
    textColor: "{colors.text-light}"
    rounded: "{rounded.pill}"
    padding: "10px 24px"
  card-product:
    backgroundColor: "{colors.surface-white}"
    rounded: "{rounded.lg}"
    padding: "20px"
---

# Design System: LalaCoffee

## Overview

**Creative North Star: "The Roasted Sanctuary — Chốn Thư Thái Cà Phê Mộc"**

Hệ thống thiết kế của LalaCoffee phản ánh tinh thần mộc mạc, tĩnh tại và chân thực từ những hạt cà phê rang xay nguyên bản của vùng đất đỏ bazan. Trải nghiệm thị giác kết hợp giữa bề mặt sáng màu kem yến mạch (`#faf7f2`), sắc gỗ ấm nồng (`#8b572a`) và điểm xuyết ánh vàng mật ong hổ phách (`#c19942`), tạo ra không gian thưởng thức đồ uống an yên và đáng tin cậy.

**Key Characteristics:**
- Tông màu cà phê rang mộc tự nhiên, tránh hoàn toàn màu xám nguội nhân tạo.
- Tương phản thị giác nghiêm ngặt: mọi chữ số giá tiền và thông tin đặt món phải đạt chuẩn WCAG AA (≥ 4.5:1).
- Chuyển động cơ học hữu cơ, nhã nhặn, tôn trọng sự nhạy cảm chuyển động của người dùng.
- Vùng chạm cảm ứng rộng rãi (tối thiểu 44px trên di động) cho hành vi đặt món thoải mái.

## Colors

Bảng màu ấm áp bắt nguồn từ các giai đoạn rang cà phê (Light, Medium, Dark Roast) và màu kem sữa tự nhiên.

### Primary
- **Deep Roast Coffee** (`#8b572a`): Màu nhận diện chính cho các nút hành động (CTA), tiêu đề nổi bật và nhãn giá tiền trên nền sáng. Đảm bảo độ tương phản 5.0:1 trên nền trắng.
- **Espresso Dark** (`#5c3716`): Trạng thái hover/active của nút và các văn bản quan trọng.

### Secondary
- **Amber Gold** (`#c19942`): Màu vàng đồng làm điểm nhấn thương hiệu, huy hiệu đặc quyền (badge), đường kẻ trang trí và viền vi tế.
- **Warm Honey** (`#e4b95b`): Dành riêng cho nền tối (Dark banner / Header overlay) khi cần độ tương phản cao với nền cà phê đậm.

### Neutral
- **Oat Cream** (`#faf7f2`): Nền các khối nội dung thẻ, card sản phẩm và nền phụ toàn trang.
- **Pure White** (`#ffffff`): Nền card sản phẩm chính và nền trang đọc tin.
- **Espresso Black** (`#1d1313`): Nền footer và thanh trạng thái tối.
- **Charcoal Roast** (`#2b1e16`): Màu chữ chính cho toàn bộ hệ thống, ấm hơn màu đen thuần.
- **Muted Bean** (`#5f5b57`): Màu chữ phụ cho mô tả, kích thước, ghi chú (đạt tương phản ≥ 5.5:1 trên nền sáng).

### Named Rules
**The 4.5:1 Contrast Rule.** Không bao giờ dùng màu vàng kim `#e4b95b` hoặc `#c7a17a` làm màu chữ trên nền trắng hoặc nền kem. Giá tiền phải dùng `#8b572a` hoặc `#2b1e16`.

## Typography

**Display Font:** Montserrat (fallback: Arial, sans-serif)
**Body Font:** Quicksand (fallback: -apple-system, BlinkMacSystemFont, Segoe UI, sans-serif)

**Character:** Tiêu đề cô đọng, dứt khoát với Montserrat; nội dung mềm mại, thân thiện và ấm áp với Quicksand có nét bo tròn nhẹ nhàng.

### Hierarchy
- **Display** (800, clamp(2rem, 5vw, 2.625rem), 1.25): Dùng cho thông điệp chính tại Hero Banner.
- **Headline** (700, 2rem, 1.3): Tiêu đề từng phân mục lớn (Chọn theo gu, Thực đơn).
- **Title** (700, 1.25rem, 1.4): Tên sản phẩm, tên bài viết blog, tiêu đề đơn hàng.
- **Body** (500, 0.9375rem, 1.6): Nội dung mô tả, đoạn văn, độ dài dòng khuyên dùng từ 65–75 ký tự.
- **Label** (700, 0.8125rem, letter-spacing 0.04em, uppercase): Nhãn danh mục, trạng thái đơn, tag món mới.

## Layout

- **Container Model**: Hệ thống lưới 12 cột Bootstrap 5, độ rộng tối đa 1240px với đệm an toàn `clamp(16px, 3vw, 32px)`.
- **Rhythm**: Nhịp điệu khoảng cách theo bội số 8px (`8px`, `16px`, `24px`, `32px`, `48px`, `64px`). Khoảng cách trên tiêu đề luôn lớn hơn khoảng cách dưới tiêu đề ít nhất 1.5 lần.

## Elevation & Depth

LalaCoffee ưu tiên độ phẳng xúc giác (Tactile Flatness) kết hợp đổ bóng mềm mịn tự nhiên. Tránh hoàn toàn bóng màu xơ cứng hoặc bóng đổ không có độ mờ.

### Shadow Vocabulary
- **Card Rest** (`box-shadow: 0 2px 12px rgba(43, 30, 22, 0.06)`): Bề mặt thẻ sản phẩm ở trạng thái nghỉ.
- **Card Hover** (`box-shadow: 0 10px 28px rgba(43, 30, 22, 0.12)`): Trạng thái nhấc nhẹ khi di chuột, dịch chuyển `translateY(-4px)`.
- **Dropdown / Modal** (`box-shadow: 0 15px 35px rgba(0, 0, 0, 0.15)`): Bề mặt lớp phủ tài khoản và giỏ hàng.

## Shapes

- **Corner Radius**: Bo tròn 12px–16px cho thẻ sản phẩm, 8px cho ô nhập liệu, và bo tròn viên thuốc (pill - 9999px) cho các nút hành động nhỏ.
- **Form Controls**: Viền mảnh 1px màu `#e8dfd5`, khi focus chuyển sang viền `#8b572a` kèm vòng sáng mờ 3px `rgba(139, 87, 42, 0.15)`.

## Components

### Buttons
- **Primary Button**: Nền `#8b572a`, chữ `#ffffff`, bo tròn 6px, padding `12px 28px`. Hover đổi thành `#5c3716` và nhấc nhẹ.
- **Quantity Stepper Buttons**: Nút tròn đường kính 36px, viền mềm, căn giữa icon, vùng cảm ứng đảm bảo 44px.

### Product Card
- Nền trắng trên khung kem `#faf7f2`, ảnh món tỷ lệ 4:3 hoặc 1:1, hiệu ứng phóng to ảnh 1.04 khi hover với transition 0.4s ease.

## Do's and Don'ts

### Do:
- **Do** duy trì độ tương phản chữ luôn ≥ 4.5:1 so với nền trực tiếp.
- **Do** hỗ trợ đầy đủ phím Tab và phím Enter/Space cho mọi nút bấm.
- **Do** tôn trọng tùy chọn `@media (prefers-reduced-motion: reduce)`.

### Don't:
- **Don't** đưa bảng quản trị hay mã scaffolding vào các trang khách hàng.
- **Don't** sử dụng hộp thoại `alert()` của trình duyệt để thông báo nghiệp vụ; sử dụng toast hoặc SweetAlert2.
- **Don't** nạp các thư viện hiệu ứng trùng lặp (nhiều slider, nhiều lightbox).
