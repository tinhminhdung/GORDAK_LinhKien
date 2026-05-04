# 🚫 QUY TẮC NGHIÊM NGẶT KHI CODE LẠI GIAO DIỆN (SEO PRESERVATION)

Tài liệu này quy định các quy tắc bắt buộc phải tuân thủ khi thay đổi hoặc xây dựng lại giao diện trang người dùng để đảm bảo không làm gãy hệ thống SEO và duy trì thứ hạng Google.

---

## 1. Cấu Trúc Layout Bắt Buộc (Layout Integrity)
Khi tạo Layout mới (ví dụ `_NewLayout.cshtml`), bạn **BẮT BUỘC** phải bao gồm 3 thành phần hạ tầng sau:

### Trong thẻ `<head>`:
```html
<head>
    <!-- 1. Bộ máy Meta, Canonical, OG và Analytics -->
    <partial name="_SeoHeader" />
    
    <!-- 2. Bộ máy tạo Dữ liệu cấu trúc Schema.org thông minh -->
    <partial name="_SeoSchema" />
    
    <!-- Các CSS của bạn -->
</head>
```

### Trước thẻ đóng `</body>`:
```html
<body>
    ...
    <!-- 3. Bộ máy hiệu suất: LazyLoad, Pre-fetch và Tracking Scripts -->
    <partial name="_SeoFooter" />
</body>
```

---

## 2. Quy Tắc Truyền Dữ Liệu SEO (Data Passing)
SEO hiện nay hoạt động dựa trên dữ liệu (`ViewData`). Khi code trang Chi tiết hoặc Danh mục, phải cung cấp đủ "nhiên liệu" cho bộ máy SEO:

### Đối với trang Sản phẩm (Product Detail):
```csharp
ViewData["Title"] = Model.SeoTitle ?? Model.Name;
ViewData["Description"] = Model.SeoDescription ?? Model.ShortDescription;
ViewData["Image"] = Model.MainImage;
ViewData["SchemaProduct"] = Model; // Bắt buộc để tạo Rich Snippets
```

### Đối với trang Tin tức (News Detail):
```csharp
ViewData["SchemaNews"] = Model; // Bắt buộc để tạo NewsArticle Schema
```

### Đối với Breadcrumbs (Đường dẫn):
Luôn truyền danh sách đường dẫn để Google hiển thị phân cấp:
```csharp
ViewData["SchemaBreadcrumbs"] = new List<BreadcrumbItem> {
    new BreadcrumbItem { Name = "Trang chủ", Url = "/" },
    new BreadcrumbItem { Name = "Tên chuyên mục", Url = "/url-chuyen-muc" }
};
```

---

## 3. SEO Hình Ảnh & Hiệu Suất (Image SEO)
Không sử dụng thẻ `<img>` thông thường. Phải tuân thủ quy tắc Lazy Loading:

*   **SAI:** `<img src="/path/to/image.jpg" alt="Description">`
*   **ĐÚNG:** `<img data-src="/path/to/image.jpg" class="lazy" alt="@@Model.Name">`

**Yêu cầu:**
1.  Phải có class `lazy`.
2.  Đường dẫn ảnh nằm trong `data-src`.
3.  **ALT TAG:** Luôn luôn phải có thuộc tính `alt`. Giá trị của `alt` nên là tên sản phẩm hoặc tiêu đề bài viết.

---

## 4. Cấu Trúc Heading (H1-H6 Hierarchy)
Google sử dụng Heading để hiểu nội dung chính.
1.  **H1:** Chỉ được phép có **DUY NHẤT MỘT** thẻ `<h1>` trên mỗi trang (Thường là tên sản phẩm hoặc tiêu đề bài viết).
2.  **Thứ tự:** Không được nhảy bậc (Ví dụ: Đang `<h2>` không được nhảy thẳng xuống `<h4>`).
3.  **Semantic HTML:** Sử dụng các thẻ `<main>`, `<article>`, `<section>`, `<aside>` đúng mục đích thay vì lạm dụng `<div>`.

---

## 5. URL & Routing (Không thay đổi)
Không bao giờ thay đổi cấu trúc URL trong `Program.cs` hoặc các thuộc tính `[Route]` trong Controller trừ khi có kế hoạch 301 Redirect.
*   Link sitemap: `domain.com/sitemap.xml`
*   Link robots: `domain.com/robots.txt`
*   Đuôi file: Ưu tiên giữ `.html` cho các trang nội dung/sản phẩm.

---

## 6. Lưu Ý Về JavaScript
1.  **Không chặn render:** Hạn chế nhúng các file JS nặng vào `<head>`.
2.  **Thư viện Core:** Luôn giữ jQuery ở đầu trang nếu các script khác phụ thuộc vào nó (như Cart Count), nhưng các script SEO nên là Vanilla JS để chạy độc lập.

---

## 7. Hiệu Suất & Trải Nghiệm (Performance & instant.page)
Website đã được tích hợp công nghệ **Instant Page** (Pre-fetching) để tăng tốc độ phản hồi cực nhanh:

1.  **Cơ chế:** Khi người dùng di chuột qua một liên kết (~65ms), hệ thống sẽ tự động tải trước nội dung trang đó.
2.  **Quy tắc code:** 
    *   Không được gỡ bỏ dòng script `instant.page` trong `_SeoFooter.cshtml`.
    *   Đối với các liên kết không muốn tải trước (ví dụ: Log out, Delete, hoăc các link gây tốn resource), hãy thêm thuộc tính `data-no-instant` vào thẻ `<a>`.
    *   Đảm bảo các link điều hướng là thẻ `<a>` chuẩn, không dùng `window.location` qua JS để bộ máy này có thể nhận diện được.

---

> [!IMPORTANT]
> Việc vi phạm bất kỳ quy tắc nào ở trên có thể dẫn đến việc mất Rich Snippets (Sao đánh giá, giá tiền trên Google) hoặc rớt hạng từ khóa ngay lập tức.

---

## 8. Nguyên Tắc Bất Di Bất Dịch Về Font Chữ (UTF-8 Encoding)
1. **Luôn bảo toàn bộ mã UTF-8**: Mọi file `.cs`, `.cshtml`, `.js`, `.css` v.v. khi được lưu, chỉnh sửa, hoặc tạo mới đều phải giữ nguyên cấu trúc mã hóa UTF-8.
2. **Tuyệt đối không làm hỏng tiếng Việt**: Hệ thống từng bị lỗi Mojibake (chữ biến thành `ThÆ° viá»‡n`, `?`...). Bất cứ khi nào copy/paste hoặc refactor code, nếu phát hiện chữ tiếng Việt, **bắt buộc** phải giữ nguyên vẹn dấu tiếng Việt.
3. Đây là nguyên tắc cốt lõi sẽ luôn được lưu trong "bộ nhớ" (Knowledge Items) của AI để áp dụng cho tất cả các phiên làm việc về sau mà không cần bạn phải nhắc lại.
