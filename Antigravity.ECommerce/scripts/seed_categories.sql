USE Antigravity_ECommerce;
GO

-- Xóa dữ liệu cũ nếu muốn làm sạch (Cẩn thận!)
-- DELETE FROM Categories;

-- 1. SEED PRODUCT CATEGORIES (CategoryType = 1)
INSERT INTO Categories (ParentId, Name, Slug, Description, CategoryType, Status, SortOrder, CreatedBy, CreatedAt)
VALUES 
(0, N'Điện thoại', 'dien-thoai', N'Các dòng điện thoại thông minh', 1, 1, 1, 'Admin', GETDATE()),
(0, N'Laptop', 'laptop', N'Máy tính xách tay chính hãng', 1, 1, 2, 'Admin', GETDATE()),
(0, N'Phụ kiện', 'phu-kien', N'Phụ kiện công nghệ', 1, 1, 3, 'Admin', GETDATE());

DECLARE @PhoneId INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Slug = 'dien-thoai' AND CategoryType = 1);
DECLARE @LaptopId INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Slug = 'laptop' AND CategoryType = 1);

INSERT INTO Categories (ParentId, Name, Slug, Description, CategoryType, Status, SortOrder, CreatedBy, CreatedAt)
VALUES 
(@PhoneId, N'iPhone', 'iphone', N'Điện thoại Apple iPhone', 1, 1, 1, 'Admin', GETDATE()),
(@PhoneId, N'Samsung', 'samsung', N'Điện thoại Samsung Galaxy', 1, 1, 2, 'Admin', GETDATE()),
(@LaptopId, N'Macbook', 'macbook', N'Laptop Apple Macbook', 1, 1, 1, 'Admin', GETDATE()),
(@LaptopId, N'Dell', 'dell', N'Laptop Dell chuyên nghiệp', 1, 1, 2, 'Admin', GETDATE());

-- 2. SEED NEWS CATEGORIES (CategoryType = 2)
INSERT INTO Categories (ParentId, Name, Slug, Description, CategoryType, Status, SortOrder, CreatedBy, CreatedAt)
VALUES 
(0, N'Tin tức Sự kiện', 'tin-su-kien', N'Cập nhật tin tức hot nhất', 2, 1, 1, 'Admin', GETDATE()),
(0, N'Khuyến mãi', 'khuyen-mai', N'Các chương trình ưu đãi', 2, 1, 2, 'Admin', GETDATE()),
(0, N'Đánh giá Công nghệ', 'reviews', N'Review các sản phẩm mới', 2, 1, 3, 'Admin', GETDATE());

-- 3. SEED MENU ITEMS (CategoryType = 0)
INSERT INTO Categories (ParentId, Name, Slug, LinkType, Url, MenuPosition, CategoryType, Status, SortOrder, CreatedBy, CreatedAt)
VALUES 
(0, N'Trang chủ', 'trang-chu', 2, '/', 'Header,Footer', 0, 1, 1, 'Admin', GETDATE()),
(0, N'Sản phẩm', 'san-pham', 2, '/san-pham.html', 'Header', 0, 1, 2, 'Admin', GETDATE()),
(0, N'Tin tức', 'tin-tuc', 2, '/tin-tuc.html', 'Header,Footer', 0, 1, 3, 'Admin', GETDATE()),
(0, N'Giới thiệu', 'gioi-thieu', 2, '/gioi-thieu.html', 'Header,Footer', 0, 1, 4, 'Admin', GETDATE()),
(0, N'Liên hệ', 'lien-he', 2, '/lien-he.html', 'Header,Footer', 0, 1, 5, 'Admin', GETDATE());

-- Thêm một số menu con (Cấp 2)
DECLARE @ProdMenuId INT = (SELECT TOP 1 CategoryId FROM Categories WHERE Slug = 'san-pham' AND CategoryType = 0);
INSERT INTO Categories (ParentId, Name, Slug, LinkType, Url, MenuPosition, CategoryType, Status, SortOrder, CreatedBy, CreatedAt)
VALUES 
(@ProdMenuId, N'Điện thoại hot', 'phone-hot', 2, '/dien-thoai.html', 'Header', 0, 1, 1, 'Admin', GETDATE()),
(@ProdMenuId, N'Laptop bán chạy', 'laptop-hot', 2, '/laptop.html', 'Header', 0, 1, 2, 'Admin', GETDATE());

GO
