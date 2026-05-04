-- Xóa dữ liệu cũ của Menu (Type 0) để tránh trùng lặp khi demo
DELETE FROM Categories WHERE CategoryType = 0;

-- Thêm dữ liệu Menu mẫu đa cấp (Unicode UTF-8)
-- Level 1
INSERT INTO Categories (Name, Slug, ParentId, CategoryType, LinkType, Url, Status, MenuPosition, SortOrder, CreatedAt) VALUES
(N'Trang chủ', 'trang-chu', 0, 0, 2, '/', 1, 'Header,Footer', 1, GETDATE()),
(N'Sản phẩm', 'san-pham', 0, 0, 2, '/Product/Search', 1, 'Header', 2, GETDATE()),
(N'Tin tức', 'tin-tuc', 0, 0, 2, '/News', 1, 'Header,Footer', 3, GETDATE()),
(N'Giới thiệu', 'gioi-thieu', 0, 0, 2, '/trang/gioi-thieu.html', 1, 'Header,Footer', 4, GETDATE()),
(N'Liên hệ', 'lien-he', 0, 0, 2, '/trang/lien-he.html', 1, 'Header,Footer', 5, GETDATE());

-- Level 2 (Dưới 'Sản phẩm')
DECLARE @ProdId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'san-pham' AND CategoryType = 0);
INSERT INTO Categories (Name, Slug, ParentId, CategoryType, LinkType, Url, Status, MenuPosition, SortOrder, CreatedAt) VALUES
(N'Điện thoại thông minh', 'dien-thoai', @ProdId, 0, 2, '/danh-muc/dien-thoai.html', 1, 'Header', 1, GETDATE()),
(N'Máy tính xách tay', 'lap-top', @ProdId, 0, 2, '/danh-muc/lap-top.html', 1, 'Header', 2, GETDATE()),
(N'Phụ kiện công nghệ', 'phu-kien', @ProdId, 0, 2, '/danh-muc/phu-kien.html', 1, 'Header', 3, GETDATE());

-- Level 2 (Dưới 'Tin tức')
DECLARE @NewsId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'tin-tuc' AND CategoryType = 0);
INSERT INTO Categories (Name, Slug, ParentId, CategoryType, LinkType, Url, Status, MenuPosition, SortOrder, CreatedAt) VALUES
(N'Đánh giá sản phẩm', 'review-sp', @NewsId, 0, 2, '/tin-tuc-dm/review-sp.html', 1, 'Header', 1, GETDATE()),
(N'Mẹo công nghệ', 'meo-vat', @NewsId, 0, 2, '/tin-tuc-dm/meo-vat.html', 1, 'Header', 2, GETDATE());

-- Level 3 (Dưới 'Máy tính xách tay')
DECLARE @LaptopId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'lap-top' AND CategoryType = 0);
INSERT INTO Categories (Name, Slug, ParentId, CategoryType, LinkType, Url, Status, MenuPosition, SortOrder, CreatedAt) VALUES
(N'Laptop Gaming', 'laptop-gaming', @LaptopId, 0, 2, '/danh-muc/laptop-gaming.html', 1, 'Header', 1, GETDATE()),
(N'Macbook Air/Pro', 'macbook', @LaptopId, 0, 2, '/danh-muc/macbook.html', 1, 'Header', 2, GETDATE());

-- Cập nhật lại Cache sau khi nạp data
-- Trong thực tế sẽ chạy code xóa cache, ở đây SQL chỉ nạp data.
GO
